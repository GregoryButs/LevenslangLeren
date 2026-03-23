using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoKlasAuthenticatie.Controllers
{
    [Authorize(Policy = "CanVote")]
    public class VotingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
