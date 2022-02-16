namespace RemkofFrontend.ViewModels
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsActivated { get; set; }
        public bool MarkedForRemoval { get; set; }
    }
}
