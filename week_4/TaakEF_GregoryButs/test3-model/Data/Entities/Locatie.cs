using test4_start.Data.Entities;

namespace test3_model.Data.Entities
{
    public class Locatie : BaseEntity
    {
        public string Naam { get; set; }
        public ICollection<Event> Events { get; set; } = new List<Event>();

    }
}