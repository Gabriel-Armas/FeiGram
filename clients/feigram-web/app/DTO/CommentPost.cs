namespace app.DTO
{
    public class CommentPost
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? PostId { get; set; }
        public string? TextComment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}