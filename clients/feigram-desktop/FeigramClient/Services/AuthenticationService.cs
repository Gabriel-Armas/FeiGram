using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FeigramClient.Models;

namespace FeigramClient.Services
{
    internal class AuthenticationService
    {
        private readonly HttpClient _httpClient;
        private string _jwtToken = "";

        public AuthenticationService(HttpClient http)
        {
            _httpClient = http;
        }

        public void SetToken(string token)
        {
            _jwtToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            var request = new LoginRequest { Email = email, Password = password };

            var response = await _httpClient.PostAsJsonAsync("/auth/login", request);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return data;
            }

            return null;
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(MultipartFormDataContent form)
        {
            var response = await _httpClient.PostAsync("/auth/register", form);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                await HandleErrorResponse(response);
                var error = await response.Content.ReadAsStringAsync();

                return (false, error);
            }
        }

        public async Task<AuthResponse> GetAccountAsync(string id)
        {
            var response = await _httpClient.GetAsync($"/auth/users/{id}");

            if (response.IsSuccessStatusCode)
            {
                var account = await response.Content.ReadFromJsonAsync<AuthResponse>();
                return account ?? new AuthResponse { Email = string.Empty, Role = string.Empty };
            }
            else
            {
                await HandleErrorResponse(response);
                return new AuthResponse { Email = string.Empty, Role = string.Empty }; 
            }
        }

        public async Task<bool> BanAsync(string email)
        {
            var content = JsonContent.Create(new { Email = email });
            var response = await _httpClient.PostAsync("/auth/ban-user", content);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                await HandleErrorResponse(response);
                return false;
            }
        }

        private async Task HandleErrorResponse(HttpResponseMessage response)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            string errorMessage;

            try
            {
                var errorJson = System.Text.Json.JsonDocument.Parse(errorContent);
                if (errorJson.RootElement.TryGetProperty("message", out var messageElement))
                {
                    errorMessage = messageElement.GetString() ?? "Error desconocido";
                }
                else
                {
                    errorMessage = errorContent;
                }
            }
            catch
            {
                errorMessage = errorContent;
            }

            if (errorMessage.Contains("Token expirado", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Sesión expirada. Por favor, inicia sesión de nuevo.");
            }
            else if (errorMessage.Contains("baneado", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Usuario baneado. No tienes acceso.");
            }
            else
            {
                throw new Exception($"Error en la respuesta del servidor: {errorMessage}");
            }
        }

        public async Task<bool> UnbanUserAsync(string email)
        {
            var content = JsonContent.Create(new { Email = email });
            var response = await _httpClient.PostAsync($"/auth/unban-user", content);
            return response.IsSuccessStatusCode;
        }
    }
}
