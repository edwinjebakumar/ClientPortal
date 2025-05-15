using System;
using System.Collections.Generic;

namespace ClientPortalUI.Helpers
{
    public static class ViewHelpers
    {
        public static string GetFieldTypeIcon(string fieldType)
        {
            return fieldType?.ToLower() switch
            {
                "text" => "bi-text-left",
                "number" => "bi-123",
                "email" => "bi-envelope",
                "date" => "bi-calendar",
                "checkbox" => "bi-check-square",
                "dropdown" => "bi-list",
                "textarea" => "bi-textarea-t",
                _ => "bi-input-cursor-text"
            };
        }
    }
}
