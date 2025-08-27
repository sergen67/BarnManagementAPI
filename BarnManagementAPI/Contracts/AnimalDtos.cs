namespace BarnManagementAPI.Contracts
{
  
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
