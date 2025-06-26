using FeigramClient.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeigramClient.Services
{
    class FeedService
    {
        private readonly HttpClient _httpClient;
        private string _jwtToken = "";

        public FeedService(HttpClient http)
        {
            _httpClient = http;
        }

        public void SetToken(string token)
        {
            _jwtToken = token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        public async Task<List<PostDto>> GetRecommendations(string userId, int skip = 0, int limit = 10)
        {
            try
            {
                var url = $"/feed/posts/recommendations?user_id={userId}&skip={skip}&limit={limit}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                        error.Contains("Token expirado", StringComparison.OrdinalIgnoreCase))
                        throw new UnauthorizedAccessException("Tu sesión ha expirado. Inicia sesión nuevamente.");

                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden &&
                        error.Contains("baneado", StringComparison.OrdinalIgnoreCase))
                        throw new UnauthorizedAccessException("Tu cuenta está baneada.");

                    throw new Exception($"Error al obtener recomendaciones: {response.StatusCode} - {error}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<RecommendationResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Posts ?? new List<PostDto>();
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener recomendaciones del servidor", ex);
            }
        }

    }
}
