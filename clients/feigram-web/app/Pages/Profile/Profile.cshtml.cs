using app.ViewModel;
using app.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace app.Pages.Profile
{

    public class ProfileModel : PageModel
    {
        public ProfileViewModel Profile { get; set; } = new ProfileViewModel();
        
        public async Task<IActionResult> OnGetAsync()
        {
            var usuario_token = "123";

            if (string.IsNullOrEmpty(usuario_token))
            {
                return RedirectToPage("/Login");
            }

            Profile = new ProfileViewModel
            {
                ProfilePictureUrl = "/images/DojaCat.jpg",
                Username = "Yael Alfredo Salazar Aguilar",
                Matricula = "S22013671",
                Posts = new List<PostViewModel>
            {
                new PostViewModel { Id = 1, ImageUrl = "/images/logo-fei.png", Caption = "¡Mi primer post kawaii!" },
                new PostViewModel { Id = 2, ImageUrl = "/images/post2.png", Caption = "Listo para el backend uwu~" },
                new PostViewModel { Id = 3, ImageUrl = "/images/post3.png", Caption = "Progresando como un prota de shonen 💪" },
                new PostViewModel { Id = 4, ImageUrl = "/images/post3.png", Caption = "Progresando como un prota de shonen 💪" },
                new PostViewModel { Id = 5, ImageUrl = "/images/post3.png", Caption = "Progresando como un prota de shonen 💪" }
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
            var postDto = GetPostById(postId); // Aquí simulas como si lo trajeras de la API

            if (postDto == null)
            {
                return NotFound();
            }

            var model = new PostPartialViewModel
            {
                Id = postDto.Id,
                ImageUrl = postDto.ImageUrl,
                Caption = postDto.Caption,
                Likes = postDto.Likes,
                Comments = postDto.Comments.Select(c => new CommentViewModel
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
    }
}
