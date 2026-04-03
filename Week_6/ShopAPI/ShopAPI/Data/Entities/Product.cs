namespace ShopAPI.Data.Entities
{
    public class Product : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public decimal BuyPrice { get; set; }
        public int TaxesLevelId { get; set; }
        public int AmountStock { get; set; }
        public bool Active { get; set; }

        public Category Category { get; set; }
        public Tax TaxLevel { get; set; }

    }


}
