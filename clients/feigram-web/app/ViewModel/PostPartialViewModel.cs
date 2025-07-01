namespace app.ViewModel
{
    public class PostPartialViewModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = "";
        public string Caption { get; set; } = "";
        public int Likes { get; set; }
        public List<CommentViewModel> Comments { get; set; } = new();
    }
}