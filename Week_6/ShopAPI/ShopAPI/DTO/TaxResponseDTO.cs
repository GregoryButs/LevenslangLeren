using ShopAPI.Data.Entities;

namespace ShopAPI.DTO
{
    public class TaxResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TaxLevel TaxLevel { get; set; }
        public int TaxPercentage { get; set; }
    }
}