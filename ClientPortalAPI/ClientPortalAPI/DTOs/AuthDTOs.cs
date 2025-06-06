using System.ComponentModel.DataAnnotations;

namespace ClientPortalAPI.DTOs
{
    public class LoginRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequestDTO
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;        public string Role { get; set; } = string.Empty;
        public int? ClientId { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class UpdateUserRequestDTO
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public int? ClientId { get; set; }

        public bool ChangePassword { get; set; } = false;

        [StringLength(100, MinimumLength = 6)]
        public string? NewPassword { get; set; }

        [Compare("NewPassword")]
        public string? ConfirmNewPassword { get; set; }
    }    public class AuthResponseDTO
    {
        public bool Succeeded { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public int? ClientId { get; set; }
        public string? ClientName { get; set; }
    }
}
