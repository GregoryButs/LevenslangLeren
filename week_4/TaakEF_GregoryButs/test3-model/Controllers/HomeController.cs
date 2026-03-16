using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using test3_model.Data;
using test3_model.Models;
using test3_model.Models.Viewmodels;
using test4_start.Services;

namespace test3_model.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IEventService _eventservice;
        private readonly IBegeleiderService _begeleiderservice;
        private readonly ILocatieService _locatieservice;

        public HomeController(ILogger<HomeController> logger, IEventService eventservice, IBegeleiderService begeleiderservice, ILocatieService locatieservice)
        {
            _logger = logger;
            _eventservice = eventservice;
            _begeleiderservice = begeleiderservice;
            _locatieservice = locatieservice;
        }

        public IActionResult Index()
        {
            IEnumerable<EventViewModel> model = _eventservice.GetEventList();
            return View(model);
           
        }


        public IActionResult Details(int id)
        {
            EventViewModel model = _eventservice.GetEventById(id);
            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Annuleer(int id)
        {
            _eventservice.CancelEvent(id);
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }


        public IActionResult Create()
        {
            CreateEventCommand cmd = new CreateEventCommand();
            cmd.AllLocaties = _locatieservice.GetLocaties();
            cmd.AllBegeleiders = _begeleiderservice.GetBegeleiders();
            return View(cmd);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateEventCommand cmd)
        {
            if (ModelState.IsValid)
            {
                int id = _eventservice.CreateEvent(cmd);
                return RedirectToAction("Index");
            }
            cmd.AllLocaties = _locatieservice.GetLocaties();
            cmd.AllBegeleiders = _begeleiderservice.GetBegeleiders();
            return View(cmd);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
