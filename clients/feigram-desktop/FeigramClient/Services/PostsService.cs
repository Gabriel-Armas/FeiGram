using FeigramClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        public async Task<List<CommentDto>> GetCommentsAsync(int postId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/posts/posts/{postId}/comments");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener comentarios: {response.StatusCode}");
                }

                var content = await response.Content.ReadFromJsonAsync<CommentListResult>();
                return content?.comments ?? new List<CommentDto>();
            }
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
                    fechaPublicacion = DateTime.UtcNow.ToLocalTime()
                };

                var response = await _httpClient.PostAsJsonAsync("/posts/posts", post);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al crear post: {response.StatusCode} - {error}");
                }

                //var content = await response.Content.ReadFromJsonAsync<CreatePostResult>();
                //return content?.post_id ?? throw new Exception("Respuesta inválida del servidor");
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el post", ex);
            }
        }


        // 🔵 POST /upload-image - Subir imagen
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
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al subir imagen: {response.StatusCode} - {error}");
                }

                var result = await response.Content.ReadFromJsonAsync<ImageUploadResult>();
                return result ?? throw new Exception("Respuesta inválida del servidor");
            }
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
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al obtener los posts del usuario: {response.StatusCode} - {error}");
                }

                var posts = await response.Content.ReadFromJsonAsync<List<UserPostDto>>();
                return posts ?? new List<UserPostDto>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los posts del usuario", ex);
            }
        }

        private class CommentResponse
        {
            public int PostId { get; set; }
            public List<CommentDto> Comments { get; set; }
        }
    }
}
