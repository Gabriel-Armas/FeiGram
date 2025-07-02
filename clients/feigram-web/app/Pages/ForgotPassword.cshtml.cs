using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace app.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }

        public string? Message { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            Message = "Si el correo existe, recibirás un enlace para cambiar tu contraseña 📨";

            return Page();
        }
    }
}
