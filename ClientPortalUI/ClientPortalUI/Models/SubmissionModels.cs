using System;

namespace ClientPortalUI.Models
{
    public class SubmissionRequestViewModel
    {
        public int FormAssignmentId { get; set; }
        public string SubmittedByUserId { get; set; } = string.Empty;
        public string DataJson { get; set; } = string.Empty;
    }    public class SubmissionResponseViewModel
    {
        public int SubmissionId { get; set; }
        public int FormAssignmentId { get; set; }
        public string DataJson { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }
}
