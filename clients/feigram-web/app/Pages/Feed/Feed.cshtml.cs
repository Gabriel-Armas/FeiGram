using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using app.ViewModel;
using app.DTO;
using System.Text.Json;
using System.Net.Http.Headers;

namespace app.Pages.Feed
{
    public class FeedModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IWebHostEnvironment _env;
        public List<PostViewModel> PostCompletos { get; set; } = new();
        private readonly ILogger<FeedModel> _logger;


        //public List<PostViewModel> Posts { get; set; } = new();

        public FeedModel(IHttpClientFactory clientFactory, IWebHostEnvironment env, ILogger<FeedModel> logger)
        {
            _clientFactory = clientFactory;
            _env = env;
            _logger = logger;
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
    
        /*public async Task<IActionResult> OnGetPostModalAsync(int postId)
        {
            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri("https://feigram-nginx");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Request.Cookies["jwt_token"]);

            var post = await client.GetFromJsonAsync<PostDTO>($"api/posts/{postId}");
            if (post == null) return NotFound();

            var model = new PostPartialViewModel
            {
                Id = post.Id,
                ImageUrl = post.ImageUrl,
                Caption = post.Caption,
                Likes = post.Likes,
                Comments = post.Comments.Select(c => new CommentViewModel
                {
                    User = c.Author,
                    Text = c.Text
                }).ToList()
            };

            return Partial("PostPartial", model);
        }*/
        /*
        public async Task<IActionResult> OnPostNuevoPostAsync(IFormFile imagen, string description)
        {
            if (imagen == null || imagen.Length == 0)
            {
                TempData["Error"] = "Debes seleccionar una imagen.";
                return RedirectToPage();
            }

            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri("https://feigram-nginx");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Request.Cookies["jwt_token"]);

            using var content = new MultipartFormDataContent();
            using var fileStream = imagen.OpenReadStream();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(imagen.ContentType);

            content.Add(streamContent, "imagen", imagen.FileName);
            content.Add(new StringContent(description ?? ""), "description");

            var response = await client.PostAsync("posts/posts", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Ocurrió un error al subir la publicación.";
            }

            return RedirectToPage();
        }
    }*/
    
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
}