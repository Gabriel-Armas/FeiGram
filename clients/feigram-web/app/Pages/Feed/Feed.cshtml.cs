using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using app.ViewModel;
using app.DTO;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;



namespace app.Pages.Feed
{
    public class FeedModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IWebHostEnvironment _env;
        public List<ProfileFollowingViewModel> Amigos { get; set; } = new();
        public List<PostViewModel> PostCompletos { get; set; } = new();
        private readonly ILogger<FeedModel> _logger;
        private readonly LikesService _likesService;
        private readonly PostService _postService;
        private readonly ProfileService _profileService;




        //public List<PostViewModel> Posts { get; set; } = new();

        public FeedModel(IHttpClientFactory clientFactory, IWebHostEnvironment env, ILogger<FeedModel> logger, LikesService likesService, PostService postService, ProfileService profileService)
        {
            _clientFactory = clientFactory;
            _env = env;
            _logger = logger;
            _likesService = likesService;
            _postService = postService;
            _profileService = profileService;
        }


        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("FeedModel.OnGetAsync fue llamado.");

            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Login");

            var token = Request.Cookies["jwt_token"];
            var userId = Request.Cookies["user_id"];

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No se encontró el token JWT o el user_id.");
                return RedirectToPage("/Login");
            }

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://feigram-nginx")
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Obtener recomendaciones del feed
                var responseMessage = await client.GetAsync("/posts/posts/recent");

                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error al obtener recomendaciones: {responseMessage.StatusCode}");
                    return Page();
                }

                var json = await responseMessage.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<PostAgrupadoDTO>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var publicaciones = result ?? new List<PostAgrupadoDTO>();

                foreach (var p in publicaciones)
                {
                    var profileResponse = await client.GetAsync($"profiles/profiles/{p.IdUsuario}");
                    if (!profileResponse.IsSuccessStatusCode) continue;

                    var profileJson = await profileResponse.Content.ReadAsStringAsync();
                    var userProfile = JsonSerializer.Deserialize<ProfileDTO>(profileJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    bool isLiked = await _likesService.CheckIfUserLikedPostAsync(userId, p.PostId.ToString());
                        PostCompletos.Add(new PostViewModel
                        {
                            Id = p.PostId,
                            Username = userProfile?.Username,
                            Description = p.Descripcion,
                            UserProfileImage = userProfile?.Photo,
                            Imagenes = p.Imagenes,
                            TimeAgo = GetTimeAgo(p.FechaPublicacion),
                            Likes = p.Likes,
                            Comentarios = p.Comentarios,
                            IsLiked = isLiked
                        });   
                }

                // Obtener lista de amigos
                var amigosResponse = await client.GetAsync($"/follow/following/{userId}");
                if (!amigosResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No se pudo obtener la lista de amigos.");
                }
                else
                {
                    var amigosJson = await amigosResponse.Content.ReadAsStringAsync();
                    var amigosResult = JsonSerializer.Deserialize<FollowingResponse>(amigosJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    Amigos = new List<ProfileFollowingViewModel>();
                    foreach (var amigoId in amigosResult?.Following ?? Enumerable.Empty<string>())
                    {
                        var perfilAmigoResponse = await client.GetAsync($"profiles/profiles/{amigoId}");
                        if (!perfilAmigoResponse.IsSuccessStatusCode) continue;

                        var perfilAmigoJson = await perfilAmigoResponse.Content.ReadAsStringAsync();
                        var perfilAmigo = JsonSerializer.Deserialize<ProfileDTO>(perfilAmigoJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (perfilAmigo != null)
                        {
                            Amigos.Add(new ProfileFollowingViewModel
                            {
                                Id = perfilAmigo.Id,
                                Username = perfilAmigo.Username,
                                ProfileImage = perfilAmigo.Photo
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llamar a /feed/posts/recommendations o /follow/following");
            }

            _logger.LogInformation($"Se cargaron {PostCompletos.Count} posts en el feed.");
            return Page();
        }

        private string GetTimeAgo(DateTime fecha)
        {
            var diff = DateTime.UtcNow.ToLocalTime() - fecha.ToLocalTime();

            if (diff.TotalMinutes < 60)
                return $"Hace {Math.Floor(diff.TotalMinutes)} minutos";
            else if (diff.TotalHours < 24)
                return $"Hace {Math.Floor(diff.TotalHours)} horas";
            else
                return $"Hace {Math.Floor(diff.TotalDays)} días";
        }


        public async Task<IActionResult> OnPostNuevoPostAsync(IFormFile imagen, string description, string fechaPublicacion)
        {
            if (imagen == null || imagen.Length == 0)
            {
                return new JsonResult(new { success = false, message = "Debes seleccionar una imagen." });
            }

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://feigram-nginx")
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Request.Cookies["jwt_token"]);

            string imageUrl;

            try
            {
                using var imageStream = imagen.OpenReadStream();
                var imageContent = new StreamContent(imageStream);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue(imagen.ContentType);

                using var uploadContent = new MultipartFormDataContent();
                uploadContent.Add(imageContent, "file", imagen.FileName);

                var uploadResponse = await client.PostAsync("/posts/upload-image", uploadContent);
                if (!uploadResponse.IsSuccessStatusCode)
                {
                    return new JsonResult(new { success = false, message = "Error al subir la imagen al servidor." });
                }

                var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<ImageUploadResult>();
                if (uploadResult == null || string.IsNullOrEmpty(uploadResult.Url))
                {
                    return new JsonResult(new { success = false, message = "Respuesta inválida al subir imagen." });
                }

                imageUrl = uploadResult.Url;
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error al subir la imagen: " + ex.Message });
            }

            try
            {
                var postData = new
                {
                    descripcion = description,
                    url_media = imageUrl,
                    fechaPublicacion = DateTime.Parse(fechaPublicacion) // Usa la misma fecha fija
                };

                var postJson = new StringContent(
                    JsonSerializer.Serialize(postData),
                    Encoding.UTF8,
                    "application/json"
                );

                var postResponse = await client.PostAsync("/posts/posts", postJson);

                if (!postResponse.IsSuccessStatusCode)
                {
                    return new JsonResult(new { success = false, message = "Error al crear la publicación." });
                }

                return new JsonResult(new { success = true, message = "Publicación creada correctamente." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error al crear el post: " + ex.Message });
            }
        }

        public async Task<IActionResult> OnPostLikeAsync(Like like)
        {
            if (like == null || string.IsNullOrEmpty(like.PostId) || string.IsNullOrEmpty(like.UserId))
                return BadRequest(new { success = false, message = "Datos inválidos." });

            Console.WriteLine($"✅ OnPostLikeAsync recibió Like: postId={like.PostId}, userId={like.UserId}");

            var token = Request.Cookies["jwt_token"];
            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            _likesService.SetBearerToken(token);

            var createdLike = await _likesService.CreateLikeAsync(like);
            if (createdLike == null)
                return StatusCode(500, "Error al crear el like.");

            return new JsonResult(new { success = true, message = "Like registrado correctamente." });
        }

        // public async Task<IActionResult> OnPostLikeAsync()
        // {
        //     var postId = Request.Form["PostId"];
        //     var userId = Request.Form["UserId"];

        //     Console.WriteLine($"✨ (Fake) Like recibido para postId={postId}, userId={userId}");

        //     if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(userId))
        //         return new JsonResult(new { success = false, message = "Datos inválidos." }) { StatusCode = 400 };

        //     var newLikeCount = 1;

        //     return new JsonResult(new { success = true, message = "Like registrado (fake).", likesCount = newLikeCount });
        // }


        public async Task<IActionResult> OnGetComentariosAsync(int postId)
        {
            try
            {
                var token = HttpContext.Request.Cookies["jwt_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    _profileService.SetBearerToken(token);
                    _postService.SetBearerToken(token);
                }
                else
                {
                    _logger.LogWarning("No se encontró token JWT para hacer peticiones autorizadas");
                }

                var comments = await _postService.GetCommentsAsync(postId);

                var comentariosConNombre = new List<object>();

                foreach (var comment in comments)
                {
                    var profile = await _profileService.GetProfileByIdAsync(comment.user_id);

                    comentariosConNombre.Add(new
                    {
                        Text = comment.text_comment,
                        UserId = comment.user_id,
                        Username = profile?.Username ?? "Desconocido"
                    });
                }

                return new JsonResult(comentariosConNombre);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener comentarios: {ex.Message}");
                return StatusCode(500, "Error al obtener comentarios.");
            }
        }

        

        public async Task<IActionResult> OnPostCommentAsync()
{
    var postId = Request.Form["PostId"];
    var commentText = Request.Form["CommentText"];

    if (string.IsNullOrWhiteSpace(commentText))
        return RedirectToPage();  // En caso de comentario vacío, simplemente vuelve al Feed

    var token = HttpContext.Request.Cookies["jwt_token"];
    string? userId = !string.IsNullOrEmpty(token)
        ? ObtenerUserIdDesdeToken(token)
        : null;

    var comment = new CommentPost
    {
        PostId = postId!,
        TextComment = commentText!,
        CreatedAt = DateTime.UtcNow,
        UserId = userId
    };

    try
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://feigram-nginx")
        };

        var response = await client.PostAsJsonAsync("/comments/comments", comment);

        if (!response.IsSuccessStatusCode)
        {
            return RedirectToPage(); 
        }

        return RedirectToPage();
    }
    catch (Exception)
    {
        return RedirectToPage();
    }
}



        public static string ObtenerUserIdDesdeToken(string jwtToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);
            return token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? throw new Exception("No se pudo extraer el userId");
        }

        public async Task<IActionResult> OnGetBuscarAmigosAsync(string nombre)
        {
            try
            {
                Console.WriteLine("ENTRE AL METODO");
                var token = Request.Cookies["jwt_token"];
                if (string.IsNullOrEmpty(token)) return Unauthorized();
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri("https://feigram-nginx")
                };

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"/profiles/profiles/search/{nombre}");
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine(">>> JSON RECIBIDO:");
                Console.WriteLine(json);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(">>> Status code: " + (int)response.StatusCode);
                    return StatusCode((int)response.StatusCode, json);
                }

                var results = JsonSerializer.Deserialize<List<ProfileWithFollowerCount>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new();

                return new JsonResult(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 EXCEPCIÓN:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, $"Excepción: {ex.Message}");
            }
        }






        public class CommentInput
        {
            public string PostId { get; set; }
            public string CommentText { get; set; }
        }
    }

    public class PostAgrupadoDTO
    {
        [JsonPropertyName("post_id")]
        public int PostId { get; set; }
        [JsonPropertyName("id_usuario")]
        public string IdUsuario { get; set; }
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }
        [JsonPropertyName("fechaPublicacion")]
        public DateTime FechaPublicacion { get; set; }
        [JsonPropertyName("imagenes")]
        public List<string> Imagenes { get; set; }
        [JsonPropertyName("comentarios")]
        public int Comentarios { get; set; }
        [JsonPropertyName("likes")]
        public int Likes { get; set; }
    }



    
    public class PostViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string TimeAgo { get; set; }
        public string Description { get; set; }
        public string UserProfileImage { get; set; }
         public List<string> Imagenes { get; set; } = new();
        public int Comentarios { get; set; }
        public int Likes { get; set; }
        public bool IsLiked { get; set; }
    }
}