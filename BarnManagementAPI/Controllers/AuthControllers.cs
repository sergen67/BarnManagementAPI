using Barn.Api.Services;
using BarnManagementAPI.Data;
using BarnManagementAPI.Entities;
using BarnManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthResponse = BarnManagementAPI.Contracts.AuthResponse;
using RegisterRequest = BarnManagementAPI.Contracts.RegisterRequest;

namespace BarnManagementAPI.Controllers { 
    
    [ApiController]
    [Route("api/[controller]")]   
    public class AuthControllers : ControllerBase
    {
        private readonly BarnDbContext _db;
        private readonly JwtServices _jwt;
        public AuthControllers(BarnDbContext db, JwtServices jwt) { _db = db; _jwt = jwt; }
        [AllowAnonymous]    
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
        {
            if (await _db.Users.AnyAsync(u => u.Username == req.Username))
                return BadRequest("username already exists");

            var salt = PasswordHasher.NewSalt();
            var hash = PasswordHasher.Hash(req.Password, salt);

            var user = new User { Username = req.Username, PasswordSalt = salt, PasswordHash = hash, Role = req.Role ?? "User" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = _jwt.Create(user.Username, user.Role);
            return Ok(new AuthResponse(token, user.Username, user.Role));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(Contracts.LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Username & password zorunlu.");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user is null) return Unauthorized();

            var tryHash = PasswordHasher.Hash(req.Password, user.PasswordSalt);
            if (!tryHash.SequenceEqual(user.PasswordHash)) return Unauthorized();

            var token = _jwt.Create(user.Username, user.Role);
            return Ok(new AuthResponse(token, user.Username, user.Role));
        }
    }
}

