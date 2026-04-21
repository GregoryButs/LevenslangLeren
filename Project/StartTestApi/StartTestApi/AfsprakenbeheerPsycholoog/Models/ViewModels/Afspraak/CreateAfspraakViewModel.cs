using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak
{
    /// <summary>
    /// Enum voor het herhaalpatroon van een afspraak, gebruikt in CreateAfspraakViewModel om aan te geven of en hoe een afspraak moet worden herhaald.
    /// </summary>
    public enum HerhaalPatroon
    {
        Geen = 0,
        Dagelijks = 1,
        Wekelijks = 2
    }

    // erft van IValidatableObject om custom validatie toe te voegen (voor herhaling)
    public class CreateAfspraakViewModel : IValidatableObject
    {
        [Display(Name = "Patiënt")]
        public int? PatientId { get; set; }

        [Required(ErrorMessage = "Kies een type")]
        [Display(Name = "Type afspraak")]
        public int TypeId { get; set; }

        [Required(ErrorMessage = "Starttijd is verplicht")]
        [Display(Name = "Starttijd")]
        public DateTime Starttijd { get; set; }

        [Display(Name = "Opmerkingen")]
        public string? Opmerkingen { get; set; }

        // Herhalingsopties
        [Display(Name = "Herhaling")]
        public HerhaalPatroon Herhaling { get; set; } = HerhaalPatroon.Geen;

        [Display(Name = "Herhalen t.e.m.")]
        [DataType(DataType.Date)]
        public DateTime? HerhaalTot { get; set; }

        [ValidateNever] public SelectList? PatientenLijst { get; set; }
        [ValidateNever] public SelectList? TypenLijst { get; set; }

        // Custom validatie voor herhaling - controleert of er een einddatum is als er een herhaling is, en of die einddatum niet vóór de starttijd ligt
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Herhaling != HerhaalPatroon.Geen && !HerhaalTot.HasValue)
            {
                // yield return maakt het mogelijk om meerdere validatiefouten terug te geven zonder meteen te stoppen, wat handig is voor complexere validaties
                // De validatie controleert of er een einddatum is opgegeven als er een herhaling is ingesteld, wat logisch is omdat een herhaling zonder einddatum oneindig zou zijn
                yield return new ValidationResult(
                    "Kies een einddatum voor de herhaling.",
                    new[] { nameof(HerhaalTot) });
            }

            if (HerhaalTot.HasValue && HerhaalTot.Value.Date < Starttijd.Date)
            {
                // yield return maakt het mogelijk om meerdere validatiefouten terug te geven zonder meteen te stoppen, wat handig is voor complexere validaties
                // De validatie controleert of de herhaal-einddatum niet vóór de startdatum ligt, wat logisch is omdat een afspraak niet kan eindigen voordat hij begint
                yield return new ValidationResult(
                    "De herhaal-einddatum mag niet vóór de startdatum liggen.",
                    new[] { nameof(HerhaalTot) });
            }
        }
    }
}
