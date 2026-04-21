using AfsprakenbeheerPsycholoog.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AfsprakenbeheerPsycholoog.Helpers
{
    /// <summary>
    /// Helper class voor het genereren van SelectLists voor dropdowns in de views, zoals lijsten van patiënten, afspraaktypes en statussen.
    /// </summary>

    public class SelectListHelper
    {
        // Deze helper-methoden maken het makkelijker om SelectLists te maken

        // Deze helper maakt een SelectList van patiënten, gesorteerd op naam
        public static SelectList Patienten(
            IEnumerable<Patient> patienten, int? selectedId = null)
        {
            var items = patienten
                .Select(p => new { p.Id, Naam = $"{p.Voornaam} {p.Achternaam}" })
                .OrderBy(p => p.Naam)
                .ToList();
            return new SelectList(items, "Id", "Naam", selectedId);
        }

        // Deze helper maakt een SelectList van AfspraakType, met tijdsduur!
        public static SelectList Types(
            IEnumerable<AfspraakType> types, int? selectedId = null)
        {
            var items = types
                .OrderBy(t => t.Naam)
                .Select(t => new
                {
                    Id = t.Id,
                    WeergaveNaam = $"{t.Naam} ({t.StandaardDuurMinuten} min)"
                })
                .ToList();

            return new SelectList(items, "Id", "WeergaveNaam", selectedId);
        }

        // Deze helper maakt een SelectList van de AfspraakStatus enum, gesorteerd op naam
        public static SelectList Statussen(AfspraakStatus? selected = null)
        {
            var items = Enum.GetValues<AfspraakStatus>()
                .Select(s => new { Value = s, Text = s.ToString() })
                .ToList();
            return new SelectList(items, "Value", "Text", selected);
        }
    }
}
