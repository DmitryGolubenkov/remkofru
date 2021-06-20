using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RemkofFrontend.Models;
using System.Diagnostics;

namespace RemkofFrontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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

        [Route("/Prices")]
        public IActionResult Prices()
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
