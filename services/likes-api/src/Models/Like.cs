namespace LikesApi.Models;

public class Like
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string PostId { get; set; } = string.Empty;
}
