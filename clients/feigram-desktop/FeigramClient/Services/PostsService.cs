using FeigramClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeigramClient.Services
{
    class PostsService
    {
        private readonly HttpClient _httpClient;
        private string _jwtToken = "";

        public PostsService(HttpClient http)
        {
            _httpClient = http;
        }

        public void SetToken(string token)
        {
            _jwtToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        private static void HandleErrorResponse(HttpResponseMessage response, string context)
        {
            var error = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && error.Contains("Token expirado"))
                throw new UnauthorizedAccessException("Tu sesión ha expirado. Inicia sesión nuevamente.");

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden && error.Contains("baneado"))
                throw new UnauthorizedAccessException("Tu cuenta está baneada.");

            throw new Exception($"{context}: {response.StatusCode} - {error}");
        }

        public async Task<List<CommentDto>> GetCommentsAsync(int postId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/posts/posts/{postId}/comments");

                if (!response.IsSuccessStatusCode)
                {
                    HandleErrorResponse(response, "Error al obtener comentarios");
                }

                var content = await response.Content.ReadFromJsonAsync<CommentListResult>();
                return content?.comments ?? new List<CommentDto>();
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener comentarios del servidor", ex);
            }
        }

        public async Task<bool> CreatePostAsync(string descripcion, string mediaUrl, ProfileSingleton me)
        {
            try
            {
                var post = new PostCreateDto
                {
                    descripcion = descripcion,
                    url_media = mediaUrl,
                    fechaPublicacion = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync("/posts/posts", post);

                if (!response.IsSuccessStatusCode)
                {
                    HandleErrorResponse(response, "Error al crear post");
                }

                return true;
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el post", ex);
            }
        }

        public async Task<ImageUploadResult> UploadImageAsync(string imagePath)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(imagePath);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                content.Add(fileContent, "file", Path.GetFileName(imagePath));

                var response = await _httpClient.PostAsync("/posts/upload-image", content);

                if (!response.IsSuccessStatusCode)
                {
                    HandleErrorResponse(response, "Error al subir imagen");
                }

                var result = await response.Content.ReadFromJsonAsync<ImageUploadResult>();
                return result ?? throw new Exception("Respuesta inválida del servidor");
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (Exception ex)
            {
                throw new Exception("Error al subir la imagen", ex);
            }
        }

        public async Task<List<UserPostDto>> GetUserPostsAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/posts/posts/user/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    HandleErrorResponse(response, "Error al obtener los posts del usuario");
                }

                var posts = await response.Content.ReadFromJsonAsync<List<UserPostDto>>();
                return posts ?? new List<UserPostDto>();
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los posts del usuario", ex);
            }
        }

        public async Task<int> GetLikesCounterAsync(string postId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/posts/posts/{postId}/likes-count");
                if (!response.IsSuccessStatusCode)
                {
                    HandleErrorResponse(response, "Error al obtener el contador de likes");
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                var root = doc.RootElement;

                if (root.TryGetProperty("like_count", out var likeCountElement) &&
                    likeCountElement.TryGetInt32(out var likeCount))
                {
                    return likeCount;
                }

                return 0;
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el contador de likes", ex);
            }
        }



        private class CommentResponse
        {
            public int PostId { get; set; }
            public List<CommentDto> Comments { get; set; }
        }
    }
}
