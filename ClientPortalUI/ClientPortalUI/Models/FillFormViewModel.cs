using System;
using System.Collections.Generic;

namespace ClientPortalUI.Models
{
    public class FillFormViewModel
    {
        public int AssignmentId { get; set; }
        public string FormName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime? LastSubmissionDate { get; set; }
        public string? ExistingDataJson { get; set; }
        public List<FormFieldValueViewModel> Fields { get; set; } = new();
    }    public class FormFieldValueViewModel : FormFieldViewModel
    {
        // Only add new properties, inherit the rest from FormFieldViewModel
        public new string? Value { get; set; }
        public string? ValidationMessage { get; set; }
    }
}
