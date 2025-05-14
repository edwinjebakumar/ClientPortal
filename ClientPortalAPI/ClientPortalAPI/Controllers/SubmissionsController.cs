using ClientPortalAPI.Data;
using ClientPortalAPI.DTOs;
using ClientPortalAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]    public class SubmissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SubmissionsController> _logger;

        public SubmissionsController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            ILogger<SubmissionsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: api/Submissions/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Submission>> GetSubmission(int id)
        {
            var submission = await _context.Submissions
                                           .Include(s => s.FormAssignment)
                                           .FirstOrDefaultAsync(s => s.Id == id);
            if (submission == null)
            {
                return NotFound();
            }
            return submission;
        }

        // GET: api/Submissions/latest/{assignmentId}
        [HttpGet("latest/{assignmentId}")]
        public async Task<ActionResult<Submission>> GetLatestSubmission(int assignmentId)
        {
            var submission = await _context.Submissions
                .Where(s => s.FormAssignmentId == assignmentId)
                .OrderByDescending(s => s.SubmittedAt)
                .FirstOrDefaultAsync();

            if (submission == null)
            {
                return NotFound();
            }

            return submission;
        }        // POST: api/Submissions
        [HttpPost]
        //[Authorize(Roles = "Client,Employee")]
        public async Task<ActionResult<SubmissionResponseDTO>> CreateSubmission(SubmissionRequestDTO submissionRequest)
        {
            try 
            {
                // Create new submission entity
                var submission = new Submission
                {
                    FormAssignmentId = submissionRequest.FormAssignmentId,
                    SubmittedByUserId = submissionRequest.SubmittedByUserId,
                    DataJson = submissionRequest.DataJson,
                    SubmittedAt = DateTime.UtcNow
                };

                _context.Submissions.Add(submission);
                await _context.SaveChangesAsync();                // Log activity with snapshot
                await LogActivity(submission.Id, submission.SubmittedByUserId, "Submit", submission.DataJson, "Initial submission");

                // Update form assignment status
                var formAssignment = await _context.FormAssignments.FindAsync(submission.FormAssignmentId);
                if (formAssignment != null)
                {
                    formAssignment.Status = "Submitted";
                    await _context.SaveChangesAsync();
                }

                // Return DTO
                var response = new SubmissionResponseDTO
                {
                    SubmissionId = submission.Id,
                    DataJson = submission.DataJson,
                    SubmittedAt = submission.SubmittedAt
                };

                return CreatedAtAction(nameof(GetSubmission), new { id = submission.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating submission for assignment {AssignmentId}", submissionRequest.FormAssignmentId);
                return StatusCode(500, "Error creating submission");
            }

        }

        private async Task LogActivity(int submissionId, string userId, string actionType, string dataSnapshot, string description)
        {
            var activity = new ActivityHistory
            {
                SubmissionId = submissionId,
                PerformedByUserId = userId,
                ActionType = actionType,
                Description = description,
                DataSnapshot = dataSnapshot,
                PerformedAt = DateTime.UtcNow
            };

            _context.ActivityHistories.Add(activity);
            await _context.SaveChangesAsync();
        }
    }

}
