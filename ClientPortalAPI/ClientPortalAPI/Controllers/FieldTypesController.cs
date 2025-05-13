using ClientPortalAPI.Data;
using ClientPortalAPI.DTOs.ClientPortalAPI.DTOs;
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
    }
}
