using FeigramClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
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

        private class CommentResponse
        {
            public int PostId { get; set; }
            public List<CommentDto> Comments { get; set; }
        }
    }
}
