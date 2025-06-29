using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using app.ViewModel;
using app.DTO;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace app.Pages.Account
{
    [Authorize]
    public class AccountsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AccountsModel> _logger;
        private readonly AuthService _authService;
        private readonly ProfileService _profileService;

        [BindProperty]
        public IFormFile? Photo { get; set; }

        [BindProperty]
        public RegisterViewModel CreateAccount { get; set; } = new();

        public AccountsModel(ProfileService profileService, AuthService authService, ILogger<AccountsModel> logger)
        {
            _profileService = profileService;
            _authService = authService;
            _logger = logger;
        }

        public List<FulluserViewModel> Accounts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var profiles = await _profileService.GetProfilesAsync();

            if (profiles == null || profiles.Count == 0)
            {
                _logger.LogWarning("No se encontraron perfiles.");
                return;
            }

            var token = HttpContext.Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(token))
            {
                _authService.SetBearerToken(token);
            }
            else
            {
                _logger.LogWarning("No se encontró token JWT para hacer peticiones autorizadas");
            }

            var usersList = new List<FulluserViewModel>();

            foreach (var profile in profiles)
            {
                var account = await _authService.GetAccountAsync(profile.Id);
                if (account != null)
                {
                    usersList.Add(new FulluserViewModel
                    {
                        Id = profile.Id,
                        Username = profile.Username,
                        Enrollment = profile.Enrollment,
                        Email = account.Email,
                        Role = account.Role,
                        ProfilePictureUrl = profile.Photo
                    });
                }
            }

            Accounts = usersList;
        }


        public async Task<IActionResult> OnPostBan(int id, string email)
        {
            var token = HttpContext.Request.Cookies["jwt_token"];
            
            if (!string.IsNullOrEmpty(token))
            {
                _authService.SetBearerToken(token);
            }
            else
            {
                _logger.LogWarning("No se encontró token JWT para hacer peticiones autorizadas");
            }

            _logger.LogInformation("Intentando banear al usuario con ID: {Id} y Email: {Email}", id, email);

            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "El email no puede estar vacío");
                return Page();
            }

            bool result = await _authService.BanUserAsync(email);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "No se pudo banear al usuario, por favor intenta de nuevo");
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            var token = HttpContext.Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(token))
            {
                _authService.SetBearerToken(token);
            }
            else
            {
                _logger.LogWarning("No se encontró token JWT para hacer peticiones autorizadas");
            }

            _logger.LogInformation("Intentando crear una nueva cuenta con los datos: {@CreateAccount}", CreateAccount);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido al intentar crear una cuenta: {@ModelState}", ModelState);
                return Page();
            }

            Stream? photoStream = null;
            string? photoFileName = null;

            if (Photo != null)
            {
                photoStream = Photo.OpenReadStream();
                photoFileName = Photo.FileName;
            }

            bool result = await _authService.RegisterAsync(
                CreateAccount.Username,
                CreateAccount.Password,
                CreateAccount.Email,
                CreateAccount.Sex = "",
                CreateAccount.Enrollment,
                photoStream,
                photoFileName
            );

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "No se pudo crear la cuenta, por favor intenta de nuevo");
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            var token = HttpContext.Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(token))
            {
                _authService.SetBearerToken(token);
            }
            else
            {
                _logger.LogWarning("No se encontró token JWT para hacer peticiones autorizadas");
            }

            _logger.LogInformation("Intentando editar la cuenta con ID: {Id}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido al intentar editar la cuenta: {@ModelState}", ModelState);
                return Page();
            }

            Stream? photoStream = null;
            string? photoFileName = null;

            if (Photo != null)
            {
                photoStream = Photo.OpenReadStream();
                photoFileName = Photo.FileName;
            }

            var result = await _profileService.EditProfileAsync(id, CreateAccount.Username, CreateAccount.Enrollment, photoStream, photoFileName);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "No se pudo editar la cuenta, por favor intenta de nuevo");
                return Page();
            }

            return RedirectToPage();
        }
    }
}
