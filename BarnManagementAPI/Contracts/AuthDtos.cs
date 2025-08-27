namespace BarnManagementAPI.Contracts
{
    public record RegisterRequest(string Username,string Password,string? Role);
    public record LoginRequest(string Username, string Password);
}
