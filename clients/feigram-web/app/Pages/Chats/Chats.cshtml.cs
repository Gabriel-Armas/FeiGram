using app.ViewModel;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace app.Pages.Chats
{
    public class ChatModel : PageModel
    {
        public List<FriendViewModel> Friends { get; set; } = new List<FriendViewModel>();

        public string? JwtToken { get; set; }

        public void OnGet()
        {
            JwtToken = Request.Cookies["jwt_token"];
        }
    }
}
