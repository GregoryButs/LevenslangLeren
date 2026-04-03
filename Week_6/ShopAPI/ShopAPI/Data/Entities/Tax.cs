namespace ShopAPI.Data.Entities
{
    public class Tax : BaseEntity
    {
        public string Name { get; set; }

        public TaxLevel TaxLevel { get; set; }

        public int TaxPercentage => (int)TaxLevel;

        public IEnumerable<Product> Products { get; set; }
    }

    public enum TaxLevel
    {
        Standaard = 21,
        UitzonderingenEnMaaltijden = 12,
        Basisnoden = 6
    }
}
