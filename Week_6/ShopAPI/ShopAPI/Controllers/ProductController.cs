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
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProductController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("list")]
        public ActionResult<IEnumerable<ProductResponseDTO>> GetProducts()
        {
            var products = _context.Products
                .AsNoTracking()
                .Where(p => p.Active)
                .Include(p => p.Category)
                .Include(p => p.TaxLevel)
                .ToList();

            return Ok(_mapper.Map<IEnumerable<ProductResponseDTO>>(products));
        }

        [HttpGet("{id}")]
        public ActionResult<ProductResponseDTO> GetProduct(int id)
        {
            var product = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.TaxLevel)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ProductResponseDTO>(product));
        }

        [HttpPost("")]
        public ActionResult<ProductResponseDTO> CreateProduct(ProductRequestDTO request)
        {
            var categoryExists = _context.Categories.Any(c => c.Id == request.CategoryId);
            var taxExists = _context.Taxes.Any(t => t.Id == request.TaxesLevelId);

            if (!categoryExists || !taxExists)
            {
                return BadRequest("Invalid CategoryId or TaxesLevelId.");
            }

            var product = _mapper.Map<Product>(request);

            _context.Products.Add(product);
            _context.SaveChanges();

            var createdProduct = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.TaxLevel)
                .First(p => p.Id == product.Id);

            var response = _mapper.Map<ProductResponseDTO>(createdProduct);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, response);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateProduct(int id, ProductRequestDTO request)
        {
            var existingProduct = _context.Products.Find(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            var categoryExists = _context.Categories.Any(c => c.Id == request.CategoryId);
            var taxExists = _context.Taxes.Any(t => t.Id == request.TaxesLevelId);

            if (!categoryExists || !taxExists)
            {
                return BadRequest("Invalid CategoryId or TaxesLevelId.");
            }

            _mapper.Map(request, existingProduct);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            _context.SaveChanges();
            return NoContent();
        }

        [HttpGet("{id}/sellingprice")]
        public ActionResult<double> GetSellingPrice(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            var tax = _context.Taxes.Find(product.TaxesLevelId);
            if (tax == null)
            {
                return NotFound();
            }

            var sellingPrice = (double)product.BuyPrice * 1.2 * (1.0 + ((int)tax.TaxLevel / 100.0));
            return Ok(sellingPrice);
        }

        [HttpGet("inactive")]
        public ActionResult<IEnumerable<ProductResponseDTO>> GetInactiveProducts()
        {
            var products = _context.Products
                .AsNoTracking()
                .Where(p => !p.Active)
                .Include(p => p.Category)
                .Include(p => p.TaxLevel)
                .ToList();

            return Ok(_mapper.Map<IEnumerable<ProductResponseDTO>>(products));
        }

        [HttpGet("name/{name}")]
        public ActionResult<ProductResponseDTO> GetProductByName(string name)
        {
            var product = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.TaxLevel)
                .FirstOrDefault(p => p.Name == name);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ProductResponseDTO>(product));
        }

        [HttpGet("tax/{taxLevelId}")]
        public ActionResult<IEnumerable<ProductResponseDTO>> GetProductsByTaxLevel(int taxLevelId)
        {
            var products = _context.Products
                .AsNoTracking()
                .Where(p => p.TaxesLevelId == taxLevelId)
                .Include(p => p.Category)
                .Include(p => p.TaxLevel)
                .ToList();

            return Ok(_mapper.Map<IEnumerable<ProductResponseDTO>>(products));
        }
    }
}
