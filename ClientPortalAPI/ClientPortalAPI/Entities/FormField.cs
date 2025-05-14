namespace ClientPortalAPI.Entities
{
    public class FormField
    {
        public int Id { get; set; }
        public int FormTemplateId { get; set; }
        public FormTemplate FormTemplate { get; set; } = null!;

        public string Label { get; set; } = null!;
        public int FieldTypeId { get; set; }
        public FieldType FieldType { get; set; } = null!;

        public bool IsRequired { get; set; }
        public string? OptionsJson { get; set; } // for dropdowns/radio buttons
        public int FieldOrder { get; set; } // determines the display order of fields
    }

}
