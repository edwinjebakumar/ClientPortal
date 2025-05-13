using ClientPortalAPI.Data;
using ClientPortalAPI.DTOs.ClientPortalAPI.DTOs;
using ClientPortalAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ClientPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FieldTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FieldTypesController(ApplicationDbContext context)
        {
            _context = context;
            // Ensure field types exist
            EnsureFieldTypes().Wait();
        }

        // GET: api/FieldTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FieldTypeDTO>>> GetFieldTypes()
        {
            var fieldTypes = await _context.FieldTypes
                                        .Select(c => new FieldTypeDTO
                                        {
                                            Id = c.Id,
                                            Name = c.Name,
                                        })
                                        .ToListAsync();

            return Ok(fieldTypes);
        }

        private async Task EnsureFieldTypes()
        {
            if (!await _context.FieldTypes.AnyAsync())
            {
                var defaultTypes = new[]
                {
                    new FieldType { Name = "Text" },
                    new FieldType { Name = "Number" },
                    new FieldType { Name = "Date" },
                    new FieldType { Name = "Dropdown" },
                    new FieldType { Name = "Checkbox" },
                    new FieldType { Name = "Radio" },
                    new FieldType { Name = "Email" },
                    new FieldType { Name = "Phone" }
                };

                await _context.FieldTypes.AddRangeAsync(defaultTypes);
                await _context.SaveChangesAsync();
            }
        }
    }
}
