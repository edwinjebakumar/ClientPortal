using System.ComponentModel.DataAnnotations;

namespace ClientPortalUI.Models
{
    public class FormTemplateAssignmentRequest
    {
        [Required]
        public int FormTemplateId { get; set; }
        
        [Required]
        public List<int> SelectedClientIds { get; set; } = new();
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
