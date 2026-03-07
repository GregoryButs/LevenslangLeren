using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MVC_Bibliotheek.Models
{
    public class AuthorViewModel
    {
        public int AuthorId { get; set; }

        [Required(ErrorMessage = "Voornaam is vereist")]
        [Display(Name = "Voornaam")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Familienaam is vereist")]
        [Display(Name = "Familienaam")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Geboortedatum is vereist")]
        [Display(Name = "Geboortedatum")]
        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }
    }
}
