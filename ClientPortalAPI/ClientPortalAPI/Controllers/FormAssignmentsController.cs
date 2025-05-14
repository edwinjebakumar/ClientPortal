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
    public class FormAssignmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FormAssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/FormAssignments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FormAssignmentDTO>>> GetAssignments()
        {
            var dtoList = await _context.FormAssignments
                                .Include(a => a.Client)
                                .Include(a => a.FormTemplate)
                                .Select(a => new FormAssignmentDTO
                                {
                                    FormTemplateId = a.FormTemplateId,
                                    ClientId = a.ClientId,
                                    FormTemplateName = a.FormTemplate.Name,
                                    Description = a.FormTemplate.Description ?? "",
                                    AssignedAt = a.AssignedAt
                                })
                                .ToListAsync();

            return Ok(dtoList);
        }
        // POST: api/FormAssignments
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FormAssignment>> CreateAssignment(FormAssignment assignment)
        {
            // Validate that the same template is not assigned twice for a client
            var exists = await _context.FormAssignments.AnyAsync(a =>
                a.ClientId == assignment.ClientId && a.FormTemplateId == assignment.FormTemplateId);

            if (exists)
            {
                return Conflict("This form is already assigned to the client.");
            }

            _context.FormAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAssignments), new { id = assignment.Id }, assignment);
        }

        [HttpPost("assign")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FormAssignmentDTO>> AssignTemplateToClient(FormAssignmentDTO assignmentDto)
        {
            var assignment = new FormAssignment
            {
                ClientId = assignmentDto.ClientId,
                FormTemplateId = assignmentDto.FormTemplateId,
                AssignedAt = DateTime.Now
            };

            // Validate if assignment already exists
            var exists = await _context.FormAssignments.AnyAsync(a =>
                a.ClientId == assignment.ClientId && a.FormTemplateId == assignment.FormTemplateId);
            if (exists)
            {
                return Conflict("This template is already assigned to the client.");
            }

            _context.FormAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAssignments), new { id = assignment.Id }, assignment);
        }

    }

}
