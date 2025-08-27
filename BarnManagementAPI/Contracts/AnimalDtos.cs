namespace BarnManagementAPI.Contracts
{
    //public class AnimalCreateDtos
    //{
    //    public string Type { get; set; } = string.Empty;
    //    public int AgeDays { get; set; }
    //    public string Gender { get; set; } = "female";
    //    public int LifeSpanDays { get; set; } = 180;
    //    public int ProductionIntervalDays { get; set; } = 7;
    //    public decimal Price { get; set; } = 100m;
    //    public DateTime NextProductionAt { get; set; } = DateTime.UtcNow.AddDays(1);
    //    public bool IsAlive { get; set; } = true;
    //}
    public record AnimalDto(
        int Id,
        string Type,
        string Gender,
        int AgeDays,
        int LifeSpanDays,
        bool IsAlive,
        DateTime NextProductionAt
    );
}
