using System.ComponentModel.DataAnnotations;

namespace BarnManagementAPI.Contracts
{
    public class ProduceDto
    {
        [Required] public int AnimalId { get; set; }
        [Range(1, int.MaxValue)] public int Quantity { get; set; } = 1;
        public string? ProductType { get; set; }
    }
    public record ProductDto(
        int Id, int? AnimalId, string Type, int Quantity, bool IsSold, DateTime ProducedAt
    );

}
