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
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        public async Task<Profile?> GetProfileAsync(string id)
        {
            var response = await _httpClient.GetAsync($"/profiles/profiles/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var profile = await response.Content.ReadFromJsonAsync<Profile>();
            return profile;
        }

    }
}
