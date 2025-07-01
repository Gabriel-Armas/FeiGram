using app.ViewModel;
using app.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;


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
            if (profile == null)
                return NotFound();

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


             Profile = new ProfileViewModel
            {
                Username = profile.Username,
                Matricula = profile.Enrollment,
                ProfilePictureUrl = profile.Photo,
                Followers = profile.FollowerCount ?? 0,
                Posts = postViewModels
            };

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

    /*
                private List<PostViewModel> MapPosts(List<PostDTO> postDtos)
                {
                    return postDtos.Select(p => new PostViewModel
                    {
                        ImageUrl = p.UrlMedia,
                        Caption = p.Descripcion
                    }).ToList();
                }


                public IActionResult OnGetPostModal(int postId)
                {
                    var postDto = GetPostById(postId); // Aquí simulas como si lo trajeras de la API

                    if (postDto == null)
                    {
                        return NotFound();
                    }

                    var model = new PostPartialViewModel
                    {
                        Id = postDto.PostId,
                        ImageUrl = postDto.UrlMedia,
                        Caption = postDto.Descripcion,
                        Likes = postDto.Likes,
                    };

                    return Partial("PostPartial", model);
                }


                private PostDTO GetPostById(int id)
                {
                    return new PostDTO
                    {

                        Id = id,
                        ImageUrl = $"/images/post{id}.png",
                        Caption = $"Post número {id} ~✨",
                        Likes = id * 10,
                        Comments = new List<CommentDTO>
                        {
                            new CommentDTO { Author = "Kokomi", Text = $"¡Comentario mágico para el post {id}!" },
                            new CommentDTO { Author = "Zelda", Text = $"Sabio consejo para el héroe del post {id}~" }
                        }

                    };
                }
                */

        public async Task<IActionResult> OnPostFollowAsync()
        {
            // Aquí deberías llamar al API de follow/unfollow (simulado)
            IsFriend = !IsFriend;
            return RedirectToPage(new { userId = Profile.Matricula });
        }

    }
}
