using System.ComponentModel.DataAnnotations;

namespace MVC_Bibliotheek.Data.Entities
{
    public class Author
    {
        public int AuthorId { get; set; }

        [Display(Name = "Auteur voornaam")]
        public string FirstName { get; set; }

        [Display(Name = "Auteur familienaam")]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }

        public ICollection<Book>? Books { get; set; }
    }
}
