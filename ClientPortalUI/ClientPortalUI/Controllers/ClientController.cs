using ClientPortalUI.API;
using ClientPortalUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClientPortalUI.Controllers
{
    public class ClientController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IApiService apiService, ILogger<ClientController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> ClientDashboard(int id)
        {
            try
            {
                var client = await _apiService.GetClientAsync(id);
                if (client == null)
                {
                    TempData["Error"] = "Client not found.";
                    return RedirectToAction("ClientsManagement", "Admin");
                }

                var assignments = await _apiService.GetFormAssignmentsAsync(id);
                
                // Set ViewBag data for the view
                ViewBag.ClientId = client.Id;
                ViewBag.ClientName = client.Name;
                ViewBag.AvailableTemplates = await _apiService.GetFormTemplatesAsync();

                return View(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading client dashboard for client {ClientId}", id);
                TempData["Error"] = "Error loading client dashboard. Please try again.";
                return RedirectToAction("ClientsManagement", "Admin");
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
                    return RedirectToAction("ClientDashboard", assignment?.ClientId);
                }

                var formTemplate = await _apiService.GetFormTemplateAsync(assignment.FormTemplateId);
                if (formTemplate == null)
                {
                    TempData["Error"] = "Form template not found.";
                    return RedirectToAction("ClientDashboard", assignment?.ClientId);
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
                return RedirectToAction("ClientDashboard");
            }
        }        [HttpPost]
        public async Task<IActionResult> SubmitForm(int assignmentId, [FromForm] Dictionary<string, string> formData)
        {
            try
            {
                // Get form template to validate required fields
                var assignment = await _apiService.GetFormAssignmentAsync(assignmentId);
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

                var result = await _apiService.SubmitFormAsync(submission);
                if (result != null)
                {
                    TempData["Success"] = "Form submitted successfully!";
                    return RedirectToAction("ClientDashboard", new { id = assignment.ClientId });
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
                // Get the form template and submission data
                var formTemplate = await _apiService.GetFormTemplateAsync(assignmentId);
                // Get submission history
                var history = await _apiService.GetActivityHistoryBySubmissionAsync(assignmentId);

                if (formTemplate == null)
                {
                    TempData["Error"] = "Form not found.";
                    return RedirectToAction("ClientDashboard");
                }

                ViewBag.SubmissionHistory = history;
                return View(formTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading submission for assignment {AssignmentId}", assignmentId);
                TempData["Error"] = "Error loading submission. Please try again.";
                return RedirectToAction("ClientDashboard");
            }
        }
    }
}
