using BarnManagementAPI.Contracts;
using BarnManagementAPI.Data;
using BarnManagementAPI.Entities;
using BarnManagementAPI.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BarnController : ControllerBase
    {
        private readonly BarnDbContext _db;
        public BarnController(BarnDbContext db) { _db = db; }

        [HttpPost]
        public async Task<ActionResult<Barns>> Create([FromBody] CreateBarnDto dto)
        {
            var username = User.Identity?.Name;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user is null) return Unauthorized();

            var barn = new Barns
            {
                Name = dto.Name,
                Capacity = dto.Capacity <= 0 ? 20 : dto.Capacity,
                Balance = dto.InitialBalance,
                UserId = user.Id
            };

            _db.Barns.Add(barn);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = barn.Id }, barn);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Barns>> Get(int id)
        {
            var barn = await _db.Barns.Include(b => b.Animals).FirstOrDefaultAsync(b => b.Id == id);
            if (barn == null) return NotFound();
            return barn;
        }

       
        [HttpPost("{id:int}/deposit")]
        public async Task<IActionResult> Deposit(int id, [FromBody] BalanceDto dto)
        {
            if (dto.Amount <= 0) return BadRequest("Amount must be > 0");
            var barn = await _db.Barns.FirstOrDefaultAsync(b => b.Id == id);
            if (barn is null) return NotFound();
            barn.Balance += dto.Amount;
            _db.Transactions.Add(new Transaction
            {
                BarnId = barn.Id,
                Type = TransactionType.Income,
                Amount = dto.Amount,
                RefType = "Deposit"
            });
            await _db.SaveChangesAsync();
            return Ok(new { barn.Id, barn.Balance });
        }
    }
}
