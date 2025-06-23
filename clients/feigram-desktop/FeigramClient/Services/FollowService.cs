using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FeigramClient.Services
{
    public class FollowService
    {
        private readonly HttpClient _httpClient;
        private string _jwtToken = "";

        public FollowService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SetToken(string token)
        {
            _jwtToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        public async Task<string> FollowUserAsync(string followerId, string followedId)
        {
            var url = $"/follow/follow/{followerId}/{followedId}";

            var response = await _httpClient.PostAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al seguir al usuario: {error}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<Dictionary<string, string>>(content);

            return json?["message"] ?? "Follow successful";
        }

        public async Task<List<string>> GetFollowingAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"/follow/following/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al obtener seguidos: {error}");
            }

            var content = await response.Content.ReadAsStringAsync();

            var json = JsonSerializer.Deserialize<FollowingResponse>(content);
            return json?.Following ?? new List<string>();
        }

        public async Task<string> UnfollowUserAsync(string followerId, string followedId)
        {
            var url = $"/follow/unfollow/{followerId}/{followedId}";

            var response = await _httpClient.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al dejar de seguir al usuario: {error}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<Dictionary<string, string>>(content);

            return json?["message"] ?? "Unfollow successful";
        }


        private class FollowingResponse
        {
            [JsonPropertyName("following")]
            public List<string> Following { get; set; } = new();
        }
    }
}
