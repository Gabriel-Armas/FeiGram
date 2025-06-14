using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using app.ViewModel;
using app.DTO;

namespace app.Pages.Account
{
    public class AccountsModel : PageModel
    {
        public List<AccountViewModel> Accounts { get; set; } = new();

        public void OnGet()
        {
            Accounts = GetAccounts();
        }

        public IActionResult OnPostBan(int id)
        {
            BanAccount(id);
            return RedirectToPage();
        }

        private List<AccountViewModel> GetAccounts()
        {
            return new List<AccountViewModel>
            {
                new AccountViewModel { Id = 1, FullName = "Aki", Email = "aki@example.com", StudentId = "UV123456" },
                new AccountViewModel { Id = 2, FullName = "Yuu", Email = "yuu@example.com", StudentId = "UV654321" }
            };
        }

        private void BanAccount(int id)
        {

        }
    }
}
