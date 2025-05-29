using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ClientPortalAPI.DTOs;
using ClientPortalAPI.Entities;
using ClientPortalAPI.Services;
using ClientPortalAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ClientPortalAPI.Controllers
{
    public class ClientResult
    {
        public int Id { get; set; }        public string Name { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly AuthDbContext _authDbContext;
        private readonly ApplicationDbContext _applicationDbContext;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            AuthDbContext authDbContext,
            ApplicationDbContext applicationDbContext)        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
            _authDbContext = authDbContext;
            _applicationDbContext = applicationDbContext;
        }

        [HttpGet("check-admin")]
        public async Task<IActionResult> CheckAdminUser()
        {
            var adminEmail = _configuration["AdminUser:Email"];
            var user = await _userManager.FindByEmailAsync(adminEmail);
            
            if (user == null)
            {
                return NotFound("Admin user not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new { 
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles
            });
        }

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdminUser()
        {
            try
            {
                var adminEmail = _configuration["AdminUser:Email"];
                var adminPassword = _configuration["AdminUser:Password"];

                if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
                {
                    return BadRequest("Admin credentials not configured in appsettings.json");
                }

                _logger.LogInformation("Creating admin user with email: {Email}", adminEmail);

                var existingUser = await _userManager.FindByEmailAsync(adminEmail);
                if (existingUser != null)
                {
                    _logger.LogInformation("Deleting existing admin user");
                    var deleteResult = await _userManager.DeleteAsync(existingUser);
                    if (!deleteResult.Succeeded)
                    {
                        return BadRequest(new { Message = "Failed to delete existing admin user", Errors = deleteResult.Errors });
                    }                }

                // Create the Admin role if it doesn't exist
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    _logger.LogInformation("Creating Admin role");
                    var createRoleResult = await _roleManager.CreateAsync(new IdentityRole("Admin"));                    if (!createRoleResult.Succeeded)
                    {
                        return BadRequest(new { Message = "Failed to create Admin role", Errors = createRoleResult.Errors });
                    }
                }

                // Create new admin user with consistent username
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,  // Always use email as username
                    Email = adminEmail,
                    EmailConfirmed = true,
                    NormalizedUserName = adminEmail.ToUpper(),  // Pre-normalize the username
                    NormalizedEmail = adminEmail.ToUpper()  // Pre-normalize the email
                };

                _logger.LogInformation("Creating new admin user");
                var createResult = await _userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    return BadRequest(new { Message = "Failed to create admin user", Errors = createResult.Errors });
                }

                // Add to Admin role
                _logger.LogInformation("Adding user to Admin role");
                var roleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
                if (!roleResult.Succeeded)
                {
                    return BadRequest(new { Message = "Failed to add user to Admin role", Errors = roleResult.Errors });
                }

                // Verify the user was created correctly
                var verifyUser = await _userManager.FindByEmailAsync(adminEmail);
                var roles = await _userManager.GetRolesAsync(verifyUser);

                return Ok(new { 
                    Message = "Admin user created successfully",
                    Email = verifyUser.Email,
                    EmailConfirmed = verifyUser.EmailConfirmed,
                    Roles = roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin user");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginRequestDTO model)
        {            try
            {                _logger.LogInformation("Login attempt for user: {Email}", model.Email);

                // Find user by email
                var user = await _authDbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null)
                {
                    _logger.LogWarning("User not found: {Email}", model.Email);
                    return NotFound(new AuthResponseDTO 
                    { 
                        Succeeded = false,
                        Errors = new List<string> { "User not found." }
                    });
                }

                _logger.LogInformation("Found user: {Email}, EmailConfirmed: {EmailConfirmed}", 
                    user.Email, user.EmailConfirmed);                // Check user properties before password validation
                _logger.LogInformation("User details: UserName={UserName}, NormalizedUserName={NormalizedUserName}, Email={Email}, NormalizedEmail={NormalizedEmail}, HasPasswordHash={HasPasswordHash}",
                    user.UserName,
                    user.NormalizedUserName,
                    user.Email,
                    user.NormalizedEmail,
                    !string.IsNullOrEmpty(user.PasswordHash));

                // First verify with UserManager to get detailed password validation errors
                var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!isPasswordValid)
                {
                    _logger.LogWarning("Password validation failed for user {Email}", model.Email);
                    return BadRequest(new AuthResponseDTO 
                    { 
                        Succeeded = false,
                        Errors = new List<string> { "Invalid password." }
                    });
                }

                // Then use SignInManager for full sign-in validation
                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
                if (!signInResult.Succeeded)
                {
                    string errorMessage = signInResult.IsLockedOut ? "Account is locked." :
                                        signInResult.IsNotAllowed ? "Sign in not allowed." :
                                        "Sign in failed.";

                    _logger.LogWarning("Login failed for {Email}: {Error}", model.Email, errorMessage);
                    return BadRequest(new AuthResponseDTO 
                    { 
                        Succeeded = false,
                        Errors = new List<string> { errorMessage }
                    });
                }                var roles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation("User roles for {Email}: {Roles}", 
                    model.Email, string.Join(", ", roles));                var token = _tokenService.GenerateJwtToken(user, roles);

                // Get client name from ApplicationDbContext if ClientId exists
                string? clientName = null;
                if (user.ClientId.HasValue)
                {
                    var client = await _applicationDbContext.Clients
                        .Where(c => c.Id == user.ClientId.Value)
                        .FirstOrDefaultAsync();
                    clientName = client?.Name;
                }

                // Include client information in response
                var response = new AuthResponseDTO
                {
                    Succeeded = true,
                    Token = token,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList(),
                    ClientId = user.ClientId,
                    ClientName = clientName
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", model.Email);
                return StatusCode(500, new AuthResponseDTO 
                { 
                    Succeeded = false,
                    Errors = new List<string> { "An error occurred during login." }
                });
            }
        }

        [HttpPost("register")]        public async Task<ActionResult<AuthResponseDTO>> Register(RegisterRequestDTO model)
        {
            // Validate username availability
            var existingUser = await _userManager.FindByNameAsync(model.UserName);
            if (existingUser != null)
            {
                return BadRequest(new AuthResponseDTO
                {
                    Succeeded = false,
                    Errors = new List<string> { "Username is already taken." }
                });
            }            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                ClientId = model.ClientId
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new AuthResponseDTO
                {
                    Succeeded = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            // Assign role (new approach)
            if (!string.IsNullOrEmpty(model.Role))
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Role));
                }
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            // Legacy support: Assign roles from Roles list
            foreach (var role in model.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
                await _userManager.AddToRoleAsync(user, role);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateJwtToken(user, roles);

            return Ok(new AuthResponseDTO
            {
                Succeeded = true,
                Token = token,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList()
            });
        }

        [HttpPost("debug-verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] LoginRequestDTO model)
        {
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var user = new ApplicationUser(); // dummy user for hash verification

            // First test against the specific hash from the database
            var storedHash = "AQAAAAIAAYagAAAAENScE3aSjWmWHoqzXHuwr1kIviKg9f3PNrvWoR+qPwvZekwMQ1t8V0bia3H1aBIVcw==";
            var verificationResult = passwordHasher.VerifyHashedPassword(
                user, 
                storedHash,
                model.Password);

            // Also verify against the actual user if they exist
            var dbUser = await _userManager.FindByEmailAsync(model.Email);
            var dbVerificationResult = dbUser != null ? 
                passwordHasher.VerifyHashedPassword(dbUser, dbUser.PasswordHash ?? string.Empty, model.Password) 
                : PasswordVerificationResult.Failed;

            return Ok(new
            {
                Email = model.Email,
                TestedPassword = model.Password,
                StoredHashVerificationResult = verificationResult.ToString(),
                DbUserVerificationResult = dbVerificationResult.ToString(),
                DbUserHash = dbUser?.PasswordHash
            });
        }

        [HttpPost("reset-admin")]
        public async Task<IActionResult> ResetAdminUser()
        {
            try
            {
                var adminEmail = _configuration["AdminUser:Email"];
                var adminPassword = _configuration["AdminUser:Password"];

                _logger.LogInformation("Starting admin user reset process");                // Delete existing user if exists
                var existingUser = await _userManager.FindByEmailAsync(adminEmail);
                if (existingUser != null)
                {
                    // Remove from roles first
                    var userRoles = await _userManager.GetRolesAsync(existingUser);
                    foreach (var role in userRoles)
                    {
                        await _userManager.RemoveFromRoleAsync(existingUser, role);
                    }
                    
                    // Then delete the user
                    var deleteResult = await _userManager.DeleteAsync(existingUser);
                    if (!deleteResult.Succeeded)
                    {
                        _logger.LogError("Failed to delete existing admin user: {Errors}", 
                            string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
                        return BadRequest(new { Message = "Failed to delete existing admin user", Errors = deleteResult.Errors });
                    }
                    _logger.LogInformation("Deleted existing admin user");
                }

                // Create new admin user with password
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                _logger.LogInformation("Creating admin user with email: {Email} and password: {Password}", adminEmail, adminPassword);
                var createResult = await _userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    _logger.LogError("Failed to create admin user: {Errors}", 
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    return BadRequest(new { Errors = createResult.Errors });
                }

                // Ensure Admin role exists
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    _logger.LogInformation("Created Admin role");
                }

                // Add to Admin role
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                _logger.LogInformation("Successfully reset admin user");

                // Try to verify the password immediately
                var passwordValid = await _userManager.CheckPasswordAsync(adminUser, adminPassword);
                
                return Ok(new { 
                    Message = "Admin user reset successfully",
                    Email = adminUser.Email,
                    PasswordValid = passwordValid,
                    Roles = await _userManager.GetRolesAsync(adminUser)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting admin user");
                return StatusCode(500, new { Message = "Error resetting admin user", Error = ex.Message });
            }
        }

        [HttpGet("check-username/{username}")]
        public async Task<IActionResult> CheckUsernameAvailability(string username)
        {
            // Normalize the username before checking
            var normalizedUsername = _userManager.NormalizeName(username);
            var user = await _userManager.FindByNameAsync(normalizedUsername);
            
            return Ok(new { isAvailable = user == null });
        }        // User Management Endpoints
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userViewModels = new List<object>();
                
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    
                    // Get client name from ApplicationDbContext if ClientId exists
                    string? clientName = null;
                    if (user.ClientId.HasValue)
                    {
                        var client = await _authDbContext.Database.SqlQueryRaw<ClientResult>(
                            "SELECT Id, Name FROM Clients WHERE Id = {0}", user.ClientId.Value)
                            .FirstOrDefaultAsync();
                        clientName = client?.Name;
                    }
                    
                    userViewModels.Add(new
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Roles = roles,
                        ClientId = user.ClientId,
                        ClientName = clientName,
                        CreatedDate = user.CreatedDate,
                        IsActive = true // Assuming all users are active for now
                    });
                }

                return Ok(userViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { Message = "Error retrieving users" });
            }
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                
                // Get client name from ApplicationDbContext if ClientId exists
                string? clientName = null;
                if (user.ClientId.HasValue)
                {
                    var client = await _authDbContext.Database.SqlQueryRaw<ClientResult>(
                        "SELECT Id, Name FROM Clients WHERE Id = {0}", user.ClientId.Value)
                        .FirstOrDefaultAsync();
                    clientName = client?.Name;
                }
                
                var userViewModel = new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles,
                    ClientId = user.ClientId,
                    ClientName = clientName,
                    CreatedDate = user.CreatedDate,
                    IsActive = true
                };

                return Ok(userViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", userId);
                return StatusCode(500, new { Message = "Error retrieving user" });
            }
        }

        [HttpPut("users/{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequestDTO request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                // Update basic properties
                user.UserName = request.UserName;
                user.Email = request.Email;
                user.ClientId = request.ClientId;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return BadRequest(new AuthResponseDTO
                    {
                        Succeeded = false,
                        Errors = updateResult.Errors.Select(e => e.Description).ToList()
                    });
                }

                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }
                
                if (!string.IsNullOrEmpty(request.Role))
                {
                    await _userManager.AddToRoleAsync(user, request.Role);
                }

                // Update password if provided
                if (request.ChangePassword && !string.IsNullOrEmpty(request.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
                    
                    if (!passwordResult.Succeeded)
                    {
                        return BadRequest(new AuthResponseDTO
                        {
                            Succeeded = false,
                            Errors = passwordResult.Errors.Select(e => e.Description).ToList()
                        });
                    }
                }

                return Ok(new AuthResponseDTO { Succeeded = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return StatusCode(500, new { Message = "Error updating user" });
            }
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _roleManager.Roles.ToListAsync();
                var roleViewModels = roles.Select(r => new
                {
                    Name = r.Name,
                    DisplayName = r.Name // For now, use the same as Name
                }).ToList();

                return Ok(roleViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return StatusCode(500, new { Message = "Error retrieving roles" });
            }
        }
    }
}
