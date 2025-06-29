using app.DTO;
using System.Net.Http;
using System.Text;
using System.Text.Json;


public class AuthService
{
    private readonly HttpClient _client;

    public AuthService(IHttpClientFactory factory)
    {
        _client = factory.CreateClient("feigram");
    }

    public void SetBearerToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new { email, password }),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/auth/login", content);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LoginResponse>(json);
    }

    public async Task<bool> RegisterAsync(
        string username,
        string password,
        string email,
        string sex,
        string enrollment,
        Stream? photoStream = null,
        string? photoFileName = null)
    {
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(username), "Username");
        content.Add(new StringContent(password), "Password");
        content.Add(new StringContent(email), "Email");
        content.Add(new StringContent(sex), "Sex");
        content.Add(new StringContent(enrollment), "Enrollment");

        if (photoStream != null && !string.IsNullOrEmpty(photoFileName))
        {
            var imageContent = new StreamContent(photoStream);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(imageContent, "Photo", photoFileName);
        }

        var response = await _client.PostAsync("/auth/register", content);

        return response.IsSuccessStatusCode;
    }

    public async Task<AccountDTO?> GetAccountAsync(string id)
    {
        var response = await _client.GetAsync($"/auth/users/{id}");

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AccountDTO>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    
    public async Task<bool> BanUserAsync(string email)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new { Email = email }),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/auth/ban-user", content);

        return response.IsSuccessStatusCode;
    }
}