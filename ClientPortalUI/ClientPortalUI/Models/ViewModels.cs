using System;
using System.Collections.Generic;

namespace ClientPortalUI.Models
{
    public class FormTemplateViewModel
    {
        public int AssignmentId { get; set; }  // Pass back the assignment identifier
        public int TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<FormFieldViewModel> Fields { get; set; } = new();
    }

    public class FormFieldViewModel
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string FieldTypeName { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string Options { get; set; } = string.Empty;
        // You can add properties like IsRequired, etc.
    }

    public class FieldTypeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // E.g., Text, Date, Dropdown, etc.
    }


    public class SubmissionViewModel
    {
        public int FormAssignmentId { get; set; }
        public string SubmittedByUserId { get; set; } = string.Empty;
        public string DataJson { get; set; } = string.Empty; // The JSON data from the form
    }

    public class SubmissionResponseViewModel
    {
        public int SubmissionId { get; set; }
        public string DataJson { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }

    public class ActivityHistoryViewModel
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DataSnapshot { get; set; } = string.Empty;
        public DateTime PerformedAt { get; set; }
    }

    public class ClientViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

}
