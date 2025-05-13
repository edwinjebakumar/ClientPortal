using Microsoft.AspNetCore.Http;

namespace ClientPortalAPI.Entities
{
    public class FormTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? BaseTemplateId { get; set; }
        public BaseTemplate? BaseTemplate { get; set; }

        public ICollection<FormField> Fields { get; set; } = new List<FormField>();
        public ICollection<FormAssignment> Assignments { get; set; } = new List<FormAssignment>();
    }

}
