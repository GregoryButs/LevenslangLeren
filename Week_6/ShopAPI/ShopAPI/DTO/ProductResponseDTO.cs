using ShopAPI.Data.Entities;

namespace ShopAPI.DTO
{
    public class ProductResponseDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal BuyPrice { get; set; }
        public int AmountStock { get; set; }
        public bool Active { get; set; }
        public ProductCategoryDTO Category { get; set; } = new();
        public ProductTaxDTO Tax { get; set; } = new();
    }

    public class ProductCategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ProductTaxDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TaxLevel TaxLevel { get; set; }
        public int TaxPercentage { get; set; }
    }
}