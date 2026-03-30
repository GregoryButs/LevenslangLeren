using Movie_Store.Models;
using System.ComponentModel.DataAnnotations;

namespace MovieStore_StartHier_OK.Models.ViewModels
{
    public class CheckOutModel
    {
        public int Id { get; set; }

        public ShoppingCart ShoppingCart { get; set; }

        [Required]
        public string MovieTitle { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        [Display(Name = "Total")]
        public double TotalPrice { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public int Number { get; set; }

        [Required]
        public string ZipCode { get; set; }


        public Order ToOrder(string userId)
        {
            return new Order
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                Street = this.Street,
                City = this.City,
                Number = this.Number,
                Zip = int.Parse(this.ZipCode),
                UserId = userId,
                OrderLines = ShoppingCart.Lines.Select(line => new OrderLine
                {
                    MovieId = line.Movie.Id,
                    Quantity = line.Quantity,
                    Price = line.TotalPrice
                }).ToList()
            };
        }

    }
}
