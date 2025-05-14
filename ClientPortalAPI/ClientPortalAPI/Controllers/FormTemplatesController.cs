using ClientPortalAPI.Data;
using ClientPortalAPI.DTOs;
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
                                         Description = t.Description ?? "",
                                         BaseTemplateId = t.BaseTemplateId
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
                Fields = template.Fields
                    .OrderBy(f => f.FieldOrder)
                    .Select(f => new FormFieldDTO
                    {
                        FieldId = f.Id,
                        Label = f.Label,
                        FieldTypeName = f.FieldType.Name,
                        IsRequired = f.IsRequired,
                        Options = f.OptionsJson,
                        FieldOrder = f.FieldOrder
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
                        OptionsJson = fieldDto.Options,
                        FieldOrder = fieldDto.FieldOrder
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
                            Options = f.OptionsJson,
                            FieldOrder = f.FieldOrder
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
        public async Task<IActionResult> UpdateFormTemplate(int id, FormTemplateDTO templateDto)
        {
            if (id != templateDto.TemplateId)
            {
                return BadRequest();
            }

            var existingTemplate = await _context.FormTemplates
                .Include(t => t.Fields)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTemplate == null)
            {
                return NotFound();
            }

            try
            {
                // Update basic template properties
                existingTemplate.Name = templateDto.Name;
                existingTemplate.Description = templateDto.Description;

                // Remove fields that are no longer in the template
                var fieldsToRemove = existingTemplate.Fields
                    .Where(f => !templateDto.Fields.Any(dto => dto.FieldId == f.Id))
                    .ToList();

                foreach (var field in fieldsToRemove)
                {
                    _context.Remove(field);
                }

                // Update existing fields and add new ones
                foreach (var fieldDto in templateDto.Fields)
                {
                    var fieldType = await _context.FieldTypes
                        .FirstOrDefaultAsync(ft => ft.Name == fieldDto.FieldTypeName);

                    if (fieldType == null)
                    {
                        return BadRequest($"Invalid field type: {fieldDto.FieldTypeName}");
                    }

                    if (fieldDto.FieldId > 0)
                    {
                        // Update existing field
                        var existingField = existingTemplate.Fields
                            .FirstOrDefault(f => f.Id == fieldDto.FieldId);

                        if (existingField != null)
                        {
                            existingField.Label = fieldDto.Label;
                            existingField.FieldType = fieldType;
                            existingField.IsRequired = fieldDto.IsRequired;
                            existingField.OptionsJson = fieldDto.Options;
                            existingField.FieldOrder = fieldDto.FieldOrder;
                        }
                    }
                    else
                    {
                        // Add new field
                        existingTemplate.Fields.Add(new FormField
                        {
                            Label = fieldDto.Label,
                            FieldType = fieldType,
                            IsRequired = fieldDto.IsRequired,
                            OptionsJson = fieldDto.Options,
                            FieldOrder = fieldDto.FieldOrder
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FormTemplateExists(id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating template: {TemplateName}", templateDto.Name);
                return StatusCode(500, "An error occurred while updating the form template.");
            }
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
