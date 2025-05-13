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
        }        public async Task<IActionResult> AdminDashboard()
        {
            try
            {
                var templates = await _apiService.GetFormTemplatesAsync();
                return View(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving form templates for dashboard");
                TempData["Error"] = "Error loading form templates. Please try again.";
                return View(new List<FormTemplateViewModel>());
            }
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

        public async Task<IActionResult> FormTemplates()
        {
            try
            {
                var templates = await _apiService.GetFormTemplatesAsync();
                return View(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving form templates");
                TempData["Error"] = "Error loading form templates. Please try again.";
                return RedirectToAction(nameof(AdminDashboard));
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditTemplate(int id)
        {
            try
            {
                var template = await _apiService.GetFormTemplateAsync(id);
                if (template == null)
                {
                    TempData["Error"] = "Template not found.";
                    return RedirectToAction(nameof(FormTemplates));
                }

                if (template.IsBaseTemplate)
                {
                    TempData["Error"] = "Base templates cannot be edited.";
                    return RedirectToAction(nameof(FormTemplates));
                }

                ViewBag.FieldTypes = await _apiService.GetFieldTypesAsync();
                return View(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving template for editing. Id: {TemplateId}", id);
                TempData["Error"] = "Error loading template. Please try again.";
                return RedirectToAction(nameof(FormTemplates));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditTemplate([FromForm] FormTemplateViewModel template)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.FieldTypes = await _apiService.GetFieldTypesAsync();
                    return View(template);
                }

                if (template.IsBaseTemplate)
                {
                    TempData["Error"] = "Base templates cannot be edited.";
                    return RedirectToAction(nameof(FormTemplates));
                }

                // Validate that there is at least one field
                if (template.Fields == null || !template.Fields.Any())
                {
                    ModelState.AddModelError("", "At least one field is required.");
                    ViewBag.FieldTypes = await _apiService.GetFieldTypesAsync();
                    return View(template);
                }

                // Validate dropdown fields have options
                foreach (var field in template.Fields.Where(f => f.FieldTypeName == "Dropdown"))
                {
                    if (string.IsNullOrWhiteSpace(field.Options))
                    {
                        ModelState.AddModelError("", $"Field '{field.Label}' is a dropdown but has no options defined.");
                        ViewBag.FieldTypes = await _apiService.GetFieldTypesAsync();
                        return View(template);
                    }
                }

                _logger.LogInformation("Updating form template: {TemplateName} with {FieldCount} fields", 
                    template.Name, template.Fields.Count);

                var success = await _apiService.UpdateFormTemplateAsync(template);
                
                if (success)
                {
                    TempData["Success"] = "Form template updated successfully!";
                    return RedirectToAction(nameof(FormTemplates));
                }
                
                ModelState.AddModelError("", "Failed to update form template. Please try again.");
                ViewBag.FieldTypes = await _apiService.GetFieldTypesAsync();
                return View(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating form template: {TemplateName}", template.Name);
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                ViewBag.FieldTypes = await _apiService.GetFieldTypesAsync();
                return View(template);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewTemplate(int id)
        {
            try
            {
                var template = await _apiService.GetFormTemplateAsync(id);
                if (template == null)
                {
                    TempData["Error"] = "Template not found.";
                    return RedirectToAction(nameof(FormTemplates));
                }

                return View(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving template for viewing. Id: {TemplateId}", id);
                TempData["Error"] = "Error loading template. Please try again.";
                return RedirectToAction(nameof(FormTemplates));
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            try
            {
                var template = await _apiService.GetFormTemplateAsync(id);
                if (template == null)
                {
                    TempData["Error"] = "Template not found.";
                    return RedirectToAction(nameof(FormTemplates));
                }

                if (template.IsBaseTemplate)
                {
                    TempData["Error"] = "Base templates cannot be deleted.";
                    return RedirectToAction(nameof(FormTemplates));
                }

                var success = await _apiService.DeleteFormTemplateAsync(id);
                if (success)
                {
                    TempData["Success"] = "Template deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to delete template. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting template. Id: {TemplateId}", id);
                TempData["Error"] = "Error deleting template. Please try again.";
            }

            return RedirectToAction(nameof(FormTemplates));
        }

        [HttpGet]
        public async Task<IActionResult> GetFieldTypes()
        {
            try
            {
                var fieldTypes = await _apiService.GetFieldTypesAsync();
                return Json(fieldTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving field types");
                return StatusCode(500, "Error retrieving field types");
            }
        }
    }
}
