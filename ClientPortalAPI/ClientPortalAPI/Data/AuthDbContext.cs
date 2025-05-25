using ClientPortalAPI.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClientPortalAPI.Data
{
    public class AuthDbContext: IdentityDbContext<ApplicationUser>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the relationship between ApplicationUser and Client
            // Note: Client is managed by ApplicationDbContext, so we just configure the FK here
            builder.Entity<ApplicationUser>()
                .Property(u => u.ClientId)
                .IsRequired(false);
        }
    }
}
