using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using FeigramClient.Models;
using static System.Net.WebRequestMethods;

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
    }
}
