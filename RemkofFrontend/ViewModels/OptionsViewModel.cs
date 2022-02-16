using System.ComponentModel;

namespace RemkofFrontend.ViewModels
{
    public class OptionsViewModel
    {
        [DisplayName("Регистрация разрешена")]
        public bool IsRegistrationEnabled { get; set; }
    }
}
