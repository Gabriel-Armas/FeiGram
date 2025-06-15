using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using app.ViewModel;
using app.DTO;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace app.Pages.Account
{
    [Authorize]
    public class AccountsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AccountsModel> _logger;

        public AccountsModel(IHttpClientFactory httpClientFactory, ILogger<AccountsModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public List<AccountViewModel> Accounts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://feigram-nginx");

            try
            {
                var response = await client.GetAsync("/profiles/profiles");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Se obtuvieron los perfiles exitosamente");

                    Accounts = JsonSerializer.Deserialize<List<AccountViewModel>>(json) ?? new();
                }
                else
                {
                    _logger.LogWarning("No se pudo obtener la lista de perfiles Código: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener perfiles");
            }
        }

        public async Task<IActionResult> OnPostBan(int id, string email)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://feigram-nginx");

            var content = new StringContent(
                JsonSerializer.Serialize(new { Email = email }),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await client.PostAsync("/auth/ban-user", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Usuario con ID {Id} y correo {Email} fue baneado exitosamente", id, email);
                }
                else
                {
                    _logger.LogWarning("No se pudo banear al usuario {Email} 😿 Status: {StatusCode}", email, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "¡Error! No se pudo banear al usuario {Email} 😢", email);
            }

            return RedirectToPage();
        }
    }
}
