using ClientPortalAPI.Data;
using ClientPortalAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubmissionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        // POST: api/Submissions
        [HttpPost]
        [Authorize(Roles = "Client,Employee")]
        public async Task<ActionResult<Submission>> CreateSubmission(Submission submission)
        {
            // For new submission (Option 2: update existing record if exists)
            // Here we assume that a client can only have one active submission per assignment.
            // You may want to check if a submission already exists.
            var existingSubmission = await _context.Submissions
                                      .FirstOrDefaultAsync(s => s.FormAssignmentId == submission.FormAssignmentId && s.SubmittedByUserId == submission.SubmittedByUserId);

            string dataJson = submission.DataJson; // assuming client sends valid JSON data

            if (existingSubmission == null)
            {
                // Insert new submission
                submission.SubmittedAt = DateTime.UtcNow;
                _context.Submissions.Add(submission);
                await _context.SaveChangesAsync();

                // Log activity with snapshot
                await LogActivity(submission.Id, submission.SubmittedByUserId, "Submit", dataJson, "Initial submission");

                return CreatedAtAction(nameof(GetSubmission), new { id = submission.Id }, submission);
            }
            else
            {
                // Update existing submission
                var oldData = existingSubmission.DataJson;
                existingSubmission.DataJson = dataJson;
                existingSubmission.SubmittedAt = DateTime.UtcNow;
                _context.Entry(existingSubmission).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // Log activity capturing before and after (for demo, we store new data snapshot)
                await LogActivity(existingSubmission.Id, submission.SubmittedByUserId, "Edit", dataJson, "Updated submission");
                return Ok(existingSubmission);
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
