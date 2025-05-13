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
        private readonly ILogger<FormTemplatesController> _logger;

        public FormTemplatesController(ApplicationDbContext context, ILogger<FormTemplatesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/FormTemplates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FormTemplateDTO>>> GetFormTemplates()
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
        public async Task<ActionResult<FormTemplateDTO>> GetFormTemplate(int id)
        {
            var template = await _context.FormTemplates
                                     .Include(ft => ft.Fields)
                                     .ThenInclude(f => f.FieldType)
                                     .FirstOrDefaultAsync(ft => ft.Id == id);

            if (template == null)
            {
                return NotFound();
            }

            return new FormTemplateDTO
            {
                TemplateId = template.Id,
                Name = template.Name,
                Description = template.Description,
                Fields = template.Fields.Select(f => new FormFieldDTO
                {
                    FieldId = f.Id,
                    Label = f.Label,
                    FieldTypeName = f.FieldType.Name,
                    IsRequired = f.IsRequired,
                    Options = f.OptionsJson
                }).ToList()
            };
        }

        // POST: api/FormTemplates
        [HttpPost]
        public async Task<ActionResult<FormTemplateDTO>> CreateFormTemplate(FormTemplateDTO templateDto)
        {
            try
            {
                _logger.LogInformation("Creating template: {TemplateName}", templateDto.Name);

                var template = new FormTemplate
                {
                    Name = templateDto.Name,
                    Description = templateDto.Description,
                    Fields = new List<FormField>()
                };

                // Process fields
                foreach (var fieldDto in templateDto.Fields)
                {
                    var fieldType = await _context.FieldTypes
                        .FirstOrDefaultAsync(ft => ft.Name == fieldDto.FieldTypeName);

                    if (fieldType == null)
                    {
                        return BadRequest($"Invalid field type: {fieldDto.FieldTypeName}");
                    }

                    var field = new FormField
                    {
                        Label = fieldDto.Label,
                        FieldType = fieldType,
                        IsRequired = fieldDto.IsRequired,
                        OptionsJson = fieldDto.Options
                    };

                    template.Fields.Add(field);
                }

                _context.FormTemplates.Add(template);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created template: {TemplateName} with {FieldCount} fields", 
                    templateDto.Name, templateDto.Fields.Count);

                return CreatedAtAction(
                    nameof(GetFormTemplate), 
                    new { id = template.Id }, 
                    new FormTemplateDTO
                    {
                        TemplateId = template.Id,
                        Name = template.Name,
                        Description = template.Description,
                        Fields = template.Fields.Select(f => new FormFieldDTO
                        {
                            FieldId = f.Id,
                            Label = f.Label,
                            FieldTypeName = f.FieldType.Name,
                            IsRequired = f.IsRequired,
                            Options = f.OptionsJson
                        }).ToList()
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating template: {TemplateName}", templateDto.Name);
                return StatusCode(500, "An error occurred while creating the form template.");
            }
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
