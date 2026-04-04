namespace ShopAPI.DTO
{
    public class ProductRequestDTO
    {
        public string Code { get; set; } 
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public decimal BuyPrice { get; set; }
        public int TaxesLevelId { get; set; }
        public int AmountStock { get; set; }
        public bool Active { get; set; }
    }
}