using FeigramClient.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeigramClient.Services
{
    class ProfileService
    {
        private readonly HttpClient _httpClient;
        private string _jwtToken = "";

        public ProfileService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SetToken(string token)
        {
            _jwtToken = token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        public async Task<Profile?> GetProfileAsync(string id)
        {
            var response = await _httpClient.GetAsync($"/profiles/profiles/{id}");
            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponse(response);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Profile>();
        }

        public async Task<List<Profile>> GetProfilesAsync()
        {
            var response = await _httpClient.GetAsync("/profiles/profiles");

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponse(response);
                return new List<Profile>();
            }

            var profiles = await response.Content.ReadFromJsonAsync<List<Profile>>();
            return profiles ?? new List<Profile>();
        }

        public async Task<bool> EditAsync(string id, MultipartFormDataContent form)
        {
            var response = await _httpClient.PutAsync($"/profiles/profiles/{id}", form);

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponse(response);
                return false;
            }

            return true;
        }

        public async Task<ProfileWithFollowerCount?> GetByEnrollmentAsync(string enrollment)
        {
            var response = await _httpClient.GetAsync($"/profiles/profiles/enrollment/{enrollment}");

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponse(response);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ProfileWithFollowerCount>();
        }

        public async Task<List<ProfileWithFollowerCount>> SearchProfilesByNameAsync(string name)
        {
            var response = await _httpClient.GetAsync("/profiles/profiles/search/" + name);

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponse(response);
                return new List<ProfileWithFollowerCount>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ProfileWithFollowerCount>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ProfileWithFollowerCount>();
        }

        private async Task HandleErrorResponse(HttpResponseMessage response)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            string errorMessage;

            try
            {
                var errorJson = JsonDocument.Parse(errorContent);
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
    }
}
