using Movie_Store.Models;
using System.ComponentModel.DataAnnotations;

namespace MovieStore_StartHier_OK.Models
{
    public class ShoppingCart
    {
        public List<CartLine> Lines { get; set; } = new List<CartLine>();

        public double TotalPrice()
        {
            // mac 2 cijfers na de komma
            return Math.Round(Lines.Sum(x => x.TotalPrice), 2);
        }

        public ShoppingCart()
        {
            Lines = new List<CartLine>();
        }

        public void AddMovie(Movie movie, int quantity)
        {
            CartLine line = Lines.Where(x => x.Movie.Id == movie.Id).FirstOrDefault();
            if (line == null)
            {
                Lines.Add(new CartLine(movie, quantity));
            }
            else
            {
                line.Quantity += quantity;
            }
        }

        public void RemoveMovie(int movieId)
        {
            Lines.RemoveAll(x => x.Movie.Id == movieId);
        }

        public void Clear()
        {
            Lines.Clear();
        }


    }
}
