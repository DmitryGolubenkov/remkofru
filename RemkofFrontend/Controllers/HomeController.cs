using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RemkofDataLibrary.BusinessLogic;
using RemkofDataLibrary.Models;
using RemkofFrontend.Models;
using RemkofFrontend.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RemkofFrontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPricesService _prices;
        private readonly IMemoryCache _serverCache;

        public HomeController(ILogger<HomeController> logger, IPricesService prices, IMemoryCache serverCache)
        {
            _logger = logger;
            _prices = prices;
            _serverCache = serverCache;
        }


        public IActionResult Index()
        {
            return View();
        }

        [Route("/About")]
        public IActionResult About()
        {
            return View();
        }

        [Route("/Contacts")]
        public IActionResult Contacts()
        {
            return View();
        }

        [Route("/Status")]
        public IActionResult Status()
        {
            return View();
        }

        [Route("/ShippingAndPayment")]
        public IActionResult ShippingAndPayment()
        {
            return View();
        }

        [Route("/Prices")]
        public async Task<IActionResult> Prices()
        {
            List<ServicePrice> pricesList;
            if (!_serverCache.TryGetValue("prices_list", out pricesList))
            {
                pricesList = await _prices.GetPrices();
                _serverCache.Set("prices_list", pricesList, TimeSpan.FromDays(1));
            }

            pricesList= pricesList.OrderBy(price => price.ViewPriority).ToList();

            List<ServicePriceViewModel> shownPricesList = new List<ServicePriceViewModel>();
            foreach (var price in pricesList)
            {
                shownPricesList.Add(new ServicePriceViewModel()
                {
                    Price = price.Price,
                    ServiceName = price.ServiceName
                });
            }
            return View(shownPricesList);
        }

        [Route("/NotFound")]
        public IActionResult NotFound()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
