using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using RemkofDataLibrary.BusinessLogic.Authorization.Registration;
using RemkofDataLibrary.BusinessLogic.Authorization.Login;
using RemkofFrontend.ViewModels;
using Microsoft.Extensions.Configuration;
using RemkofDataLibrary.BusinessLogic;
using RemkofDataLibrary.BusinessLogic.Admininstraror;

namespace RemkofFrontend.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IRegistrationService _registrationService;
        private readonly ILoginService _loginService;
        private readonly IOptionsService _options;
        private readonly IUsersService _usersService;
        private readonly IConfiguration _config;
        public AuthController(ILogger<AuthController> logger, IRegistrationService registrationService, IConfiguration config, ILoginService loginService, IOptionsService options, IUsersService usersService)
        {
            _logger = logger;
            _registrationService = registrationService;
            _config = config;
            _loginService = loginService;
            _options = options;
            _usersService = usersService;

            //Если в системе не существует пользователей
            if (_usersService.GetUsers().Result.Count == 0)
                _ = CreateDefaultUser();
        }

        private async Task CreateDefaultUser()
        {
            var login = _config.GetSection("DefaultUserAuthData").GetValue<string>("login").Trim();
            var email = _config.GetSection("DefaultUserAuthData").GetValue<string>("email").Trim();
            var password = _config.GetSection("DefaultUserAuthData").GetValue<string>("password");

            await _registrationService.RegisterUser(login, email, password, true);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Admin");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Admin");
            if(!await _options.GetRegistrationSetting())
                return RedirectToAction("Index", "Home");

            if (TempData["IsAuthSuccessful"] is not null)
                ViewBag.IsAuthSuccessful = TempData["IsAuthSuccessful"];
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
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var registrationStatus = await _registrationService.RegisterUser(model.Username, model.Email, model.Password);
                switch(registrationStatus)
                {
                    case RegistrationStatus.Success:
                        //Аккаунту нужна активация - поэтому не авторизуем, а просто подтверждаем успех.
                        TempData["IsAuthSuccessful"] = true;
                        return RedirectToAction("Register", "Auth");
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
        public async Task<IActionResult> Login(LoginViewModel model)
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
                    case LoginStatus.UserNotActivated:
                        ModelState.AddModelError("", "Пользователь не активирован.");
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
