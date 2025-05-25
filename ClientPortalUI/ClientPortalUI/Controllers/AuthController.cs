using Microsoft.AspNetCore.Mvc;
using ClientPortalUI.Models;
using ClientPortalUI.API;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClientPortalUI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IApiService apiService, ILogger<AuthController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _apiService.LoginAsync(model);                if (result.Succeeded)
                {
                    // Create claims for the user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, result.Email),
                        new Claim("Token", result.Token)
                    };

                    // Add role claims
                    foreach (var role in result.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    // Add client claims if user has a client
                    if (result.ClientId.HasValue)
                    {
                        claims.Add(new Claim("ClientId", result.ClientId.Value.ToString()));
                        if (!string.IsNullOrEmpty(result.ClientName))
                        {
                            claims.Add(new Claim("ClientName", result.ClientName));
                        }
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // Redirect based on user role
                    if (result.Roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (result.Roles.Contains("Client") && result.ClientId.HasValue)
                    {
                        return RedirectToAction("Dashboard", "Client", new { clientId = result.ClientId.Value });
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register()
        {
            var model = new RegisterViewModel();
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            // Check username availability
            var isAvailable = await _apiService.CheckUsernameAvailabilityAsync(model.UserName);
            if (!isAvailable)
            {
                ModelState.AddModelError("UserName", "This username is already taken.");
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            var result = await _apiService.RegisterAsync(model);
            if (result.Succeeded)
            {
                _logger.LogInformation("Admin created a new user account.");
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction(nameof(ManageUsers));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            await PopulateDropdownsAsync(model);
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _apiService.GetUsersAsync();
            return View(users);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _apiService.GetUserAsync(id);
            if (user == null || string.IsNullOrEmpty(user.Id))
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Roles.FirstOrDefault() ?? "",
                ClientId = user.ClientId
            };

            await PopulateDropdownsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            var result = await _apiService.UpdateUserAsync(model);
            if (result.Succeeded)
            {
                _logger.LogInformation("Admin updated user {UserId}.", model.Id);
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(ManageUsers));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            await PopulateDropdownsAsync(model);
            return View(model);
        }

        private async Task PopulateDropdownsAsync(RegisterViewModel model)
        {
            var roles = await _apiService.GetRolesAsync();
            var clients = await _apiService.GetClientsAsync();

            model.AvailableRoles = roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.DisplayName,
                Selected = r.Name == model.Role
            }).ToList();

            model.AvailableClients = clients.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = c.Id == model.ClientId
            }).ToList();

            // Add "No Client" option
            model.AvailableClients.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "No Client",
                Selected = !model.ClientId.HasValue
            });
        }

        private async Task PopulateDropdownsAsync(EditUserViewModel model)
        {
            var roles = await _apiService.GetRolesAsync();
            var clients = await _apiService.GetClientsAsync();

            model.AvailableRoles = roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.DisplayName,
                Selected = r.Name == model.Role
            }).ToList();

            model.AvailableClients = clients.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = c.Id == model.ClientId
            }).ToList();

            // Add "No Client" option
            model.AvailableClients.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "No Client",
                Selected = !model.ClientId.HasValue
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CheckUsername(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { isAvailable = false });

            var isAvailable = await _apiService.CheckUsernameAvailabilityAsync(id);
            return Json(new { isAvailable });
        }
    }
}
