using FeigramClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

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

        public async Task<List<Profile>> GetProfilesAsync()
        {
            var response = await _httpClient.GetAsync("/profiles/profiles");
            if (!response.IsSuccessStatusCode)
            {
                return new List<Profile>();
            }

            var profiles = await response.Content.ReadFromJsonAsync<List<Profile>>();
            return profiles ?? new List<Profile>();
        }

        public async Task<bool> EditAsync(string id, MultipartFormDataContent form)
        {
            var response = await _httpClient.PutAsync($"/profiles/{id}", form);

            return response.IsSuccessStatusCode;
        }

        public async Task<ProfileWithFollowerCount?> GetByEnrollmentAsync(string enrollment)
        {
            var response = await _httpClient.GetAsync($"/profiles/profiles/enrollment/{enrollment}");

            if (!response.IsSuccessStatusCode)
                return null;

            var profile = await response.Content.ReadFromJsonAsync<ProfileWithFollowerCount>();
            return profile;
        }

        public async Task<List<ProfileWithFollowerCount>> SearchProfilesByNameAsync(string name)
        {
            var response = await _httpClient.GetAsync("/profiles/profiles/search/" + name);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ProfileWithFollowerCount>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ProfileWithFollowerCount>();
        }

    }
}
