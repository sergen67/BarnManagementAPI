using System.ComponentModel.DataAnnotations;

namespace BarnManagementAPI.Contracts
{
    public class PurchaseAnimalDto
    {
        [Required] public string Type { get; set; } = string.Empty; 
        [Required] public string Gender { get; set; } = "female";    
        [Range(1, 3650)] public int LifeSpanDays { get; set; } = 180;
        [Range(1, 365)] public int ProductionIntervalDays { get; set; } = 7;
        [Range(0.01, double.MaxValue)] public decimal Price { get; set; } = 100m;
    }
}
