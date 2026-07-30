using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AfsprakenbeheerPsycholoog.Controllers.Api
{
    [ApiController]
    [Route("api/afspraaktype")]
    [Authorize]
    public class ApiAfspraakTypeController : ControllerBase
    {
        private readonly IAfspraakTypeService _service;

        public ApiAfspraakTypeController(IAfspraakTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public IActionResult GetAlleTypes()
        {
            var types = _service.GetAlleTypes();
            return Ok(types);
        }

        [HttpGet("{id}")]
        public IActionResult GetTypeById(int id)
        {
            var type = _service.GetTypeById(id);
            if (type == null) return NotFound(new { message = "Type niet gevonden." });
            return Ok(type);
        }

        [HttpPost]
        [Authorize(Policy = "PsycholoogOnly")]
        public IActionResult CreateType([FromBody] AfspraakType model)
        {
            model.Id = 0;
            ModelState.Remove(nameof(AfspraakType.Id));
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _service.CreateType(model);
            return CreatedAtAction(nameof(GetTypeById), new { id = model.Id }, model);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "PsycholoogOnly")]
        public IActionResult EditType(int id, [FromBody] AfspraakType model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != model.Id) return BadRequest(new { message = "ID in URL komt niet overeen met ID in body." });

            var type = _service.GetTypeById(id);
            if (type == null) return NotFound(new { message = "Type niet gevonden." });

            _service.EditType(model);
            return Ok(new { message = "Type succesvol bijgewerkt." });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "PsycholoogOnly")]
        public IActionResult DeleteType(int id)
        {
            _service.DeleteType(id);
            return Ok(new { message = "Type succesvol verwijderd." });
        }
    }
}
