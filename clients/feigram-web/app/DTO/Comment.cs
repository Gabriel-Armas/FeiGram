namespace app.DTO
{
    public class Comment
    {
        public string post_id { get; set; } = string.Empty;
        public string user_id { get; set; } = string.Empty;
        public string text_comment { get; set; } = string.Empty;
        public DateTime created_at { get; set; }
    }
}