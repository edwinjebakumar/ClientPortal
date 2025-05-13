using ClientPortalUI.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace ClientPortalUI.API
{
    public class ApiService : IApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiService> _logger;

        public ApiService(IHttpClientFactory httpClientFactory, ILogger<ApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // Retrieves all form assignments for a given client
        public async Task<List<FormAssignmentViewModel>> GetFormAssignmentsAsync(int clientId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = $"FormAssignments?clientId={clientId}";
            try
            {
                var assignments = await client.GetFromJsonAsync<List<FormAssignmentViewModel>>(endpoint);
                return assignments ?? new List<FormAssignmentViewModel>();
            }
            catch (Exception ex)
            {
                // Log exception as needed
                throw new ApplicationException("Error retrieving form assignments.", ex);
            }
        }

        // Retrieves details about a form template (including its fields) using the assignment identifier.
        public async Task<FormTemplateViewModel> GetFormTemplateAsync(int assignmentId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = $"FormTemplates/{assignmentId}";
            try
            {
                var template = await client.GetFromJsonAsync<FormTemplateViewModel>(endpoint);
                return template ?? new FormTemplateViewModel();
            }
            catch (Exception ex)
            {
                // Log exception as needed
                throw new ApplicationException("Error retrieving form template.", ex);
            }
        }

        // Submits a form submission. Depending on backend logic, this can create or update a submission.
        public async Task<SubmissionResponseViewModel> SubmitFormAsync(SubmissionViewModel submission)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = "Submissions";
            try
            {
                var response = await client.PostAsJsonAsync(endpoint, submission);
                if (response.IsSuccessStatusCode)
                {
                    var submissionResponse = await response.Content.ReadFromJsonAsync<SubmissionResponseViewModel>();
                    return submissionResponse ?? new SubmissionResponseViewModel();
                }
                else
                {
                    throw new ApplicationException($"Error submitting form. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Log exception as needed
                throw new ApplicationException("Error submitting form.", ex);
            }
        }

        // Retrieves activity history for a given submission (by submission id)
        public async Task<List<ActivityHistoryViewModel>> GetActivityHistoryBySubmissionAsync(int submissionId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = $"ActivityHistory/submission/{submissionId}";
            try
            {
                var history = await client.GetFromJsonAsync<List<ActivityHistoryViewModel>>(endpoint);
                return history ?? new List<ActivityHistoryViewModel>();
            }
            catch (Exception ex)
            {
                // Log exception as needed
                throw new ApplicationException("Error retrieving activity history for the submission.", ex);
            }
        }

        // Retrieves activity history by user id (to see all actions performed by a user)
        public async Task<List<ActivityHistoryViewModel>> GetActivityHistoryByUserAsync(string userId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = $"ActivityHistory/user/{userId}";
            try
            {
                var history = await client.GetFromJsonAsync<List<ActivityHistoryViewModel>>(endpoint);
                return history ?? new List<ActivityHistoryViewModel>();
            }
            catch (Exception ex)
            {
                // Log exception as needed
                throw new ApplicationException("Error retrieving activity history for the user.", ex);
            }
        }

        // Fetching available field types
        public async Task<List<FieldTypeViewModel>> GetFieldTypesAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var endpoint = "FieldTypes";
                
                var response = await client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to get field types. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    return new List<FieldTypeViewModel>();
                }

                var fieldTypes = await response.Content.ReadFromJsonAsync<List<FieldTypeViewModel>>();
                return fieldTypes ?? new List<FieldTypeViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving field types");
                throw new ApplicationException("Error retrieving field types. Please try again.", ex);
            }
        }

        // Creating a new form template
        public async Task<bool> CreateFormTemplateAsync(FormTemplateViewModel formTemplate)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                _logger.LogInformation("Creating form template: {TemplateName}", formTemplate.Name);

                var response = await client.PostAsJsonAsync("FormTemplates", formTemplate);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully created form template: {TemplateName}", formTemplate.Name);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create form template. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating form template: {TemplateName}", formTemplate.Name);
                throw new ApplicationException("Error creating form template.", ex);
            }
        }
    }
}
