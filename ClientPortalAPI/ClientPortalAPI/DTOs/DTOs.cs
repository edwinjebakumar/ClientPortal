﻿using System;
using System.Collections.Generic;

namespace ClientPortalAPI.DTOs
{
    // DTO for Form Assignment (for listing the forms assigned to a client)
    public class FormAssignmentDTO
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int FormTemplateId { get; set; }
        public string FormTemplateName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }
        public DateTime? LastSubmissionDate { get; set; }
    }    // DTO for Form Template (detailed view including fields)
    public class FormTemplateDTO
    {
        public int TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? BaseTemplateId { get; set; }
        public List<FormFieldDTO> Fields { get; set; } = new();
    }

    // DTO for Form Field (part of a form template)
    public class FormFieldDTO
    {
        public int FieldId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string FieldTypeName { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string? Options { get; set; }
        public int FieldOrder { get; set; }
    }    // DTO for receiving a form submission from the UI
    public class SubmissionRequestDTO
    {
        public int FormAssignmentId { get; set; }
        public string SubmittedByUserId { get; set; } = string.Empty;
        // This property holds the submission data in JSON format.
        public string DataJson { get; set; } = string.Empty;
    }    // DTO for sending a submission response back to the client
    public class SubmissionResponseDTO
    {
        public int SubmissionId { get; set; }
        public string DataJson { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }

    // DTO for Activity History (audit trail of submissions/updates)
    public class ActivityHistoryDTO
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public string ActionType { get; set; } = string.Empty;  // e.g. "Submit", "Edit"
        public string? Description { get; set; }
        // Stores a JSON snapshot of the submission data for auditing
        public string DataSnapshot { get; set; } = string.Empty;
        public DateTime PerformedAt { get; set; }
    }    public class ClientDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AssignedFormsCount { get; set; }
    }

    public class FieldTypeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // E.g., Text, Date, Dropdown, etc.
    }
}
