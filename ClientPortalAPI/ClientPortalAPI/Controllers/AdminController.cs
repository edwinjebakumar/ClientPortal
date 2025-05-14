using ClientPortalAPI.Data;
using ClientPortalAPI.DTOs;
using ClientPortalAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> CreateTemplate(FormTemplateDTO dto)
        {
            var template = new FormTemplate
            {
                Name = dto.Name,
                Description = dto.Description,
                Fields = dto.Fields.Select(f => new FormField
                {
                    Label = f.Label,
                    FieldType = new FieldType { Id = f.FieldId, Name = f.FieldTypeName},
                    OptionsJson = f.Options,
                }).ToList()
            };

            _context.FormTemplates.Add(template);
            await _context.SaveChangesAsync();

            return RedirectToAction("FormTemplates");
        }

    }
}
