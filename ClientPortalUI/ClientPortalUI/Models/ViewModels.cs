using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClientPortalUI.Models
{    
    public class FormTemplateViewModel
    {
        public int AssignmentId { get; set; }
        public int TemplateId { get; set; }

        [Required(ErrorMessage = "Template name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Template name must be between 3 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        public int? BaseTemplateId { get; set; }
        public bool IsBaseTemplate => BaseTemplateId is not null;

        public List<FormFieldViewModel> Fields { get; set; } = new();
    }

    public class FormFieldViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Field label is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Field label must be between 1 and 100 characters")]
        public string Label { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field type is required")]
        public string FieldTypeName { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        [StringLength(1000, ErrorMessage = "Options cannot exceed 1000 characters")]
        public string Options { get; set; } = string.Empty;

        public int FieldOrder { get; set; }
    }

    public class FieldTypeViewModel
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
    }

    public class SubmissionViewModel
    {
        public int FormAssignmentId { get; set; }
        public string SubmittedByUserId { get; set; } = string.Empty;
        public string DataJson { get; set; } = string.Empty;
    }    // Moved to SubmissionModels.cs

    public class ActivityHistoryViewModel
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public string PerformedByUserId { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DataSnapshot { get; set; } = string.Empty;
        public DateTime PerformedAt { get; set; }
    }

    public class ClientViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AssignedFormsCount { get; set; }
    }    // Moved to FillFormViewModel.cs

    public class ViewSubmissionDetailsViewModel
    {
        public SubmissionResponseViewModel Submission { get; set; } = new();
        public List<ActivityHistoryViewModel> ActivityHistory { get; set; } = new();
    }
}
