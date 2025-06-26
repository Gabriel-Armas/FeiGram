using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using app.ViewModel;
using app.DTO;
using System.Net.Http.Headers;

namespace app.Pages.Feed
{
    public class FeedModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IWebHostEnvironment _env;

        public List<PostViewModel> Posts { get; set; } = new();

        public FeedModel(IHttpClientFactory clientFactory, IWebHostEnvironment env)
        {
            _clientFactory = clientFactory;
            _env = env;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Login");


            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri("https://feigram-nginx");
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", Request.Cookies["jwt_token"]);

            try
            {
                var response = await client.GetFromJsonAsync<List<PostViewModel>>($"feed/recommendations");
                if (response != null)
                    Posts = response;
            }
            catch
            {
                // Puedes loguear error si deseas UwU
            }

            return Page();
        }

        public async Task<IActionResult> OnGetPostModalAsync(int postId)
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
        }

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
    }
}