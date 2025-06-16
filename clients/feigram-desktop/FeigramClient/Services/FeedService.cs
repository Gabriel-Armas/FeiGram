using FeigramClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeigramClient.Services
{
    class FeedService
    {
        private readonly HttpClient _httpClient;

        public FeedService(HttpClient http)
        {
            _httpClient = http;
        }

        public async Task<List<PostDto>> GetRecommendations(string userId)
        {
            var response = await _httpClient.GetAsync($"/feed/posts/recommendations?user_id={userId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RecommendationResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Posts ?? new List<PostDto>();
        }

    }
}
