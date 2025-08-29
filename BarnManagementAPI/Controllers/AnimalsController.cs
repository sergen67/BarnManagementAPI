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
    [Route("[controller]")]
    [Authorize]
    public class AnimalsController : ControllerBase
    {
        private readonly BarnDbContext _db;
        public AnimalsController(BarnDbContext db) { _db = db; }

        [HttpGet]
        public async Task<ActionResult<List<AnimalDto>>> Get([FromQuery] int? barnId)
        {
            var now = DateTime.UtcNow;

            var q = _db.Animals
                .AsNoTracking()
                .Where(a => a.IsAlive);                 // ölüleri gizle

            if (barnId.HasValue)
                q = q.Where(a => a.BarnId == barnId.Value);

            var list = await q
                .Select(a => new AnimalDto(
                    a.Id,
                    a.Type ?? "",                    // entity’de Type kullanıyorsan burayı a.Type yap
                    a.Gender ?? "",
                    (int)Math.Max(0, (now - a.BornAt).TotalSeconds), // AgeDays alanına saniye yazıyoruz
                    a.LifeSpanDays,
                    a.IsAlive,
                    a.NextProductionAt
                ))
                .ToListAsync();

            return list;
        }



        [HttpGet("{id:int}")]
        public async Task<ActionResult<AnimalDto>> Get(int id)
        {
            var a = await _db.Animals.AsNoTracking()
                                     .FirstOrDefaultAsync(x => x.Id == id);

            if (a is null) return NotFound();

            var dto = new AnimalDto(
                Id: a.Id,
                Type: a.Type,
                Gender: a.Gender,
                AgeDays: (int)(DateTime.UtcNow - a.BornAt).TotalDays,
                LifeSpanDays: a.LifeSpanDays,
                IsAlive: a.IsAlive,
                NextProductionAt: a.NextProductionAt
            );

            return Ok(dto);
        }

        [HttpPost("{id:int}/mark-dead")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkDead(int id)
        {
            var a = await _db.Animals.FindAsync(id);
            if (a == null) return NotFound();
            if (!a.IsAlive) return BadRequest("Already dead.");
            a.IsAlive = false;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("purchase")]
        public async Task<ActionResult<AnimalDto>> Purchase([FromQuery] int barnId, [FromBody] PurchaseAnimalDto dto)
        {
            if (!await _db.Barns.AnyAsync(b => b.Id == barnId))
                return NotFound("Barn not found.");

            var now = DateTime.UtcNow;
            var (lifeSec, intervalSec) = DefaultsFor(dto.Species);

            var a = new Animal
            {
                BarnId = barnId,


                Type = dto.Species?.Trim() ?? "",

                Gender = string.IsNullOrWhiteSpace(dto.Gender) ? "female" : dto.Gender!.Trim(),


                LifeSpanDays = lifeSec,
                ProductionIntervalDays = intervalSec,

                BornAt = now,
                NextProductionAt = now,
                IsAlive = true
            };

            _db.Animals.Add(a);
            await _db.SaveChangesAsync();


            return Ok(ToDto(a));
        }

        private static (int lifeSec, int intervalSec) DefaultsFor(string? species)
        {
            var s = (species ?? "").ToLowerInvariant();
            if (s.Contains("inek") || s.Contains("cow")) return (120, 15);
            if (s.Contains("tavuk") || s.Contains("chicken")) return (30, 10);
            return (45, 12);
        }

        private static AnimalDto ToDto(Animal a)
        {

            var ageSeconds = (int)Math.Max(0, (DateTime.UtcNow - a.BornAt).TotalSeconds);

            return new AnimalDto(
                a.Id,
                a.Type ?? "",
                a.Gender ?? "",
                ageSeconds,
                a.LifeSpanDays,
                a.IsAlive,
                a.NextProductionAt
            );
        }


        [Authorize]
        [HttpPost("animals/{id:int}/sell")]
        public async Task<ActionResult<AnimalSaleResultDto>> Sell(int id, [FromBody] SellAnimalDto dto)
        {
            if (dto is null) return BadRequest("Body boş olamaz.");
            if (dto.Price <= 0) return BadRequest("Price > 0 olmalı.");

            var now = DateTime.UtcNow;


            var a = await _db.Animals.FirstOrDefaultAsync(x => x.Id == id);
            if (a is null) return NotFound("Animal not found.");

            if (!a.IsAlive)
                return BadRequest("Animal already not alive (satılmış/ölmüş).");

            var barn = await _db.Barns.FirstOrDefaultAsync(b => b.Id == a.BarnId);
            if (barn is null) return BadRequest("Animal'ın bağlı olduğu barn bulunamadı.");


            await using var tx = await _db.Database.BeginTransactionAsync();

            try
            {

                a.IsAlive = false;

                a.NextProductionAt = DateTime.MaxValue;


                barn.Balance += dto.Price;


                _db.Transactions.Add(new Transaction
                {
                    BarnId = barn.Id,
                    Amount = dto.Price,
                    Type = (TransactionType)1,
                    RefType = "AnimalSale",
                    RefId = a.Id,
                    CreatedAt = now
                });

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                var result = new AnimalSaleResultDto(
                    AnimalId: a.Id,
                    Price: dto.Price,
                    NewBarnBalance: barn.Balance,
                    SoldAtUtc: now
                );
                return Ok(result);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

    }
}
