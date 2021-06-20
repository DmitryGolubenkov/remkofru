using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RemkofDataLibrary.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using RemkofDataLibrary.BusinessLogic.Authorization;
using RemkofDataLibrary.BusinessLogic.Authorization.Registration;
using RemkofDataLibrary.BusinessLogic.Authorization.Login;
using RemkofFrontend.ViewModels;
using Microsoft.Extensions.Configuration;

namespace RemkofFrontend.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IRegistrationService _registrationService;
        private readonly ILoginService _loginService;
        private readonly IConfiguration _config;

        public AuthController(ILogger<AuthController> logger, IRegistrationService registrationService, IConfiguration config, ILoginService loginService)
        {
            _logger = logger;
            _registrationService = registrationService;
            _config = config;
            _loginService = loginService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Admin");
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Admin");
            if(_config.GetValue<bool>("RegistrationEnabled") == false)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var registrationStatus = await _registrationService.RegisterUser(model.Username, model.Email, model.Password);
                switch(registrationStatus)
                {
                    case RegistrationStatus.Success:
                        await Authenticate(model.Username);
                        return RedirectToAction("Index", "Admin");
                    case RegistrationStatus.UsernameAndEmailExists:
                        ModelState.AddModelError("Email", "Данный почтовый адрес уже зарегестрирован.");
                        ModelState.AddModelError("Username", "Пользователь с таким именем уже существует.");
                        break;
                    case RegistrationStatus.EmailExists:
                        ModelState.AddModelError("Email", "Данный почтовый адрес уже зарегестрирован.");
                        break;
                    case RegistrationStatus.UsernameExists:
                        ModelState.AddModelError("Username", "Пользователь с таким именем уже существует.");
                        break;
                }
            }
            ViewBag.Message = "Запрос не прошел валидацию";
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                model.Username = model.Username.Trim();

                var status = await _loginService.Login(model.Username, model.Password);
                switch(status)
                {
                    case LoginStatus.Success:
                        await Authenticate(model.Username);
                        return RedirectToAction("Index", "Admin");

                    case LoginStatus.IncorrectLogin:
                        ModelState.AddModelError("", "Такого пользователя не существет.");
                        break;
                    case LoginStatus.IncorrectPassword:
                        ModelState.AddModelError("", "Пароль неверен.");
                        break;
                }
            }
            
            return View(model);
        }

        private async Task Authenticate(string userName)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}
