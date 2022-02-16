using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RemkofDataLibrary.BusinessLogic;
using RemkofDataLibrary.BusinessLogic.Admininstraror;
using RemkofDataLibrary.Models;
using RemkofFrontend.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RemkofFrontend.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IUsersService _usersService;
        private readonly IPricesService _prices;
        private readonly IMemoryCache _serverCache;
        private readonly IOptionsService _options;

        private List<AdminServicePriceViewModel> CachedPrices
        {
            get
            {
                var s = HttpContext.Session.GetString("cached_prices");
                if (s != null)
                    return JsonSerializer.Deserialize<List<AdminServicePriceViewModel>>(s);
                return null;
            }
            set
            {
                HttpContext.Session.SetString("cached_prices", JsonSerializer.Serialize(value));
            }
        }


        private void ClearCachedPrices()
        {
            ViewBag.IsParsedData = false;
            HttpContext.Session.Remove("cached_prices");
        }

        public AdminController(IConfiguration config, IUsersService usersService, IPricesService prices, IMemoryCache serverCache, IOptionsService options)
        {
            _config = config;
            _usersService = usersService;
            _prices = prices;
            _serverCache = serverCache;
            _options = options;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public IActionResult Index()
        {
            return RedirectToAction("Users");
        }

        [HttpGet]
        public async Task<IActionResult> Options()
        {
            List<OptionModel> optionList = await _options.GetOptions();
            if (TempData["Success"] is not null)
                ViewBag.Success = (bool)TempData["Success"];
            if (optionList is not null && optionList.Count > 0)
            {
                OptionsViewModel model = new OptionsViewModel()
                {
                    IsRegistrationEnabled = optionList.Find(x => x.KeyName == "is_registration_enabled").KeyValue.Equals("true", StringComparison.InvariantCultureIgnoreCase) ? true : false
                };
                return View(model);
            }
            else
                return View(new OptionsViewModel()
                {
                    //Настройки по умолчанию
                    IsRegistrationEnabled = true
                }
                );

        }

        [HttpPost]
        public async Task<IActionResult> Options(OptionsViewModel newOptions)
        {
            List<OptionModel> optionList = new List<OptionModel>();
            optionList.Add(new OptionModel() { KeyName = "is_registration_enabled", KeyValue = newOptions.IsRegistrationEnabled.ToString() });

            await _options.SaveOptions(optionList);

            TempData["Success"] = true;
            return RedirectToAction("Options");
        }

        public async Task<IActionResult> Prices()
        {
            if (CachedPrices != null)
            {
                ViewBag.IsParsedData = true;
                return View(CachedPrices);
            }
            ViewBag.IsParsedData = false;
            var data = await _prices.GetPrices();
            data = data.OrderBy(price => price.ViewPriority).ToList();
            List<AdminServicePriceViewModel> adminServicePriceViewModels = new List<AdminServicePriceViewModel>();
            foreach (var item in data)
            {
                adminServicePriceViewModels.Add(new AdminServicePriceViewModel()
                {
                    ServiceId = item.PriceId,
                    ServiceName = item.ServiceName,
                    Price = item.Price,
                    ViewPriority = item.ViewPriority
                });
            }

            if (TempData["AddPriceSuccess"] is not null)
            {
                ViewBag.AddPriceSuccess = TempData["AddPriceSuccess"];
                if (TempData["AddPriceMessage"] is not null)
                    ViewBag.AddPriceMessage = TempData["AddPriceMessage"];
            }

            return View(adminServicePriceViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> Prices(List<AdminServicePriceViewModel> adminServicePriceViewModels)
        {
            HashSet<int> priorities = new HashSet<int>();
            List<ServicePrice> prices = new List<ServicePrice>();
            foreach (var item in adminServicePriceViewModels)
            {
                if (item.MarkedForRemoval)
                {
                    await DeletePrice(item);
                    continue;
                }
                if (!priorities.Add(item.ViewPriority))
                {

                    ViewBag.PricesUpdateSuccess = false;
                    ViewBag.PricesUpdateMessage = "У двух рядов совпадают приоритеты.";
                    ViewBag.IsParsedData = false;

                    return View("Prices", adminServicePriceViewModels);
                }

                prices.Add(new ServicePrice()
                {
                    PriceId = item.ServiceId,
                    ServiceName = item.ServiceName,
                    Price = item.Price,
                    ViewPriority = item.ViewPriority
                });

            }

            await _prices.UpdatePrices(prices);

            //Обновляем значение в кэше
            //_serverCache.Set("prices_list", prices, TimeSpan.FromDays(1));
            await RefreshPricesCache();
            return RedirectToAction("Prices");
        }


        public async Task<IActionResult> Users()
        {
            var usersFromDb = await _usersService.GetUsers();
            List<UserViewModel> usersViewModels = new List<UserViewModel>();
            foreach (var user in usersFromDb)
            {
                UserViewModel userViewModel = new UserViewModel()
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    IsActivated = user.IsActivated,
                    MarkedForRemoval = false
                };
                usersViewModels.Add(userViewModel);
            }

            if (TempData["UsersUpdateSuccess"] is not null)
            {
                ViewBag.IsUsersUpdateSuccessful = (bool)TempData["UsersUpdateSuccess"];
                if (TempData["UsersUpdateMessage"] is not null)
                    ViewBag.UsersUpdateMessage = TempData["UsersUpdateMessage"].ToString();
            }

            _serverCache.Set("users", usersFromDb, TimeSpan.FromMinutes(10));
            return View(usersViewModels);
        }

        private async void DeleteUser(User user)
        {
            await _usersService.DeleteUser(user);
        }

        public bool ValidateUsers(List<UserViewModel> models)
        {
            HashSet<string> hashes = new HashSet<string>();
            foreach (var user in models)
            {
                if (!hashes.Add(user.Username))
                {
                    TempData["UsersUpdateSuccess"] = false;
                    TempData["UsersUpdateMessage"] = "Все имена пользователей должны отличаться.";
                    return false;

                }
                if (!hashes.Add(user.Email))
                {
                    TempData["UsersUpdateSuccess"] = false;
                    TempData["UsersUpdateMessage"] = "Все почтовые адреса должны отличаться.";
                    return false;
                }
            }
            return true;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUsers(List<UserViewModel> models)
        {
            if (models is null || models.Count == 0)
                return RedirectToAction("Users");

            //Валидация
            if (!ValidateUsers(models))
                return RedirectToAction("Users");


            //Работа
            List<User> users;
            if (!_serverCache.TryGetValue("users", out users))
                users = await _usersService.GetUsers();

            for (int i = 0; i < models.Count; i++)
            {
                User userToCheck = null;
                UserViewModel model = models[i];

                //Сначала проверяем, совпадают ли модель и пользователь по индексам списков
                if (users[i].UserId == model.UserId)
                    userToCheck = users[i];
                else
                {
                    //Если нет - пытаемся найти перебором
                    for (int j = 0; j < users.Count; j++)
                        if (users[j].UserId == model.UserId)
                        {
                            userToCheck = users[j];
                            break;
                        }
                }

                //Если нашли
                if (userToCheck != null)
                {
                    //Если пользователь отмечен для удаления - удаляем и переходим к следующему
                    if (model.MarkedForRemoval)
                    {
                        DeleteUser(userToCheck);
                        i--;
                        continue;
                    }

                    if (userToCheck.Email != model.Email || userToCheck.Username != model.Username || userToCheck.IsActivated != model.IsActivated)
                    {
                        if (model.Username is null || model.Username.Length == 0)
                        {
                            TempData["UsersUpdateSuccess"] = false;
                            TempData["UsersUpdateMessage"] = "Одно из полей имён пользователя пусто.";
                            return RedirectToAction("Users");
                        }
                        if (model.Email is null || model.Email.Length == 0)
                        {
                            TempData["UsersUpdateSuccess"] = false;
                            TempData["UsersUpdateMessage"] = "Одно из полей с почтовым адресом пусто.";
                            return RedirectToAction("Users");
                        }

                        userToCheck.Username = model.Username.Trim();
                        userToCheck.Email = model.Email.Trim();
                        userToCheck.IsActivated = model.IsActivated;

                        await _usersService.UpdateUser(userToCheck);
                    }
                }
                else
                {
                    TempData["UsersUpdateSuccess"] = false;
                    TempData["UsersUpdateMessage"] = "Пользователь не найден.";
                    return RedirectToAction("Users");
                }

            }

            TempData["UsersUpdateSuccess"] = true;
            return RedirectToAction("Users");
        }

        [HttpPost]
        public IActionResult CancelImport()
        {
            ClearCachedPrices();

            return RedirectToAction("Prices");
        }


        //TODO: корректная валидация. Надо сделать модель, в ней атрибуты валидации, затем здесь поменять код под модель. И также поменять в отображении.
        [HttpPost]
        public IActionResult ProcessCsvFile(IFormFile uploadedFile)
        {
            if (uploadedFile is not null)
            {
                try
                {
                    int priority = 1;
                    List<AdminServicePriceViewModel> adminServicePrices = new List<AdminServicePriceViewModel>();
                    Encoding encoding = Encoding.GetEncoding("windows-1251");

                    using var memoryStream = new MemoryStream();
                    if (memoryStream.Length < 4194304)
                        using (var reader = new StreamReader(uploadedFile.OpenReadStream(), encoding))
                        {

                            while (reader.Peek() >= 0)
                            {
                                string line = reader.ReadLine();
                                string[] parts = line.Split(';');
                                if (parts.Length == 2)
                                    adminServicePrices.Add(new AdminServicePriceViewModel()
                                    {
                                        ServiceName = parts[0],
                                        Price = parts[1],
                                        ViewPriority = priority
                                    });
                                priority++;
                            }
                        }

                    ViewBag.IsParsedData = true;
                    CachedPrices = adminServicePrices;
                    return View("Prices", adminServicePrices);
                }
                catch (Exception ex)
                {
                    TempData["ImportSuccess"] = false;
                    TempData["ImportMessage"] = $"Во время выполнения задачи возникла следующая ошибка: + {ex.Message}";
                }
            }
            return RedirectToAction("Prices");
        }

        [HttpPost]
        public async Task<IActionResult> SaveNewPrices()
        {
            List<ServicePrice> servicePrices = new List<ServicePrice>();

            foreach (var pr in CachedPrices)
            {

                servicePrices.Add(new ServicePrice()
                {
                    ServiceName = pr.ServiceName,
                    Price = pr.Price,
                    ViewPriority = pr.ViewPriority
                });
            }
            await _prices.SaveNewPriceList(servicePrices);
            ClearCachedPrices();
            await RefreshPricesCache();
            return RedirectToAction("Prices");
        }

        private async Task RefreshPricesCache()
        {
            var pricesList = await _prices.GetPrices();
            _serverCache.Set("prices_list", pricesList, TimeSpan.FromDays(1));
        }


        private async Task DeletePrice(AdminServicePriceViewModel price)
        {
            await _prices.DeletePrice(new ServicePrice()
            {
                ServiceName = price.ServiceName,
                Price = price.Price,
                ViewPriority = price.ViewPriority,
                PriceId = price.ServiceId
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddPrice(AdminServicePriceViewModel model)
        {
            if (model is not null)
            {
                if (model.Price is null || model.ServiceName is null || model.ViewPriority == 0)
                {
                    TempData["AddPriceSuccess"] = false;
                    TempData["AddPriceMessage"] = $"Одно из полей осталось незаполненным.";
                    return RedirectToAction("Prices");
                }

                ServicePrice servicePrice = new ServicePrice()
                {
                    ServiceName = model.ServiceName,
                    Price = model.Price,
                    ViewPriority = model.ViewPriority
                };

                await _prices.AddPriceToDatabase(servicePrice);

                TempData["AddPriceSuccess"] = true;
                await RefreshPricesCache();
            }
            return RedirectToAction("Prices");
        }
    }
}
