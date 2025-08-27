using System.Text.Json.Serialization;
using BarnManagementAPI.Enum;
namespace BarnManagementAPI.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int BarnId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string? RefType { get; set; }  
        public int? RefId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [JsonIgnore]
        public Barns Barn { get; set; } = null!;
    }
}
