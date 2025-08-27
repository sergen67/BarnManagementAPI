namespace BarnManagementAPI.Entities;

public class Barns
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; } = 20;
    public decimal Balance { get; set; } = 0m;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
    public ICollection<Animal> Animals { get; set; } = new List<Animal>();
}
