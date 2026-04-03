using Microsoft.AspNetCore.Mvc;
using ShopAPI.Data;
using ShopAPI.Data.Entities;

namespace ShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("list")]
        public ActionResult<IEnumerable<Category>> GetCategories()
        {
            var categories = _context.Categories.ToList();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public ActionResult<Category> GetCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        [HttpGet("{id}/products")]
        public ActionResult<IEnumerable<Product>> GetCategoryProducts(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            var products = _context.Products.Where(p => p.CategoryId == id).ToList();
            return Ok(products);
        }

        [HttpPost("")]
        public ActionResult<Category> CreateCategory(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }
            var existingCategory = _context.Categories.Find(id);
            if (existingCategory == null)
            {
                return NotFound();
            }
            existingCategory.Name = category.Name;
            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            _context.Categories.Remove(category);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
