namespace ClientPortalAPI.Entities
{
    public class ActivityHistory
    {
        public int Id { get; set; }

        public int SubmissionId { get; set; }
        public Submission Submission { get; set; } = null!;

        public string PerformedByUserId { get; set; } = null!;
        public ApplicationUser PerformedBy { get; set; } = null!;

        public string ActionType { get; set; } = null!; // e.g., "Submit", "Edit"
        public string? Description { get; set; }
        public string? DataSnapshot { get; set; } // JSON
        public DateTime PerformedAt { get; set; }
    }

}
