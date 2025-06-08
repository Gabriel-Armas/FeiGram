using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using app.ViewModel;
using app.DTO;

namespace app.Pages.Account
{
    public class CreateAccountModel : PageModel
    {
        [BindProperty]
        public CreateAccountViewModel CreateAccount { get; set; } = new CreateAccountViewModel();

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            return RedirectToPage("/Login");
        }
    }
}