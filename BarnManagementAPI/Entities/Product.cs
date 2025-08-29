using System.Text.Json.Serialization;
namespace BarnManagementAPI.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public int? AnimaId { get; set; }
        public required string ProductType { get; set; }
        public int Quantity { get; set; }
        public DateTime ProductAt {  get; set; } = DateTime.UtcNow;
        public bool Issold {  get; set; }

        public decimal? SoldPrice { get; set; }
        [JsonIgnore]
        public Animal? Animal { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
