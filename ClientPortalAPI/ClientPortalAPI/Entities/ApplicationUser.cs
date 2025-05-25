using Microsoft.AspNetCore.Identity;

namespace ClientPortalAPI.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public int? ClientId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
