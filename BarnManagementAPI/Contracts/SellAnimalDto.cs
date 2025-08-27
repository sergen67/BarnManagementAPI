using System.ComponentModel.DataAnnotations;

namespace BarnManagementAPI.Contracts
{
    public class SellAnimalDto
    {
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
    }
    public record AnimalSaleResultDto(
        int AnimalId,
        decimal Price,
        decimal NewBarnBalance,
        DateTime SoldAtUtc
    );  
}
