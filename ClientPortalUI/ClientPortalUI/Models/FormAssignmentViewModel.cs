namespace ClientPortalUI.Models
{
    public class FormAssignmentViewModel
    {
        public int Id { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public string Status { get; set; } = "Pending";
        public bool HasSubmission { get; set; }
        public int ClientId { get; set; }
        public int TemplateId { get; set; }
    }
}
