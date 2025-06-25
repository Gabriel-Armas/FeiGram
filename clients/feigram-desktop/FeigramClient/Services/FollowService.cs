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
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        public async Task<string> FollowUserAsync(string followerId, string followedId)
        {
            try
            {
                var url = $"/follow/follow/{followerId}/{followedId}";
                var response = await _httpClient.PostAsync(url, null);

                await HandleErrors(response, "seguir al usuario");

                var content = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
                return json?["message"] ?? "Follow successful";
            }
            catch (Exception ex)
            {
                throw new Exception("Error en FollowUserAsync", ex);
            }
        }

        public async Task<List<string>> GetFollowingAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/follow/following/{userId}");

                await HandleErrors(response, "obtener seguidos");

                var content = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<FollowingResponse>(content);
                return json?.Following ?? new List<string>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error en GetFollowingAsync", ex);
            }
        }

        public async Task<string> UnfollowUserAsync(string followerId, string followedId)
        {
            try
            {
                var url = $"/follow/unfollow/{followerId}/{followedId}";
                var response = await _httpClient.DeleteAsync(url);

                await HandleErrors(response, "dejar de seguir al usuario");

                var content = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
                return json?["message"] ?? "Unfollow successful";
            }
            catch (Exception ex)
            {
                throw new Exception("Error en UnfollowUserAsync", ex);
            }
        }

        public async Task<bool> IsFollowingAsync(string followerId, string followedId)
        {
            try
            {
                var url = $"/follow/is_following/{followerId}/{followedId}";
                var response = await _httpClient.GetAsync(url);

                await HandleErrors(response, "verificar seguimiento");

                var content = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<IsFollowingResponse>(content);
                return json?.IsFollowing ?? false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error en IsFollowingAsync", ex);
            }
        }

        private async Task HandleErrors(HttpResponseMessage response, string actionDescription)
        {
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                    error.Contains("Token expirado", System.StringComparison.OrdinalIgnoreCase))
                    throw new UnauthorizedAccessException("Tu sesión ha expirado. Inicia sesión nuevamente.");

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden &&
                    error.Contains("baneado", System.StringComparison.OrdinalIgnoreCase))
                    throw new UnauthorizedAccessException("Tu cuenta está baneada.");

                throw new Exception($"Error al {actionDescription}: {response.StatusCode} - {error}");
            }
        }

        private class IsFollowingResponse
        {
            [JsonPropertyName("is_following")]
            public bool IsFollowing { get; set; }
        }

        private class FollowingResponse
        {
            [JsonPropertyName("following")]
            public List<string> Following { get; set; } = new();
        }
    }
}
