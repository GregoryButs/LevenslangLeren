using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopAPI.Data;
using ShopAPI.Data.Entities;

namespace ShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TaxController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("list")]
        public ActionResult<IEnumerable<Tax>> GetTaxes()
        {
            var taxes = _context.Taxes
                .Include(t => t.Products)
                .ToList();

            return Ok(taxes);
        }

        [HttpGet("{id}")]
        public ActionResult<Tax> GetTax(int id)
        {
            var tax = _context.Taxes
                .Include(t => t.Products)
                .FirstOrDefault(t => t.Id == id);

            if (tax == null)
            {
                return NotFound();
            }
            return Ok(tax);
        }

        [HttpPost("")]
        public ActionResult<Tax> CreateTax(Tax tax)
        {
            _context.Taxes.Add(tax);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetTax), new { id = tax.Id }, tax);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateTax(int id, Tax tax)
        {
            if (id != tax.Id)
            {
                return BadRequest();
            }
            var existingTax = _context.Taxes.Find(id);
            if (existingTax == null)
            {
                return NotFound();
            }
            existingTax.Name = tax.Name;
            existingTax.TaxLevel = tax.TaxLevel;
            _context.SaveChanges();
            return NoContent();

        }

        [HttpDelete("{id}")]
        public ActionResult DeleteTax(int id)
        {
            var tax = _context.Taxes.Find(id);
            if (tax == null)
            {
                return NotFound();
            }
            _context.Taxes.Remove(tax);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
