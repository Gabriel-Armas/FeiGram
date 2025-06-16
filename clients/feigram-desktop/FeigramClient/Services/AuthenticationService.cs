using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using FeigramClient.Models;
using System.IO;

namespace FeigramClient.Services
{
    internal class AuthenticationService
    {
        private readonly HttpClient _httpClient;

        public AuthenticationService(HttpClient http)
        {
            _httpClient = http;
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

        public async Task<bool> RegisterAsync(MultipartFormDataContent form)
        {
            var response = await _httpClient.PostAsync("/auth/register", form);
            return response.IsSuccessStatusCode;
        }


        public async Task<AuthResponse> GetAccountAsync(string id)
        {
            var response = await _httpClient.GetAsync($"/auth/users/{id}");

            if (response.IsSuccessStatusCode)
            {
                var account = await response.Content.ReadFromJsonAsync<AuthResponse>();
                return account ?? new AuthResponse { Email = string.Empty, Role = string.Empty };
            }
            return new AuthResponse { Email = string.Empty, Role = string.Empty };
        }


        public async Task<bool> BanAsync(string email)
        {
            var content = JsonContent.Create(new { Email = email });
            var response = await _httpClient.PostAsync("/auth/ban-user", content);
            return response.IsSuccessStatusCode;
        }
    }
}
