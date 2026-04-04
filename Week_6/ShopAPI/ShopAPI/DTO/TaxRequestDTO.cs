using ShopAPI.Data.Entities;

namespace ShopAPI.DTO
{
    public class TaxRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public TaxLevel TaxLevel { get; set; }
    }
}