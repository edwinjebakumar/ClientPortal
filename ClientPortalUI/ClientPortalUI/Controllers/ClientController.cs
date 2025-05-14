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
                var formTemplate = await _apiService.GetFormTemplateAsync(assignmentId);
                if (formTemplate == null)
                {
                    TempData["Error"] = "Form not found.";
                    return RedirectToAction("ClientDashboard");
                }

                return View(formTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading form for assignment {AssignmentId}", assignmentId);
                TempData["Error"] = "Error loading form. Please try again.";
                return RedirectToAction("ClientDashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitForm(int assignmentId, [FromForm] Dictionary<string, string> formData)
        {
            try
            {
                var submission = new SubmissionViewModel
                {
                    FormAssignmentId = assignmentId,
                    DataJson = System.Text.Json.JsonSerializer.Serialize(formData)
                };

                var result = await _apiService.SubmitFormAsync(submission);
                if (result != null)
                {
                    TempData["Success"] = "Form submitted successfully!";
                    return RedirectToAction("ClientDashboard");
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
