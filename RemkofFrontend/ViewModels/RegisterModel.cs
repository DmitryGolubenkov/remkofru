using System.ComponentModel.DataAnnotations;

namespace RemkofFrontend.ViewModels
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Имя пользователя не указано")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Не указан Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароль введен неверно")]
        public string ConfirmPassword { get; set; }
    }
}
