using AfsprakenbeheerPsycholoog.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak
{
    /// <summary>
    /// ViewModel voor het bewerken van een bestaande afspraak.
    /// </summary>
    public class EditAfspraakViewModel : CreateAfspraakViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Status")]
        public AfspraakStatus Status { get; set; }
    }
}

