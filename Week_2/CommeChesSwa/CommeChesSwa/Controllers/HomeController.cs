using System.Diagnostics;
using CommeChesSwa.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommeChesSwa.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult OverOns()
        {
            return View();
        }
    }
}
