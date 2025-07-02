namespace app.ViewModel
{
    public class StatsViewModel
    {
        public int TotalPosts { get; set; }
        public Dictionary<string, int> PostsPerDay { get; set; } = new();
        public string WeekRange { get; set; } = "";

    }
}