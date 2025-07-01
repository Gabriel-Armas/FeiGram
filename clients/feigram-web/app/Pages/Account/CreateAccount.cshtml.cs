// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.RazorPages;
// using app.ViewModel;
// using app.DTO;

// namespace app.Pages.Account
// {
//     public class CreateAccountModel : PageModel
//     {
//         private readonly AuthService _authService;

//         public CreateAccountModel(AuthService authService)
//         {
//             _authService = authService;
//         }

//         [BindProperty]
//         public CreateAccountViewModel CreateAccount { get; set; } = new CreateAccountViewModel();

//         [BindProperty]
//         public IFormFile? Photo { get; set; }

//         public IActionResult OnGet()
//         {
//             return Page();
//         }

//         public async Task<IActionResult> OnPostAsync()
//         {
//             if (!ModelState.IsValid)
//             {
//                 return Page();
//             }

//             Stream? photoStream = null;
//             string? photoFileName = null;

//             if (Photo != null)
//             {
//                 photoStream = Photo.OpenReadStream();
//                 photoFileName = Photo.FileName;
//             }

//             var result = await _authService.RegisterAsync(
//                 CreateAccount.Username,
//                 CreateAccount.Password,
//                 CreateAccount.Email,
//                 CreateAccount.Sex,
//                 CreateAccount.Enrollment,
//                 photoStream,
//                 photoFileName
//             );

//             if (!result)
//             {
//                 ModelState.AddModelError(string.Empty, "No se pudo crear la cuenta, por favor intenta de nuevo :(");
//                 return Page();
//             }

//             return RedirectToPage("/Account/Accounts");
//         }
//     }
// }