using ClientPortalUI.API;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClientPortalUI.Controllers
{
    public class ClientController : Controller
    {
        private readonly IApiService _apiService;

        public ClientController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> ClientDashboard()
        {
            // Replace this with your logic to get the logged-in client's ID.
            int clientId = GetLoggedInClientId();

            var assignments = await _apiService.GetFormAssignmentsAsync(clientId);
            return View(assignments);
        }

        private int GetLoggedInClientId()
        {
            // For demo purposes, we hard-code this value.
            return 2;  // For example, return 2 if it's EmiCorp
        }

        public async Task<IActionResult> FillForm(int assignmentId)
        {
            // Call your API service to get the form details (template, fields, assignment info)
            var formTemplate = await _apiService.GetFormTemplateAsync(assignmentId);
            if (formTemplate == null)
            {
                return NotFound();
            }
            return View(formTemplate); // Pass the data to the view
        }

    }

}
