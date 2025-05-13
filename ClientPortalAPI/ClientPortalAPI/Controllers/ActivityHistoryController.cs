using ClientPortalAPI.Data;
using ClientPortalAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityHistoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ActivityHistoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ActivityHistory/submission/5
        [HttpGet("submission/{submissionId}")]
        public async Task<ActionResult<IEnumerable<ActivityHistory>>> GetActivityHistoryBySubmission(int submissionId)
        {
            var history = await _context.ActivityHistories
                                        .Where(a => a.SubmissionId == submissionId)
                                        .OrderByDescending(a => a.PerformedAt)
                                        .ToListAsync();
            return history;
        }

        // Optionally, an endpoint to get a user’s activity history
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ActivityHistory>>> GetActivityHistoryByUser(string userId)
        {
            var history = await _context.ActivityHistories
                                        .Where(a => a.PerformedByUserId == userId)
                                        .OrderByDescending(a => a.PerformedAt)
                                        .ToListAsync();
            return history;
        }
    }

}
