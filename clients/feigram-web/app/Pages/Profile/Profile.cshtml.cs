using app.ViewModel;
using app.DTO;
using System.Linq;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace app.Pages.Profile
{

    public class ProfileModel : PageModel
    {
        public ProfileViewModel Profile { get; set; } = new ProfileViewModel();

        public async Task<IActionResult> OnGetAsync()
        {
            // Aquí simulas si hay token (luego puedes cambiarlo por HttpContext.Session o cookies)
            var usuario_token = "123";

            if (string.IsNullOrEmpty(usuario_token))
            {
                return RedirectToPage("/Login");
            }

            Profile = new ProfileViewModel
            {
                ProfilePictureUrl = "/images/default-avatar.png",
                Username = "YaelHero",
                Matricula = "S12345678",
                Posts = new List<PostViewModel>
            {
                new PostViewModel { ImageUrl = "/images/logo-fei.png", Caption = "¡Mi primer post kawaii!" },
                new PostViewModel { ImageUrl = "/images/post2.png", Caption = "Listo para el backend uwu~" },
                new PostViewModel { ImageUrl = "/images/post3.png", Caption = "Progresando como un prota de shonen 💪" }
            }
            };

            return Page();
        }


        private List<PostViewModel> MapPosts(List<PostDTO> postDtos)
        {
            return postDtos.Select(p => new PostViewModel
            {
                ImageUrl = p.ImageUrl,
                Caption = p.Caption
            }).ToList();
        }

        public IActionResult OnGetPostModal(int postId)
        {
            var post = GetPostById(postId);

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
        }

        // Simulación de un método que obtiene un post por ID, en un caso real iría a la API
        private PostDTO GetPostById(int id)
        {
            return new PostDTO
            {
                Id = id,
                ImageUrl = "/images/logo-fei.png",
                Caption = "Post de prueba~ nyaa~ 💖",
                Likes = 12,
                Comments = new List<CommentDTO>
                {
                    new CommentDTO { Author = "Sakura", Text = "Kawaii~!" },
                    new CommentDTO { Author = "Naruto", Text = "¡Dattebayo!" }
                }
            };
        }
    }
}
