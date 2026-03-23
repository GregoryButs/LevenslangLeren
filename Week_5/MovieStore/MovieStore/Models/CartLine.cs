using Movie_Store.Models;
using System.ComponentModel.DataAnnotations;

namespace MovieStore_StartHier_OK.Models
{
    public class CartLine
    {
        public Movie Movie { get; set; }
        public int Quantity { get; set; }

        [Display(Name = "Subtotal")]
        public double TotalPrice => Math.Round(Movie.Price * Quantity,2);
        public CartLine(Movie movie, int quantity)
        {
            Movie = movie;
            Quantity = quantity;
        }
        public CartLine()
        {
        }
    }
}
