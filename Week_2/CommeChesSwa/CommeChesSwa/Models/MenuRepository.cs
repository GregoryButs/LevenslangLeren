namespace CommeChesSwa.Models
{
    public class MenuRepository
    {
        private static List<Menu> _menus;

        public MenuRepository()
        {
            if (_menus == null)
            {
                Initialize();
            }
        }

        private static void Initialize()
        {
            List<Gerecht> ontbijtGerechten = new List<Gerecht>()
            {
                new Gerecht() { Naam="Croque Monsieur", Prijs=4.50, IsVeggie=false },
                new Gerecht() {Naam="Croque Madame met ham", Prijs=4.99, IsVeggie=false},
                new Gerecht() {Naam="Verse Croissants (x2)", Prijs = 3.50, IsVeggie = true}
            };

            Menu ontbijtMenu = new Menu() { Id = "ONT", Naam = "Ontbijtmenu", Gerechten = ontbijtGerechten };

            List<Gerecht> lunchGerechten = new List<Gerecht>()
            {
                new Gerecht() { Naam="Stoofvlees met frietjes", Prijs=14.95, IsVeggie=false },
                new Gerecht() {Naam="Vol-au-vent met puree", Prijs=16.50, IsVeggie=false},
                new Gerecht() {Naam="Vegetarische Luikse Salade", Prijs = 12.90, IsVeggie = true}
            };

            Menu lunchMenu = new Menu() { Id = "LUNCH", Naam = "Lunchmenu", Gerechten = lunchGerechten };

            List<Gerecht> suggestieGerechten = new List<Gerecht>()
            {
                new Gerecht() {Naam="Verse tomatensoep", Prijs=6.50, IsVeggie=true},
                new Gerecht() { Naam="Scampi's van de chef", Prijs=17.95, IsVeggie=false },
                new Gerecht() {Naam="Duo van kaas-en garnaalkroketten", Prijs=18.95, IsVeggie=false},
                new Gerecht() {Naam="Veggie tikka masala met zoete aardappel", Prijs = 17.00, IsVeggie = true}
            };
            Menu suggestieMenu = new Menu() { Id = "SUGG", Naam = "Suggestiemenu", Gerechten = suggestieGerechten };

            _menus = new List<Menu>() { ontbijtMenu, lunchMenu, suggestieMenu };
        }

        public IEnumerable<Menu> GetAll()
        {
            return _menus;
        }

        public Menu GetById(string id)
        {
            return _menus.Find(x => x.Id == id);
        }
    }

}
