namespace ClientPortalAPI.Entities
{
    public class Submission
    {
        public int Id { get; set; }

        public int FormAssignmentId { get; set; }
        public FormAssignment FormAssignment { get; set; } = null!;

        public string SubmittedByUserId { get; set; } = null!;
        public ApplicationUser SubmittedBy { get; set; } = null!;

        public DateTime SubmittedAt { get; set; }
        public string DataJson { get; set; } = null!;

        public ICollection<SubmissionFile> Files { get; set; } = new List<SubmissionFile>();
        public ICollection<ActivityHistory> ActivityHistories { get; set; } = new List<ActivityHistory>();
    }

}
