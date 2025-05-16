using System.ComponentModel.DataAnnotations;

namespace ClientPortalUI.Models
{
    public class TemplateAssignmentViewModel
    {
        [Required]
        public int FormTemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        
        [Required]
        public List<int> SelectedClientIds { get; set; } = new();
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        public List<ClientViewModel> AvailableClients { get; set; } = new();
    }
}
