using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; }
}
