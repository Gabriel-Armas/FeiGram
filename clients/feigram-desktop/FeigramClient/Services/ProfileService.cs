using FeigramClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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

        public async Task<bool> EditAsync(string id, string? name, string? sex, string? photoPath = null)
        {
            using var form = new MultipartFormDataContent();

            if (!string.IsNullOrWhiteSpace(name))
                form.Add(new StringContent(name), "Name");

            if (!string.IsNullOrWhiteSpace(sex))
                form.Add(new StringContent(sex), "Sex");

            if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
            {
                var fileStream = File.OpenRead(photoPath);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                form.Add(fileContent, "Photo", Path.GetFileName(photoPath));
            }

            var response = await _httpClient.PutAsync($"/profiles/{id}", form);

            return response.IsSuccessStatusCode;
        }

    }
}
