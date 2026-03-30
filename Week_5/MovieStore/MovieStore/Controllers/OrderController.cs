using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Movie_Store.Helpers;
using Movie_Store.Models;
using Movie_Store.Services;
using MovieStore_StartHier_OK.Authorisation;
using MovieStore_StartHier_OK.Models;
using MovieStore_StartHier_OK.Models.ViewModels;

namespace MovieStore_StartHier_OK.Controllers
{
    [Authorize]

    public class OrderController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMovieService _movieService;
        private readonly IOrderService _orderService;

        public OrderController(IMovieService movieService, IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _movieService = movieService;
            _orderService = orderService;
            _userManager = userManager;
        }

        public IActionResult GetCart()
        {
            // geef de cart terug vanuit de session
            SessionHelper.GetObjectFromJson<ShoppingCart>(HttpContext.Session, "cart");
            return View();
        }

        public IActionResult SaveCart()
        {
            SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", new ShoppingCart());
            return View();
        }

        public IActionResult CreateModel(ShoppingCart shoppingCart)
        {
            var user = _userManager.GetUserAsync(HttpContext.User).Result;

            CheckOutModel model = new CheckOutModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                ShoppingCart = shoppingCart
            };
            return View(model);
        }


        public IActionResult CheckOut()
        {
            //TASK: implement here
            ShoppingCart cart = SessionHelper.GetObjectFromJson<ShoppingCart>(HttpContext.Session, "cart");

            if (cart == null || cart.Lines.Count == 0)
            {
                return RedirectToAction(nameof(CartController.Index), nameof(CartController));
            }

            CreateModel(cart);

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckOut(CheckOutModel model)
        {
            ShoppingCart cart = SessionHelper.GetObjectFromJson<ShoppingCart>(HttpContext.Session, "cart");

            if (cart == null || cart.Lines.Count == 0)
            {
                return RedirectToAction(nameof(CartController.Index), nameof(CartController));
            }

            CreateModel(cart);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _userManager.GetUserAsync(HttpContext.User).Result;
            string userId = user.Id;
            _orderService.CreateOrder(model, userId);

            // clear shopping cart
            return View("Succes");
        }


        public IActionResult Success()
        {
            //TASK: clear shopping cart and show a simple success-page
            ShoppingCart cart = SessionHelper.GetObjectFromJson<ShoppingCart>(HttpContext.Session, "cart");
            if (cart != null)
            {
                cart.Clear();
                SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            }
            return View();
        }
    }
}
