using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using app.ViewModel;
using app.DTO;

namespace app.Pages.Feed
{
    public class FeedModel : PageModel
    {
        public List<PostViewModel> Posts { get; set; } = new List<PostViewModel>();

        public IActionResult OnGet()
        {
            var usuario_token = "123";

            if (string.IsNullOrEmpty(usuario_token))
            {
                return RedirectToPage("/Login");
            }

            Posts = new List<PostViewModel>
            {
                new PostViewModel
                {
                    ImageUrl = "/images/DojaCat.jpg",
                    Caption = "¡Hoy cociné algo rico con Kirito! 😋🍳"
                },
                new PostViewModel
                {
                    ImageUrl = "/images/Perfil.jpg",
                    Caption = "Miau"
                }
            };

            return Page();
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