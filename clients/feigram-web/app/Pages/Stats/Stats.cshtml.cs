using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using app.ViewModel;

namespace app.Pages.Stats
{
    public class StatsModel : PageModel
    {
        private readonly StatisticsService _statsService;
        private readonly ILogger<StatsModel> _logger;

        public StatsModel(StatisticsService statsService, ILogger<StatsModel> logger)
        {
            _logger = logger;
            _statsService = statsService;
        }

        public StatsViewModel Stats { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(token))
            {
                _statsService.SetBearerToken(token);
            }
            else
            {
                _logger.LogWarning("No se encontró token JWT para hacer peticiones autorizadas");
            }

            Stats = await _statsService.GetStatsAsync();
            return Page();
        }
    }
}