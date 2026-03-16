using test4_start.Data.Entities;

namespace test3_model.Data.Entities
{
    public class Begeleider : BaseEntity
    {
        public string Naam { get; set; } = null!;
        public string Functie { get; set; } = null!;
        public string UrlFoto { get; set; }

        // Navigatie
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
