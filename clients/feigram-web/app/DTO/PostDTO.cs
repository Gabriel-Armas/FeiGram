namespace app.DTO
{
    public class PostDTO
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public DateTime PostedAt { get; set; }

        public int Likes { get; set; }
        public List<CommentDTO> Comments { get; set; }
    }
}
