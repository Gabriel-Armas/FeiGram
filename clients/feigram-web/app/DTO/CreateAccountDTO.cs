namespace app.DTO
{
    public class CreateAccountDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Enrollment { get; set; } = string.Empty;
    }
}