namespace app.ViewModel
{
    public class PostViewModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
        public int Likes { get; set; }    
    }
}