using System.ComponentModel.DataAnnotations;

namespace StartTestApi.DTO
{
    public class CreateInschrijvingDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string DeelnemerNaam { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string DeelnemerEmail { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int EventId { get; set; }
    }
}
