using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


namespace BarnManagementAPI.Services
{

    public class JwtServices
    {

        private readonly SymmetricSecurityKey _signingKey;
        private readonly JwtOptions _opt;

        public JwtServices(IOptions<JwtOptions> opt)
        {
            _opt = opt.Value ?? throw new ArgumentNullException(nameof(opt));
            if (string.IsNullOrWhiteSpace(_opt.Key) || Encoding.UTF8.GetByteCount(_opt.Key) < 32)
                throw new InvalidOperationException("Jwt:Key missing or too short (>=32 bytes).");

            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        }

        public string Create(string username,string role)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,username),
                new Claim(ClaimTypes.Name,username),
                new Claim(ClaimTypes.Role,role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims : claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
