namespace ClientPortalAPI.Entities
{
    public class FormAssignment
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public int FormTemplateId { get; set; }
        public FormTemplate FormTemplate { get; set; } = null!;

        public DateTime AssignedAt { get; set; }

        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }

}
