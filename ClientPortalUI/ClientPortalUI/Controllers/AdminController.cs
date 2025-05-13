using Microsoft.AspNetCore.Mvc;
using ClientPortalUI.API;
using ClientPortalUI.Models;

namespace ClientPortalUI.Controllers
{
    public class AdminController : Controller
    {
        private readonly IApiService _apiService;

        public AdminController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> CreateFormTemplate()
        {
            var fieldTypes = await _apiService.GetFieldTypesAsync(); // Get field types from API
            ViewBag.FieldTypes = fieldTypes;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateFormTemplate(FormTemplateViewModel formTemplate)
        {
            if (ModelState.IsValid)
            {
                bool success = await _apiService.CreateFormTemplateAsync(formTemplate); // Submit template to API
                if (success)
                {
                    TempData["Message"] = "Form Template created successfully!";
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    TempData["Error"] = "Error creating form template.";
                    return View(formTemplate);
                }
            }

            // Re-fetch field types in case of error
            var fieldTypes = await _apiService.GetFieldTypesAsync();
            ViewBag.FieldTypes = fieldTypes;
            return View(formTemplate);
        }
    }
}
