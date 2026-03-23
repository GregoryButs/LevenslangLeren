using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Movie_Store.Services;
using Movie_Store.Helpers;
using MovieStore_StartHier_OK.Models;
using Movie_Store.Models;

namespace MovieStore_StartHier_OK.Controllers
{
    public class CartController : Controller
    {
        private readonly IMovieService _service;

        public CartController(IMovieService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            ShoppingCart cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int movieId, int quantity)
        {
            //TASK: add item to cart or update the quantity if the item already exists in the cart .
            ShoppingCart currentCart = GetCart();
            Movie movie = _service.GetMovieById(movieId);

            if (movie != null)
            {
                currentCart.AddMovie(movie, quantity);
                SaveCart(currentCart);
            }

            return RedirectToAction("Index");
        }

        public void SaveCart(ShoppingCart cart)
        {
            //TASK: save the cart to the session
            SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
        }

        public ShoppingCart GetCart()
        {
            var cart = SessionHelper.GetObjectFromJson<ShoppingCart>(HttpContext.Session, "cart");

            if (cart == null)
            {
                cart = new ShoppingCart();
                // Optional: Save the empty cart to session immediately
                SaveCart(cart);
            }

            return cart;
        }

        //TASK: add action to remove product from cart
        public IActionResult Remove(int movieId)
        {
            ShoppingCart cart = GetCart();
            cart.RemoveMovie(movieId);
            SaveCart(cart);

            return RedirectToAction("Index");
        }

        //TASK: add action to clear the shopping cart
        public IActionResult Clear()
        {
            ShoppingCart cart = GetCart();
            cart.Clear();
            SaveCart(cart);
            return RedirectToAction("Index");
        }
    }
}
