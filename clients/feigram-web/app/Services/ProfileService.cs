using app.DTO;
using System.Text.Json;

public class ProfileService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _client;


    public ProfileService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _client = httpClientFactory.CreateClient("feigram");
    }

    public void SetBearerToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<ProfileDTO>?> GetProfilesAsync()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];

        if (!string.IsNullOrEmpty(token))
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await _client.GetAsync("/profiles/profiles");
        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine("JSON de perfiles: " + content);
        return JsonSerializer.Deserialize<List<ProfileDTO>>(content);
    }

    public async Task<bool> EditProfileAsync(
        string id,
        string username,
        string enrollment,
        string major,
        Stream? photoStream = null,
        string? photoFileName = null)
    {

        var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];

        if (!string.IsNullOrEmpty(token))
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        using var content = new MultipartFormDataContent
        {
            { new StringContent(username), "Name" },
            { new StringContent(enrollment), "Enrollment" },
            { new StringContent(major), "Major" }
        };

        if (photoStream != null && !string.IsNullOrEmpty(photoFileName))
        {
            var imageContent = new StreamContent(photoStream);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(imageContent, "Photo", photoFileName);
        }

        var response = await _client.PutAsync($"/profiles/profiles/{id}", content);

        return response.IsSuccessStatusCode;
    }

    public async Task<List<ProfileDTO>?> GetFollowingAsync(string userId)
    {
        AttachTokenFromCookie();

        var response = await _client.GetAsync($"/profiles/profiles/{userId}/following");
        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"JSON de seguidos de {userId}: " + content);

        return JsonSerializer.Deserialize<List<ProfileDTO>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<ProfileDTO?> GetProfileByIdAsync(string userId)
    {
        AttachTokenFromCookie();

        var response = await _client.GetAsync($"/profiles/profiles/{userId}");
        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Perfil JSON para user {userId}: " + content);

        return JsonSerializer.Deserialize<ProfileDTO>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private void AttachTokenFromCookie()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
        if (!string.IsNullOrEmpty(token))
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<List<ProfileDTO>?> GetProfileDetailsForUserListAsync(List<string> userIds)
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
        if (!string.IsNullOrEmpty(token))
        {
            SetBearerToken(token);
        }

        var response = await _client.PostAsJsonAsync("/profiles/batch", userIds);

        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<ProfileDTO>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}