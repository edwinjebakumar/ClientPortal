namespace ClientPortalAPI.Entities
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<FormAssignment> Assignments { get; set; } = new List<FormAssignment>();
    }

}
