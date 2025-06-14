namespace app.ViewModel
{
    public class ProfileViewModel
    {
        public string ProfilePictureUrl { get; set; }
        public string Username { get; set; }
        public string Matricula { get; set; }
        public int Followers { get; set; }
        public List<PostViewModel> Posts { get; set; }
    }    
}
