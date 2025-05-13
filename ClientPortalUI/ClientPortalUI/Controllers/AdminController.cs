using Microsoft.AspNetCore.Mvc;
using ClientPortalUI.API;
using ClientPortalUI.Models;
using System.Threading.Tasks;

namespace ClientPortalUI.Controllers
{
    public class AdminController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IApiService apiService, ILogger<AdminController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CreateFormTemplate()
        {
            try
            {
                var fieldTypes = await _apiService.GetFieldTypesAsync();
                ViewBag.FieldTypes = fieldTypes;
                
                var model = new FormTemplateViewModel
                {
                    Fields = new List<FormFieldViewModel>()
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading form template creation page");
                TempData["Error"] = "Error loading field types. Please try again.";
                return RedirectToAction(nameof(AdminDashboard));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFormTemplate([FromForm] FormTemplateViewModel formTemplate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state when creating form template");
                    ViewBag.FieldTypes = await _apiService.GetFieldTypesAsync();
                    return View(formTemplate);
                }

                // Validate that there is at least one field
                if (formTemplate.Fields == null || !formTemplate.Fields.Any())
                {
                    _logger.LogWarning("Attempted to create template without fields");
                    ModelState.AddModelError("", "At least one field is required.");
                    ViewBag.FieldTypes = await _apiService.GetFieldTypesAsync();
                    return View(formTemplate);
                }

                // Validate dropdown fields have options
                foreach (var field in formTemplate.Fields.Where(f => f.FieldTypeName == "Dropdown"))
                {
                    if (string.IsNullOrWhiteSpace(field.Options))
                    {
                        ModelState.AddModelError("", $"Field '{field.Label}' is a dropdown but has no options defined.");
                        ViewBag.FieldTypes = await _apiService.GetFieldTypesAsync();
                        return View(formTemplate);
                    }
                }

                _logger.LogInformation("Creating form template: {TemplateName} with {FieldCount} fields", 
                    formTemplate.Name, formTemplate.Fields.Count);

                var success = await _apiService.CreateFormTemplateAsync(formTemplate);
                
                if (success)
                {
                    TempData["Success"] = "Form template created successfully!";
                    return RedirectToAction(nameof(AdminDashboard));
                }
                
                ModelState.AddModelError("", "Failed to create form template. Please try again.");
                ViewBag.FieldTypes = await _apiService.GetFieldTypesAsync();
                return View(formTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating form template: {TemplateName}", formTemplate.Name);
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                ViewBag.FieldTypes = await _apiService.GetFieldTypesAsync();
                return View(formTemplate);
            }
        }
    }
}
