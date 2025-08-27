namespace BarnManagementAPI.Contracts
{
    public class CreateBarnDto
    {
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; } = 20;
        public decimal InitialBalance { get; set; } = 0m; 
    }
}
