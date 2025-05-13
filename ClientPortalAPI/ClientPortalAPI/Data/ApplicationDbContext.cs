using ClientPortalAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClientPortalAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<FieldType> FieldTypes { get; set; }
        public DbSet<BaseTemplate> BaseTemplates { get; set; }
        public DbSet<FormTemplate> FormTemplates { get; set; }
        public DbSet<FormField> FormFields { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<FormAssignment> FormAssignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<SubmissionFile> SubmissionFiles { get; set; }
        public DbSet<ActivityHistory> ActivityHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FormAssignment>()
                .HasIndex(fa => new { fa.ClientId, fa.FormTemplateId })
                .IsUnique(); // prevent assigning same form to same client multiple times
        }
    }

}
