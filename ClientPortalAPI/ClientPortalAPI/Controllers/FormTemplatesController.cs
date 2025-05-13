using ClientPortalAPI.Data;
using ClientPortalAPI.DTOs.ClientPortalAPI.DTOs;
using ClientPortalAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormTemplatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FormTemplatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/FormTemplates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FormTemplate>>> GetFormTemplates()
        {
            var formTemplates = await _context.FormTemplates
                                     .Select(t => new FormTemplateDTO
                                     {
                                         TemplateId = t.Id,
                                         Name = t.Name,
                                         Description = t.Description ?? ""
                                     })
                                     .ToListAsync();
            return Ok(formTemplates);
        }

        // GET: api/FormTemplates/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FormTemplate>> GetFormTemplate(int id)
        {
            var template = await _context.FormTemplates
                                         .Include(ft => ft.Fields)
                                         .FirstOrDefaultAsync(ft => ft.Id == id);

            if (template == null)
            {
                return NotFound();
            }

            return template;
        }

        // POST: api/FormTemplates
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FormTemplateDTO>> CreateFormTemplate(FormTemplateDTO templateDto)
        {
            var template = new FormTemplate
            {
                Name = templateDto.Name,
                Description = templateDto.Description
            };

            _context.FormTemplates.Add(template);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFormTemplates), new { id = template.Id }, template);
        }


        // PUT: api/FormTemplates/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateFormTemplate(int id, FormTemplate template)
        {
            if (id != template.Id)
            {
                return BadRequest();
            }

            _context.Entry(template).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FormTemplateExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/FormTemplates/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFormTemplate(int id)
        {
            var template = await _context.FormTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            _context.FormTemplates.Remove(template);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FormTemplateExists(int id) =>
            _context.FormTemplates.Any(ft => ft.Id == id);
    }

}
