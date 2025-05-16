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
        
        public async Task<IActionResult> AdminDashboard()
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

        [HttpGet]
        public async Task<IActionResult> CloneTemplate(int id)
        {
            try
            {
                var template = await _apiService.GetFormTemplateAsync(id);
                if (template == null)
                {
                    TempData["Error"] = "Template not found.";
                    return RedirectToAction(nameof(AdminDashboard));
                }                // Prepare a new template based on the existing one
                var newTemplate = new FormTemplateViewModel
                {
                    Name = $"Copy of {template.Name}",
                    Description = template.Description,
                    BaseTemplateId = template.IsBaseTemplate ? template.TemplateId : template.BaseTemplateId,
                    Fields = template.Fields.Select(f => new FormFieldViewModel
                    {
                        Label = f.Label,
                        FieldTypeName = f.FieldTypeName,
                        IsRequired = f.IsRequired,
                        Options = f.Options,
                        FieldOrder = f.FieldOrder
                    }).ToList()
                };

                // Get field types for the view
                ViewBag.FieldTypes = await _apiService.GetFieldTypesAsync();

                return View("CreateFormTemplate", newTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning template. Id: {TemplateId}", id);
                TempData["Error"] = "Error cloning template. Please try again.";
                return RedirectToAction(nameof(AdminDashboard));
            }
        }        [HttpGet]
        public async Task<IActionResult> ClientsManagement()
        {
            try
            {
                // Get all clients
                var clients = await _apiService.GetClientsAsync();

                // Get available form templates for assignment
                var templates = await _apiService.GetFormTemplatesAsync();
                ViewBag.AvailableTemplates = templates;

                // Set page title and description
                ViewBag.Title = "Client Management";
                ViewBag.Description = "Manage clients and their form assignments";

                return View(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clients and templates");
                TempData["Error"] = "Error loading client management page. Please try again.";
                return View(new List<ClientViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient([FromForm] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    TempData["Error"] = "Client name is required.";
                    return RedirectToAction(nameof(ClientsManagement));
                }

                var client = new ClientViewModel { Name = name };
                var success = await _apiService.CreateClientAsync(client);

                if (success)
                {
                    TempData["Success"] = "Client created successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to create client. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client: {ClientName}", name);
                TempData["Error"] = "Error creating client. Please try again.";
            }

            return RedirectToAction(nameof(ClientsManagement));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateClient([FromForm] int id, [FromForm] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    TempData["Error"] = "Client name is required.";
                    return RedirectToAction(nameof(ClientsManagement));
                }

                var client = new ClientViewModel { Id = id, Name = name };
                var success = await _apiService.UpdateClientAsync(client);

                if (success)
                {
                    TempData["Success"] = "Client updated successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to update client. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client: {ClientId}", id);
                TempData["Error"] = "Error updating client. Please try again.";
            }

            return RedirectToAction(nameof(ClientsManagement));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClient([FromForm] int id)
        {
            try
            {
                var success = await _apiService.DeleteClientAsync(id);
                if (success)
                {
                    TempData["Success"] = "Client deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to delete client. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client: {ClientId}", id);
                TempData["Error"] = "Error deleting client. Please try again.";
            }

            return RedirectToAction(nameof(ClientsManagement));
        }

        [HttpPost]
        public async Task<IActionResult> AssignForm([FromForm] int clientId, [FromForm] int formTemplateId, [FromForm] string notes)
        {
            try
            {
                // Call your API service to assign the form template to the client
                // You'll need to add this method to your IApiService and ApiService
                var success = await _apiService.AssignFormTemplateAsync(clientId, formTemplateId, notes);

                if (success)
                {
                    TempData["Success"] = "Form assigned successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to assign form. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning form template {TemplateId} to client {ClientId}", 
                    formTemplateId, clientId);
                TempData["Error"] = "Error assigning form. Please try again.";
            }

            return RedirectToAction("ClientDashboard", "Client", new { id = clientId });
        }

        // GET: Admin/AssignTemplate/{templateId}
        public async Task<IActionResult> AssignTemplate(int templateId)
        {
            try
            {
                var template = await _apiService.GetFormTemplateAsync(templateId);
                if (template == null)
                {
                    return NotFound();
                }

                var clients = await _apiService.GetClientsAsync();
                
                var viewModel = new TemplateAssignmentViewModel
                {
                    FormTemplateId = templateId,
                    TemplateName = template.Name,
                    AvailableClients = clients
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading template assignment page for template {TemplateId}", templateId);
                TempData["Error"] = "Error loading template assignment page. Please try again.";
                return RedirectToAction(nameof(FormTemplates));
            }
        }

        // POST: Admin/AssignTemplate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTemplate(TemplateAssignmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var clients = await _apiService.GetClientsAsync();
                model.AvailableClients = clients;
                return View(model);
            }

            try
            {
                var successCount = 0;
                foreach (var clientId in model.SelectedClientIds)
                {
                    var result = await _apiService.AssignFormTemplateAsync(clientId, model.FormTemplateId, model.Notes);
                    if (result)
                    {
                        successCount++;
                    }
                }

                if (successCount > 0)
                {
                    TempData["Success"] = $"Template successfully assigned to {successCount} client(s).";
                }
                else
                {
                    TempData["Error"] = "Failed to assign template to any selected clients.";
                }

                return RedirectToAction(nameof(FormTemplates));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning template {TemplateId} to clients", model.FormTemplateId);
                TempData["Error"] = "Error assigning template. Please try again.";
                var clients = await _apiService.GetClientsAsync();
                model.AvailableClients = clients;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> FormAssignment()
        {
            try
            {
                var templates = await _apiService.GetFormTemplatesAsync();
                var clients = await _apiService.GetClientsAsync();

                var viewModel = new FormAssignmentOverviewViewModel
                {
                    FormTemplates = templates,
                    AvailableClients = clients
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading form assignment page");
                TempData["Error"] = "Error loading form assignment page. Please try again.";
                return RedirectToAction(nameof(AdminDashboard));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTemplates(FormTemplateAssignmentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please check your input and try again.";
                    return RedirectToAction(nameof(FormAssignment));
                }

                var successCount = 0;
                foreach (var clientId in request.SelectedClientIds)
                {
                    var result = await _apiService.AssignFormTemplateAsync(clientId, request.FormTemplateId, request.Notes);
                    if (result)
                    {
                        successCount++;
                    }
                }

                if (successCount > 0)
                {
                    TempData["Success"] = $"Successfully assigned template to {successCount} client(s).";
                }
                else
                {
                    TempData["Error"] = "Failed to assign template to any selected clients.";
                }

                return RedirectToAction(nameof(FormAssignment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning template {TemplateId} to clients", request.FormTemplateId);
                TempData["Error"] = "Error assigning template to clients. Please try again.";
                return RedirectToAction(nameof(FormAssignment));
            }
        }
    }
}
