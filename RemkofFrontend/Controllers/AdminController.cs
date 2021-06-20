using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RemkofFrontend.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        public bool IsRegistrationEnabled { get; set; }

        //TODO: все 3 страницы
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ChangePrices()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }
    }
}
