using app.ViewModel;
using app.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;



namespace app.Pages.Profile
{

    public class ProfileModel : PageModel
    {
        private readonly ILogger<ProfileModel> _logger;
        private readonly IHttpClientFactory _clientFactory;
        public ProfileViewModel Profile { get; set; } = new ProfileViewModel();
        public bool IsOwnProfile { get; set; }
        public bool IsFriend { get; set; }

        public ProfileModel(ILogger<ProfileModel> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> OnGetAsync(string? userId)
        {
            _logger.LogInformation("ProfileModel.OnGetAsync fue llamado.");

            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Login");

            var token = Request.Cookies["jwt_token"];
            var currentUserId = Request.Cookies["user_id"];

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(currentUserId))
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

            var targetUserId = string.IsNullOrEmpty(userId) ? currentUserId : userId;

            var response = await client.GetAsync($"/profiles/profiles/{targetUserId}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"No se pudo cargar el perfil del usuario con ID: {targetUserId}");
                return NotFound();
            }
            var profile = await response.Content.ReadFromJsonAsync<ProfileInProfile>();
            Console.WriteLine("####RESPUESTA DE PROFILE:   " + profile.Id);
            if (profile == null)
                return NotFound();

            Profile.Id = profile.Id;

            // Obtener posts del usuario
            var postsResponse = await client.GetAsync($"/posts/posts/user/{targetUserId}");
            var postViewModels = new List<PostViewModel>();

            if (postsResponse.IsSuccessStatusCode)
            {
                var posts = await postsResponse.Content.ReadFromJsonAsync<List<UserPostDto>>();
                if (posts != null)
                {
                    foreach (var post in posts)
                    {
                        // Obtener número de likes por post
                        int likes = 0;
                        try
                        {
                            var likesResponse = await client.GetAsync($"/posts/posts/{post.PostId}/likes-count");
                            if (likesResponse.IsSuccessStatusCode)
                            {
                                using var stream = await likesResponse.Content.ReadAsStreamAsync();
                                using var doc = await JsonDocument.ParseAsync(stream);
                                if (doc.RootElement.TryGetProperty("like_count", out var likeCountElement) &&
                                    likeCountElement.TryGetInt32(out var likeCount))
                                {
                                    likes = likeCount;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"No se pudo obtener likes para post {post.PostId}");
                        }

                        postViewModels.Add(new PostViewModel
                        {
                            Id = post.PostId,
                            ImageUrl = post.UrlMedia,
                            Caption = post.Descripcion,
                            Likes = likes,
                            PostedAt = post.FechaPublicacion
                        });
                    }
                }
            }

            else
            {
                _logger.LogWarning($"No se pudieron obtener los posts del usuario con ID: {targetUserId}");
            }


            Profile.Username = profile.Username;
            Profile.Matricula = profile.Enrollment;
            Profile.ProfilePictureUrl = profile.Photo;
            Profile.Followers = profile.FollowerCount ?? 0;
            Profile.Posts = postViewModels;

            IsOwnProfile = targetUserId == currentUserId;

            if (!IsOwnProfile)
            {
                try
                {
                    var responseIsFollowing = await client.GetAsync($"/follow/is_following/{currentUserId}/{userId}");
                    if (!responseIsFollowing.IsSuccessStatusCode)
                    {
                        _logger.LogWarning($"No se pudo verificar si el usuario sigue al perfil {userId}: {responseIsFollowing.StatusCode}");
                        IsFriend = false;
                    }
                    else
                    {
                        var json = await responseIsFollowing.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<IsFollowingResponse>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        IsFriend = result?.IsFollowing ?? false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al verificar seguimiento.");
                    IsFriend = false;
                }
            }

            return Page();
        }

        private class IsFollowingResponse
        {
            [JsonPropertyName("is_following")]
            public bool IsFollowing { get; set; }
        }

        public async Task<IActionResult> OnPostFollowAsync(string FollowedUserId, bool CurrentlyFollowing)
        {
            var token = Request.Cookies["jwt_token"];
            var currentUserId = Request.Cookies["user_id"];

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogWarning("Token JWT o user_id no encontrado en las cookies.");
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

            HttpResponseMessage response;

            try
            {
                if (CurrentlyFollowing)
                {
                    Console.WriteLine($"Intentando hacer UNFOLLOW a {FollowedUserId} desde {currentUserId}");
                    response = await client.DeleteAsync($"/follow/unfollow/{currentUserId}/{FollowedUserId}");
                }
                else
                {
                    Console.WriteLine($"Intentando hacer FOLLOW a {FollowedUserId} desde {currentUserId}");
                    response = await client.PostAsync($"/follow/follow/{currentUserId}/{FollowedUserId}", null);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error al {(CurrentlyFollowing ? "dejar de seguir" : "seguir")} al usuario: {error}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al {(CurrentlyFollowing ? "unfollow" : "follow")}.");
            }

            return RedirectToPage(new { userId = FollowedUserId });
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

    }
}

