using Microsoft.AspNetCore.Mvc;
using ShopAPI.Data;
using ShopAPI.Data.Entities;

namespace ShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("list")]
        public ActionResult<IEnumerable<Product>> GetProducts()
        {
            // enkel producten ophalen waarvan Active = true
            var products = _context.Products.Where(p => p.Active).ToList();
            return Ok(products);
        }

        [HttpGet("inactive")]
        public ActionResult<IEnumerable<Product>> GetInactiveProducts()
        {
            // enkel producten ophalen waarvan Active = false
            var products = _context.Products.Where(p => !p.Active).ToList();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost("")]
        public ActionResult<Product> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }
            var existingProduct = _context.Products.Find(id);
            if (existingProduct == null)
            {
                return NotFound();
            }
            existingProduct.Code = product.Code;
            existingProduct.Name = product.Name;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.BuyPrice = product.BuyPrice;
            existingProduct.TaxesLevelId = product.TaxesLevelId;
            existingProduct.AmountStock = product.AmountStock;
            existingProduct.Active = product.Active;
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
            // Fix: Cast TaxLevel enum to int to allow division
            double sellingPrice = (double)product.BuyPrice * 1.2 * (1.0 + ((int)tax.TaxLevel / 100.0));
            return Ok(sellingPrice);
        }

        [HttpGet("name/{name}")]
        public ActionResult<Product> GetProductByName(string name)
        {
            var product = _context.Products.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet("tax/{taxLevel}")]
        public ActionResult<IEnumerable<Product>> GetProductsByTaxLevel(int taxLevel)
        {
            var products = _context.Products.Where(p => p.TaxesLevelId == taxLevel).ToList();
            return Ok(products);
        }
    }
}
