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
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoryController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("list")]
        public ActionResult<IEnumerable<CategoryResponseDTO>> GetCategories()
        {
            var categories = _context.Categories
                .AsNoTracking()
                .ToList();

            return Ok(_mapper.Map<IEnumerable<CategoryResponseDTO>>(categories));
        }

        [HttpGet("{id}")]
        public ActionResult<CategoryResponseDTO> GetCategory(int id)
        {
            var category = _context.Categories
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CategoryResponseDTO>(category));
        }

        [HttpPost("")]
        public ActionResult<CategoryResponseDTO> CreateCategory(CategoryRequestDTO request)
        {
            var category = _mapper.Map<Category>(request);

            _context.Categories.Add(category);
            _context.SaveChanges();

            var response = _mapper.Map<CategoryResponseDTO>(category);

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, response);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateCategory(int id, CategoryRequestDTO request)
        {
            var existingCategory = _context.Categories.Find(id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            _mapper.Map(request, existingCategory);
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
