using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Movie_Store.Services;

namespace MovieStore_StartHier_OK.Controllers
{
    public class OrderController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly IOrderService _orderService;

        public OrderController(IMovieService movieService, IOrderService orderService)
        {
            _movieService = movieService;
            _orderService = orderService;
        }

        public IActionResult CheckOut()
        {
            //TASK: implement here
            return View();
        }

        public IActionResult Success()
        {
            //TASK: clear shopping cart and show a simple success-page

            return View();
        }
    }
}
