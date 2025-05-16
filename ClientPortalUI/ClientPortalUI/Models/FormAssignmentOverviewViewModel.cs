using System.Collections.Generic;

namespace ClientPortalUI.Models
{
    public class FormAssignmentOverviewViewModel
    {
        public List<FormTemplateViewModel> FormTemplates { get; set; } = new();
        public List<ClientViewModel> AvailableClients { get; set; } = new();
    }
}
