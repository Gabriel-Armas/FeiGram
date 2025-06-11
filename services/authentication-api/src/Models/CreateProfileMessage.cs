
namespace AuthenticationApi.Models
{
   public class CreateProfileMessage
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
    } 
}