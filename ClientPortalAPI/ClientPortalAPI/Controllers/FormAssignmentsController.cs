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
            try
            {
                var dtoList = await _context.FormAssignments
                    .Include(a => a.Client)
                    .Include(a => a.FormTemplate)
                    .Select(a => new FormAssignmentDTO
                    {
                        Id = a.Id,
                        FormTemplateId = a.FormTemplateId,
                        ClientId = a.ClientId,
                        FormTemplateName = a.FormTemplate.Name,
                        Description = a.FormTemplate.Description ?? "",
                        AssignedAt = a.AssignedAt,
                        Status = a.Status
                    })
                    .ToListAsync();

                return Ok(dtoList);
            }            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving form assignments: {ex.Message}");
            }
        }

 
        [HttpGet("{clientId}")]
        public async Task<ActionResult<IEnumerable<FormAssignmentDTO>>> GetAssignmentsByClientId(int clientId)
        {
            try
            {
                // First verify the client exists
                var clientExists = await _context.Clients.AnyAsync(c => c.Id == clientId);
                if (!clientExists)
                {
                    return NotFound($"Client with ID {clientId} not found");
                }

                var assignments = await _context.FormAssignments
                    .Include(a => a.FormTemplate)
                    .Where(a => a.ClientId == clientId)
                    .Select(a => new FormAssignmentDTO
                    {
                        Id = a.Id,
                        FormTemplateId = a.FormTemplateId,
                        ClientId = a.ClientId,
                        FormTemplateName = a.FormTemplate.Name,
                        Description = a.FormTemplate.Description ?? "",
                        AssignedAt = a.AssignedAt,
                        Status = a.Status,
                        Notes = a.Notes,
                        LastSubmissionDate = a.Submissions
                            .OrderByDescending(s => s.SubmittedAt)
                            .Select(s => s.SubmittedAt)
                            .FirstOrDefault()
                    })
                    .OrderByDescending(a => a.AssignedAt)
                    .ToListAsync();

                return Ok(assignments);
            }            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving form assignments for client {clientId}: {ex.Message}");
            }
        }

        // GET: api/FormAssignments/assignment/{id}
        [HttpGet("assignment/{id}")]
        public async Task<ActionResult<FormAssignmentDTO>> GetAssignmentById(int id)
        {
            try
            {
                var assignment = await _context.FormAssignments
                    .Include(a => a.FormTemplate)
                    .Include(a => a.Submissions)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (assignment == null)
                {
                    return NotFound($"Assignment with ID {id} not found");
                }

                var dto = new FormAssignmentDTO
                {
                    Id = assignment.Id,
                    FormTemplateId = assignment.FormTemplateId,
                    ClientId = assignment.ClientId,
                    FormTemplateName = assignment.FormTemplate.Name,
                    Description = assignment.FormTemplate.Description ?? "",
                    AssignedAt = assignment.AssignedAt,
                    Status = assignment.Status,
                    Notes = assignment.Notes,
                    LastSubmissionDate = assignment.Submissions
                        .OrderByDescending(s => s.SubmittedAt)
                        .Select(s => s.SubmittedAt)
                        .FirstOrDefault()
                };

                return Ok(dto);
            }            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving form assignment {id}: {ex.Message}");
            }
        }

        // POST: api/FormAssignments
        [HttpPost("create")]
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
