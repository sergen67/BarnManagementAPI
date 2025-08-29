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
    public class ProductController : ControllerBase
    {

        private readonly BarnDbContext _db;
        public ProductController(BarnDbContext db) { _db = db; }

        [HttpGet("available")]
        public async Task<ActionResult<List<ProductDto>>> Available()
        {
            var data = await _db.Products
                .AsNoTracking()
                .Where(p => !p.Issold)                    // ← doğru ad
                .Select(p => new ProductDto(
                   p.Id, p.AnimaId, p.ProductType, p.Quantity, p.Issold, p.ProductAt
                ))
                .ToListAsync();

            return Ok(data);
        }
        [HttpPost("{id:int}/sell")]
        public async Task<IActionResult> Sell(int id, [FromQuery] decimal price)
        {
            var p = await _db.Products
                .Include(x => x.Animal)
                .ThenInclude(a => a.Barn)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            if (p.Issold) return BadRequest("Already sold.");

            p.Issold = true;
            p.SoldPrice = price;

            var barn = p.Animal.Barn;
            barn.Balance += price;

            _db.Transactions.Add(new Transaction
            {
                BarnId = barn.Id,
                Type = TransactionType.Sale,
                Amount = price,
                RefType = "Product",
                RefId = p.Id
            });

            await _db.SaveChangesAsync();
            return Ok(new { ok = true, barn.Balance });
        }
    }
}
