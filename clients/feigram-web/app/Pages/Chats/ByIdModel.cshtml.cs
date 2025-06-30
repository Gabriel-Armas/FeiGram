using app.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace app.Pages.Chats
{
    public class ByIdModel : PageModel
    {
        private readonly ProfileService _profileService;

        public ByIdModel(ProfileService profileService)
        {
            _profileService = profileService;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Missing id");

            var profile = await _profileService.GetProfileByIdAsync(id);

            if (profile == null)
                return NotFound();

            return new JsonResult(profile);
        }
    }
}
