using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using app.ViewModel;
using app.DTO;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text;


namespace app.Pages.Feed
{
    public class FeedModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IWebHostEnvironment _env;
        public List<PostViewModel> PostCompletos { get; set; } = new();
        private readonly ILogger<FeedModel> _logger;
        private readonly LikesService _likesService;
        private readonly PostService _postService;



        //public List<PostViewModel> Posts { get; set; } = new();

        public FeedModel(IHttpClientFactory clientFactory, IWebHostEnvironment env, ILogger<FeedModel> logger, LikesService likesService, PostService postService)
        {
            _clientFactory = clientFactory;
            _env = env;
            _logger = logger;
            _likesService = likesService;
            _postService = postService;
        }


        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("FeedModel.OnGetAsync fue llamado.");

            // Verifica si el usuario está autenticado
            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Login");

            // Obtiene el token JWT de la cookie
            var token = Request.Cookies["jwt_token"];
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No se encontró el token JWT en la cookie.");
                return RedirectToPage("/Login");
            }

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://feigram-nginx");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var responseMessage = await client.GetAsync("/feed/posts/recommendations");

                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error al obtener recomendaciones: {responseMessage.StatusCode}");
                    return Page(); // o RedirectToPage("/Error") si lo deseas
                }

                var json = await responseMessage.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta del feed: " + json);
                var result = JsonSerializer.Deserialize<RecommendationResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                _logger.LogInformation("Respuesta del feed: " + json);

                var recomendaciones = result?.Posts ?? new List<PostDTO>();


                foreach (var p in recomendaciones)
                {
                    var profileResponse = await client.GetAsync($"profiles/profiles/{p.IdUsuario}");
                    if (!profileResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning($"No se pudo obtener perfil de usuario {p.IdUsuario}");
                        continue;
                    }

                    var profileJson = await profileResponse.Content.ReadAsStringAsync();
                    var userProfile = JsonSerializer.Deserialize<ProfileDTO>(profileJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });


                    PostCompletos.Add(new PostViewModel
                    {
                        Id = p.PostId,
                        Username = userProfile.Username,
                        Description = p.Descripcion,
                        UserProfileImage = userProfile.Photo,
                        PostImage = p.UrlMedia,
                        TimeAgo = GetTimeAgo(p.FechaPublicacion),
                        Likes = p.Likes,
                        Comentarios = p.Comentarios,
                        IsLiked = true
                        //IsLiked = await likesService.CheckIfUserLikedPostAsync(_me.Id, p.PostId.ToString())
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llamar a /feed/recommendations");
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


        public async Task<IActionResult> OnPostNuevoPostAsync(IFormFile imagen, string description)
        {
            if (imagen == null || imagen.Length == 0)
            {
                return new JsonResult(new { success = false, message = "Debes seleccionar una imagen." });
            }

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://feigram-nginx");
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
                    fechaPublicacion = DateTime.UtcNow
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

        public async Task<IActionResult> OnPostLikeAsync([FromBody] Like like)
        {
            Console.WriteLine("✅ Entró a OnPostLikeAsync");
            if (like == null)
                return BadRequest("No se pudo deserializar el Like");

            Console.WriteLine($"Id: {like.Id}, PostId: {like.PostId}, UserId: {like.UserId}");

            if (string.IsNullOrEmpty(like?.UserId) || string.IsNullOrEmpty(like?.PostId))
            {
                return new JsonResult(new { success = false, message = "Datos inválidos." }) { StatusCode = 400 };
            }

            var result = await _likesService.CreateLikeAsync(like);
            if (result != null)
            {
                return new JsonResult(new { success = true, message = "Like registrado correctamente." });
            }

            return new JsonResult(new { success = false, message = "No se pudo registrar el like." }) { StatusCode = 500 };
        }

        public async Task<IActionResult> OnGetComentariosAsync(int postId)
        {
            try
            {
                var token = HttpContext.Request.Cookies["jwt_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    _postService.SetBearerToken(token);
                }
                else
                {
                    _logger.LogWarning("No se encontró token JWT para hacer peticiones autorizadas");
                }

                var comments = await _postService.GetCommentsAsync(postId); // tu método del servicio
                return new JsonResult(comments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener comentarios: {ex.Message}");
                return StatusCode(500, "Error al obtener comentarios.");
            }
        }




    }
    

    
    public class PostViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string TimeAgo { get; set; }
        public string Description { get; set; }
        public string UserProfileImage { get; set; }
        public string PostImage { get; set; }
        public int Comentarios { get; set; }
        public int Likes { get; set; }
        public bool IsLiked { get; set; }
    }
}