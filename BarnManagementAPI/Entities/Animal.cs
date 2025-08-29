using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BarnManagementAPI.Entities

{
        public class Animal
        {
            public int Id { get; set; }
            public int BarnId { get; set; }
            public string Type { get; set; } = string.Empty;
            public string Gender { get; set; } = "female";
            public bool IsAlive { get; set; } = true;
            public DateTime BornAt { get; set; } = DateTime.UtcNow; 
            public int LifeSpanDays { get; set; } = 180;           
            public int ProductionIntervalDays { get; set; } = 7;   
            public DateTime NextProductionAt { get; set; } = DateTime.UtcNow.AddDays(7);
       
           

            [JsonIgnore]
            public Barns Barn { get; set; } = null!;
        }
}
