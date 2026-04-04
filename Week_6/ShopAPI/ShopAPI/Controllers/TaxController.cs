using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopAPI.Data;
using ShopAPI.Data.Entities;
using ShopAPI.DTO;

namespace ShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TaxController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("list")]
        public ActionResult<IEnumerable<TaxResponseDTO>> GetTaxes()
        {
            var taxes = _context.Taxes
                .AsNoTracking()
                .ToList();

            return Ok(_mapper.Map<IEnumerable<TaxResponseDTO>>(taxes));
        }

        [HttpGet("{id}")]
        public ActionResult<TaxResponseDTO> GetTax(int id)
        {
            var tax = _context.Taxes
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == id);

            if (tax == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<TaxResponseDTO>(tax));
        }

        [HttpPost("")]
        public ActionResult<TaxResponseDTO> CreateTax(TaxRequestDTO request)
        {
            var tax = _mapper.Map<Tax>(request);

            _context.Taxes.Add(tax);
            _context.SaveChanges();

            var response = _mapper.Map<TaxResponseDTO>(tax);

            return CreatedAtAction(nameof(GetTax), new { id = tax.Id }, response);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateTax(int id, TaxRequestDTO request)
        {
            var existingTax = _context.Taxes.Find(id);
            if (existingTax == null)
            {
                return NotFound();
            }

            _mapper.Map(request, existingTax);
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
