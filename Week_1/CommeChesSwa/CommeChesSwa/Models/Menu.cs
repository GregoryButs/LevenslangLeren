namespace CommeChesSwa.Models
{
    public class Menu
    {
        public string Id { get; set; }
        public string Naam { get; set; }
        public IEnumerable<Gerecht> Gerechten { get; set; }

    }
}
