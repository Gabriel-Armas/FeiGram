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
using app.DTO;

namespace app.Pages
{
    public class LoginModel : PageModel, IAsyncPageFilter
    {
        [BindProperty]
        public LoginViewModel Input { get; set; }
        private readonly AuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(AuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _authService.LoginAsync(Input.Email, Input.Password);

            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos");
                return Page();
            }

            _logger.LogInformation("Token recibido: {Token} - Rol: {Rol} - UserId: {UserId}",
                result.Token, result.Rol, result.UserId);

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
            
            Response.Cookies.Append("jwt_token", result.Token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            Response.Cookies.Append("user_id", result.UserId, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            Response.Cookies.Append("rol", result.Rol, new CookieOptions
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