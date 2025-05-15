using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace app.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ILogger<LoginModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            var usuario_token = "123"; // Simulado, normalmente iría en la sesión o cookie
            if (string.IsNullOrEmpty(usuario_token))
            {
                return RedirectToPage("/Login"); // O Redirect("/Login") si es una ruta directa
            }

            return Page();
        }


        public IActionResult OnPost()
        {
            // Aquí validarías el usuario real...
            
            // Agregamos una cookie (token simulado por ahora)
            Response.Cookies.Append("usuario_token", "123", new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            return RedirectToPage("/Index");
        }
    }
}