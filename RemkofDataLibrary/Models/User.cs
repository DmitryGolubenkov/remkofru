namespace RemkofDataLibrary.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } //Максимум 30 символов
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public bool IsActivated { get; set; }
    }
}
