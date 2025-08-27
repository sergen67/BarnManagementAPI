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
        public async Task<ActionResult<List<AnimalDto>>> Get()
        {
            var now = DateTime.UtcNow;

            var list = await _db.Animals
             .AsNoTracking()
            .Select(a => new AnimalDto(
                a.Id,
                a.Type,
                a.Gender,
                EF.Functions.DateDiffDay(a.BornAt, now),
                a.LifeSpanDays,
                a.IsAlive,
                a.NextProductionAt
                   ))
              .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int barnId, [FromBody] AnimalCreateDtos dto)
        {
            var barn = await _db.Barns.Include(b => b.Animals)
                                      .FirstOrDefaultAsync(b => b.Id == barnId);
            if (barn is null) return NotFound("Barn not found.");



            var now = DateTime.UtcNow;

            var animal = new Animal
            {
                BarnId = barn.Id,
                Type = dto.Type,
                Gender = dto.Gender,
                LifeSpanDays = dto.LifeSpanDays,
                ProductionIntervalDays = dto.ProductionIntervalDays,
                BornAt = now,
                NextProductionAt = now.AddDays(dto.ProductionIntervalDays),
                IsAlive = true
            };

            _db.Animals.Add(animal);
            await _db.SaveChangesAsync();
            return Ok(animal);
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
        
        [HttpPost("barns/{barnId:int}/animals/purchase")]
        public async Task<IActionResult> Purchase(int barnId, [FromBody] PurchaseAnimalDto dto)
        {
            var barn = await _db.Barns
                .Include(b => b.Animals)
                .FirstOrDefaultAsync(b => b.Id == barnId);
            if (barn is null) return NotFound("Barn not found.");

            if (barn.Animals.Count(a => a.IsAlive) >= barn.Capacity)
                return BadRequest("Capacity reached.");
            if (barn.Balance < dto.Price)
                return BadRequest("Insufficient balance.");
            var now = DateTime.UtcNow;
            var animal = new Animal
            {
                BarnId = barn.Id,
                Type = dto.Type,
                Gender = dto.Gender,
                LifeSpanDays = dto.LifeSpanDays,
                ProductionIntervalDays = dto.ProductionIntervalDays,
                NextProductionAt = now.AddSeconds(5),
                IsAlive = true
            };

            barn.Balance -= dto.Price;

            _db.Animals.Add(animal);
            _db.Transactions.Add(new Transaction
            {
                BarnId = barn.Id,
                Type = TransactionType.Purchase,
                Amount = dto.Price,
                RefType = "Animal"
            });

            await _db.SaveChangesAsync();
            return Ok(animal);
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
