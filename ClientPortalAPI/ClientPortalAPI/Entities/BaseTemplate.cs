namespace ClientPortalAPI.Entities
{
    public class BaseTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ICollection<FormTemplate> Templates { get; set; } = new List<FormTemplate>();
    }

}
