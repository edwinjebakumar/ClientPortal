namespace ClientPortalUI.Models
{
    public class FormAssignmentViewModel
    {
        public int Id { get; set; }
        public string FormTemplateName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        // Add other properties as needed
    }
}
