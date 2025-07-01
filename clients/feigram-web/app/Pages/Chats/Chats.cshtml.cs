using app.ViewModel;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using app.DTO;

namespace app.Pages.Chats
{
    public class ChatModel : PageModel
    {
        private readonly ProfileService _profileService;

        public ChatModel(ProfileService profileService)
        {
            _profileService = profileService;
        }

        public string? JwtToken { get; set; }
        public string? MyUserId { get; set; }
        public List<ProfileDTO> ContactProfiles { get; set; } = new List<ProfileDTO>();

        public async Task OnGetAsync()
        {
            JwtToken = Request.Cookies["jwt_token"];
            if (string.IsNullOrEmpty(JwtToken)) return;

            _profileService.SetBearerToken(JwtToken);

            MyUserId = Request.Cookies["user_id"];
            if (string.IsNullOrEmpty(MyUserId))
            {
                ContactProfiles = new List<ProfileDTO>();
                return;
            }

            var followingProfiles = await _profileService.GetFollowingAsync(MyUserId);
            if (followingProfiles == null || followingProfiles.Count == 0)
            {
                ContactProfiles = new List<ProfileDTO>();
                return;
            }
            ContactProfiles = followingProfiles;
        }

    }
}
