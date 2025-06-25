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
    class CommentsService
    {
        private readonly HttpClient _httpClient;

        public CommentsService(HttpClient http)
        {
            _httpClient = http;
        }

        public async Task<ProfileDto?> GetProfileByIdAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/comments/profiles/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
                    return profile;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Puedes loguear el error o relanzarlo si quieres
                throw new Exception("Error al obtener el perfil", ex);
            }
        }

        public async Task<Comment?> AddCommentAsync(Comment comment)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/comments/comments", comment);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Comment>();
                }

                // Aquí podrías manejar errores específicos (404, 400, etc.)
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el comentario", ex);
            }
        }
    }
}
