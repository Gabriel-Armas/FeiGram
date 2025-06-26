using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;


namespace app.Pages
{
    public class LoginModel : PageModel, IAsyncPageFilter
    {
        [BindProperty]
        public LoginViewModel Input { get; set; }
        private readonly ILogger<LoginModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(ILogger<LoginModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://feigram-nginx");

            var content = new StringContent(
                JsonSerializer.Serialize(new { email = Input.Email, password = Input.Password }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync("/auth/login", content);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos");
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Respuesta login: " + json);

            LoginResponse result = null;
            try
            {
                _logger.LogInformation("1");
                result = JsonSerializer.Deserialize<LoginResponse>(json);
                _logger.LogInformation("Respuesta login: " + result.Token + " rol: "+ result.Rol +" user id:" +result.UserId);
                _logger.LogInformation("2");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al procesar la respuesta del servidor");
                return Page();
            }

            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                ModelState.AddModelError(string.Empty, "Respuesta inválida del servidor");
                return Page();
            }

            _logger.LogInformation("Token recibido: " + result.Token);
            
            Response.Cookies.Append("jwt_token", result.Token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });


            return RedirectToPage("/Feed/Feed");
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var path = context.HttpContext.Request.Path.Value;
            var exemptPaths = new[] { "/Login", "/Register", "/ForgotPassword", "/ResetPassword" };

            if (!exemptPaths.Contains(path))
            {
                var token = context.HttpContext.Request.Cookies["jwt_token"];

                if (string.IsNullOrEmpty(token) || TokenHelper.TokenExpirado(token))
                {
                    context.Result = new RedirectToPageResult("/Login");
                    return;
                }
            }

            await next();
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }

        private class LoginResponse
{
            [JsonPropertyName("token")]
            public string Token { get; set; }

            [JsonPropertyName("userId")]
            public string UserId { get; set; } = string.Empty;

            [JsonPropertyName("rol")]
            public string Rol { get; set; } = string.Empty;
        }
        public static class TokenHelper
        {
            public static bool TokenExpirado(string token)
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var exp = jwtToken.ValidTo;
                return exp < DateTime.UtcNow;
            }
        }
    }
}