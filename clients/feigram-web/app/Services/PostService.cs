using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using app.DTO; // Asegúrate de tener los DTOs correctos
using System.Collections.Generic;

public class PostService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;

    public PostService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _httpClient = httpClientFactory.CreateClient("feigram"); // Usa el nombre de tu cliente registrado
    }

    public void SetBearerToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<Comment>> GetCommentsAsync(int postId)
    {
        var response = await _httpClient.GetAsync($"/posts/posts/{postId}/comments");

        if (!response.IsSuccessStatusCode)
        {
            // Aquí puedes lanzar error o loguear
            return new List<Comment>();
        }

        var content = await response.Content.ReadFromJsonAsync<CommentListResult>();
        return content?.comments ?? new List<Comment>();
    }
}
