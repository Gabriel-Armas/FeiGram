using FeigramClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace FeigramClient.Services
{
    public class LikesService
    {
        private readonly HttpClient _httpClient;

        public LikesService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Like?> CreateLikeAsync(Like like)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/likes/likes", like);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Like>();
                }
                else
                {
                    Console.WriteLine($"Error al crear like. StatusCode: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Excepción en CreateLikeAsync: " + ex.Message);
                return null;
            }
        }
    }
}
