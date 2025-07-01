using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace app.Pages
{
    public class ResetPasswordModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Token { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string Message { get; set; }

        public void OnGet()
        {
            // Aquí podrías validar el token si quieres de una vez
        }

        public IActionResult OnPost()
        {
            if (Password != ConfirmPassword)
            {
                Message = "Las contraseñas no coinciden 😿";
                return Page();
            }

            // Aquí iría tu lógica para actualizar la contraseña con el token
            // (como buscar el usuario por el token, validar tiempo, etc)

            bool cambioExitoso = CambiarContrasena(Token, Password);
            if (cambioExitoso)
            {
                Message = "¡Tu contraseña ha sido cambiada exitosamente! 🎉";
            }
            else
            {
                Message = "Hubo un problema con el token 😢";
            }

            return Page();
        }

        private bool CambiarContrasena(string token, string nuevaContrasena)
        {
            // Tu lógica real aquí uwu ✨
            return true;
        }
    }
}