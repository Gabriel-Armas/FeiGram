using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using app.ViewModel;
using app.DTO;

namespace app.Pages.Stats
{
    public class StatsModel : PageModel
    {
        public StatsViewModel Stats { get; set; } = new StatsViewModel();

        public async Task<IActionResult> OnGetAsync()
        {
            var usuario_token = "123";

            if (string.IsNullOrEmpty(usuario_token))
            {
                return RedirectToPage("/Login");
            }

            Stats = new StatsViewModel
            {
                TotalPosts = 100,
                PostsPerDay = new Dictionary<string, int>
                {
                    { "Monday", 10 },
                    { "Tuesday", 15 },
                    { "Wednesday", 12 },
                    { "Thursday", 18 },
                    { "Friday", 20 },
                    { "Saturday", 14 },
                    { "Sunday", 11 }
                }
            };

            Stats.WeekRange = GetWeekRange(DateTime.Now);

            return Page();
        }

        private string GetWeekRange(DateTime date)
        {
            int diff = date.DayOfWeek == DayOfWeek.Sunday ? -6 : DayOfWeek.Monday - date.DayOfWeek;
            DateTime startOfWeek = date.AddDays(diff);
            DateTime endOfWeek = startOfWeek.AddDays(6);
            return $"{startOfWeek:yyyy-MM-dd} ~ {endOfWeek:yyyy-MM-dd}";
        }
    }
}