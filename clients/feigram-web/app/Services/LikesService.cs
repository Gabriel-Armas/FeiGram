using app.DTO;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public class LikesService
{
    private readonly HttpClient _client;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LikesService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _client = httpClientFactory.CreateClient("feigram");
    }


    public async Task<Like?> CreateLikeAsync(Like like)
    {
        Console.WriteLine($"🔁 POST /likes/likes: {like}");

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(like),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/likes/likes", jsonContent);

        Console.WriteLine($"🔁 POST /likes/likes StatusCode: {(int)response.StatusCode} ({response.StatusCode})");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Error body: {error}");
        }

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Like>();
        }

        return null;
    }


    public async Task<bool> CheckIfUserLikedPostAsync(string userId, string postId)
    {
        Console.WriteLine($"🔁 GET /likes/likes/check?userId={userId}&postId={postId}");

        var response = await _client.GetAsync($"/likes/likes/check?userId={userId}&postId={postId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        return false;
    }

    public async Task<bool> DeleteLikeAsync(string userId, string postId)
    {
        var response = await _client.DeleteAsync($"/likes/likes?userId={userId}&postId={postId}");
        
        return response.IsSuccessStatusCode;
    }
    
    public void SetBearerToken(string token)
    {
        if (_client.DefaultRequestHeaders.Authorization == null || 
            _client.DefaultRequestHeaders.Authorization.Parameter != token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

}
