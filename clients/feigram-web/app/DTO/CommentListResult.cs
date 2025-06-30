namespace app.DTO
{
    public class CommentListResult
    {
        public string post_id { get; set; } = string.Empty;
        public List<Comment> comments { get; set; } = new();
    }
}
