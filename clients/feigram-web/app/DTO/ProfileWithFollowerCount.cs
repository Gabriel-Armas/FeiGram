namespace app.DTO
{
    public class ProfileWithFollowerCount
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public string Enrollment { get; set; } = string.Empty;
        public int FollowerCount { get; set; }
    }
}