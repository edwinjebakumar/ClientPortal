using ClientPortalUI.API;
using ClientPortalUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientPortalUI.Controllers
{
    [Authorize(Roles = "Client,Admin")]
    public class ClientController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<ClientController> _logger; public ClientController(IApiService apiService, ILogger<ClientController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        private int? GetCurrentUserClientId()
        {
            var clientIdClaim = User.FindFirst("ClientId");
            if (clientIdClaim != null && int.TryParse(clientIdClaim.Value, out int clientId))
            {
                return clientId;
            }
            return null;
        }

        private bool IsCurrentUserAdmin()
        {
            return User.IsInRole("Admin");
        }

        // New action for client-specific dashboard routing
        public async Task<IActionResult> Dashboard(int? clientId = null)
        {
            try
            {
                // If user is admin, they can view any client's dashboard
                if (IsCurrentUserAdmin())
                {
                    if (clientId.HasValue)
                    {
                        return await ClientDashboard(clientId.Value);
                    }
                    else
                    {
                        // Redirect admin to admin panel if no specific client requested
                        return RedirectToAction("Index", "Admin");
                    }
                }

                // For client users, get their assigned client ID
                var userClientId = GetCurrentUserClientId();
                if (!userClientId.HasValue)
                {
                    TempData["Error"] = "You are not assigned to any client.";
                    return RedirectToAction("Index", "Home");
                }

                // Client users can only view their own dashboard
                if (clientId.HasValue && clientId.Value != userClientId.Value)
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }

                return await ClientDashboard(userClientId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing dashboard");
                TempData["Error"] = "Error accessing dashboard. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ClientDashboard(int id)
        {
            try
            {
                // Authorization check: Admin can view any client, Client can only view their own
                if (!IsCurrentUserAdmin())
                {
                    var userClientId = GetCurrentUserClientId();
                    if (!userClientId.HasValue || userClientId.Value != id)
                    {
                        return RedirectToAction("AccessDenied", "Auth");
                    }
                }

                var client = await _apiService.GetClientAsync(id);
                if (client == null)
                {
                    TempData["Error"] = "Client not found.";
                    return IsCurrentUserAdmin() ?
                        RedirectToAction("ClientsManagement", "Admin") :
                        RedirectToAction("Index", "Home");
                }

                var assignments = await _apiService.GetFormAssignmentsAsync(id);

                // Set ViewBag data for the view
                ViewBag.ClientId = client.Id;
                ViewBag.ClientName = client.Name;
                ViewBag.IsAdmin = IsCurrentUserAdmin();

                if (IsCurrentUserAdmin())
                {
                    ViewBag.AvailableTemplates = await _apiService.GetFormTemplatesAsync();
                }

                return View(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading client dashboard for client {ClientId}", id);
                TempData["Error"] = "Error loading client dashboard. Please try again.";
                return IsCurrentUserAdmin() ?
                    RedirectToAction("ClientsManagement", "Admin") :
                    RedirectToAction("Index", "Home");
            }
        }
        public async Task<IActionResult> FillForm(int assignmentId)
        {
            try
            {
                var assignment = await _apiService.GetFormAssignmentAsync(assignmentId);
                if (assignment == null)
                {
                    TempData["Error"] = "Form assignment not found.";
                    return RedirectToAction("Dashboard");
                }

                // Authorization check: Client can only access their own assignments
                if (!IsCurrentUserAdmin())
                {
                    var userClientId = GetCurrentUserClientId();
                    if (!userClientId.HasValue || userClientId.Value != assignment.ClientId)
                    {
                        return RedirectToAction("AccessDenied", "Auth");
                    }
                }
                var formTemplate = await _apiService.GetFormTemplateAsync(assignment.FormTemplateId);
                if (formTemplate == null)
                {
                    TempData["Error"] = "Form template not found.";
                    return RedirectToAction("Dashboard", new { clientId = assignment.ClientId });
                }

                // Get the latest submission if it exists
                var latestSubmission = await _apiService.GetLatestSubmissionAsync(assignmentId);

                var viewModel = new FillFormViewModel
                {
                    AssignmentId = assignmentId,
                    FormName = formTemplate.Name,
                    Description = formTemplate.Description,
                    Status = assignment.Status,
                    LastSubmissionDate = assignment.LastSubmissionDate,
                    ExistingDataJson = latestSubmission?.DataJson,
                    Fields = formTemplate.Fields.Select(f => new FormFieldValueViewModel
                    {
                        Id = f.Id,
                        Label = f.Label,
                        FieldTypeName = f.FieldTypeName,
                        IsRequired = f.IsRequired,
                        Options = f.Options,
                        FieldOrder = f.FieldOrder,
                        Value = null // Will be populated from existingData in the view
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading form for assignment {AssignmentId}", assignmentId);
                TempData["Error"] = "Error loading form. Please try again.";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitForm(int assignmentId, [FromForm] Dictionary<string, string> formData)
        {
            try
            {
                // Get form template to validate required fields
                var assignment = await _apiService.GetFormAssignmentAsync(assignmentId);

                // Authorization check: Client can only submit their own assignments
                if (!IsCurrentUserAdmin())
                {
                    var userClientId = GetCurrentUserClientId();
                    if (!userClientId.HasValue || userClientId.Value != assignment.ClientId)
                    {
                        return RedirectToAction("AccessDenied", "Auth");
                    }
                }

                var template = await _apiService.GetFormTemplateAsync(assignment.FormTemplateId);

                // Validate required fields
                var requiredFields = template.Fields.Where(f => f.IsRequired).Select(f => f.Label);
                foreach (var field in requiredFields)
                {
                    if (!formData.ContainsKey(field) || string.IsNullOrWhiteSpace(formData[field]))
                    {
                        TempData["Error"] = $"Please fill in the required field: {field}";
                        return RedirectToAction("FillForm", new { assignmentId });
                    }
                }

                var submission = new SubmissionViewModel
                {
                    FormAssignmentId = assignmentId,
                    SubmittedByUserId = User.Identity?.Name ?? "anonymous", // Replace with actual user ID
                    DataJson = System.Text.Json.JsonSerializer.Serialize(formData)
                };

                var result = await _apiService.SubmitFormAsync(submission); if (result != null)
                {
                    TempData["Success"] = "Form submitted successfully!";
                    return RedirectToAction("Dashboard", new { clientId = assignment.ClientId });
                }

                TempData["Error"] = "Failed to submit form. Please try again.";
                return RedirectToAction("FillForm", new { assignmentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting form for assignment {AssignmentId}", assignmentId);
                TempData["Error"] = "Error submitting form. Please try again.";
                return RedirectToAction("FillForm", new { assignmentId });
            }
        }
        public async Task<IActionResult> ViewSubmission(int assignmentId)
        {
            try
            {
                var assignment = await _apiService.GetFormAssignmentAsync(assignmentId);

                // Authorization check: Client can only view their own submissions
                if (!IsCurrentUserAdmin())
                {
                    var userClientId = GetCurrentUserClientId();
                    if (!userClientId.HasValue || userClientId.Value != assignment.ClientId)
                    {
                        return RedirectToAction("AccessDenied", "Auth");
                    }
                }

                // Get the form template and submission data
                var formTemplate = await _apiService.GetFormTemplateAsync(assignmentId);
                // Get submission history
                var history = await _apiService.GetActivityHistoryBySubmissionAsync(assignmentId);

                if (formTemplate == null)
                {
                    TempData["Error"] = "Form not found.";
                    return RedirectToAction("Dashboard");
                }

                ViewBag.SubmissionHistory = history;
                return View(formTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading submission for assignment {AssignmentId}", assignmentId);
                TempData["Error"] = "Error loading submission. Please try again.";
                return RedirectToAction("Dashboard");
            }
        }
        public async Task<IActionResult> ViewSubmissions(int assignmentId)
        {
            try
            {
                var assignment = await _apiService.GetFormAssignmentAsync(assignmentId);

                // Authorization check: Client can only view their own submissions
                if (!IsCurrentUserAdmin())
                {
                    var userClientId = GetCurrentUserClientId();
                    if (!userClientId.HasValue || userClientId.Value != assignment.ClientId)
                    {
                        return RedirectToAction("AccessDenied", "Auth");
                    }
                }

                var submissions = await _apiService.GetSubmissionsByAssignmentIdAsync(assignmentId);
                if (submissions == null)
                {
                    return NotFound($"No submissions found for assignment {assignmentId}");
                }

                ViewBag.AssignmentId = assignmentId;
                return View(submissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving submissions for assignment {AssignmentId}", assignmentId);
                TempData["ErrorMessage"] = "Failed to retrieve submission history.";
                return RedirectToAction("Dashboard");
            }
        }
        public async Task<IActionResult> ViewSubmissionDetails(int id)
        {
            try
            {
                var submission = await _apiService.GetSubmissionDetailsAsync(id);

                // Authorization check: Client can only view their own submission details
                if (!IsCurrentUserAdmin())
                {
                    var assignment = await _apiService.GetFormAssignmentAsync(submission.FormAssignmentId);
                    var userClientId = GetCurrentUserClientId();
                    if (!userClientId.HasValue || userClientId.Value != assignment.ClientId)
                    {
                        return RedirectToAction("AccessDenied", "Auth");
                    }
                }

                var activities = await _apiService.GetActivityHistoryBySubmissionAsync(id);

                var viewModel = new ViewSubmissionDetailsViewModel
                {
                    Submission = submission,
                    ActivityHistory = activities
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving submission details for {SubmissionId}", id);
                TempData["ErrorMessage"] = "Failed to retrieve submission details.";
                return RedirectToAction("Dashboard");
            }
        }
    }
}
