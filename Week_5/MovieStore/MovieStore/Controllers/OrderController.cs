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

        public ShoppingCart GetCart()
        {
            // geef de cart terug vanuit de session
            return SessionHelper.GetObjectFromJson<ShoppingCart>(HttpContext.Session, "cart");
        }

        public void SaveCart(ShoppingCart cart)
        {
            SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
        }

        public CheckOutModel CreateModel(ShoppingCart shoppingCart)
        {
            var user = _userManager.GetUserAsync(HttpContext.User).Result;

            CheckOutModel model = new CheckOutModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                ShoppingCart = shoppingCart
            };
            return model;
        }


        public IActionResult CheckOut()
        {
            //TASK: implement here
            ShoppingCart cart = GetCart();

            if (cart == null || cart.Lines.Count == 0)
            {
                return RedirectToAction(nameof(CartController.Index), nameof(CartController));
            }

            CheckOutModel model = CreateModel(cart);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckOut(CheckOutModel model)
        {
            ShoppingCart cart = GetCart();

            if (cart == null || cart.Lines.Count == 0)
            {
                return RedirectToAction(nameof(CartController.Index), nameof(CartController));
            }

           model.ShoppingCart = cart;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _userManager.GetUserAsync(HttpContext.User).Result;
            string userId = user.Id;
            _orderService.CreateOrder(model, userId);

            // clear shopping cart
            return RedirectToAction(nameof(Success));
        }


        public IActionResult Success()
        {
            //TASK: clear shopping cart and show a simple success-page
            ShoppingCart cart = GetCart();
            if (cart != null)
            {
                cart.Clear();
                SaveCart(cart);
            }
            return View("Success",cart);
        }
    }
}
