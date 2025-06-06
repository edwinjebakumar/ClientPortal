﻿using ClientPortalUI.Models;
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

        // Authentication Methods
        public async Task<AuthResult> LoginAsync(LoginViewModel model)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await client.PostAsJsonAsync("Auth/login", model);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResult>();
                    return result ?? new AuthResult { Succeeded = false, Errors = new List<string> { "Invalid response from server" } };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Login failed. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return new AuthResult { 
                    Succeeded = false, 
                    Errors = new List<string> { "Login failed. Please check your credentials." } 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", model.Email);
                return new AuthResult { 
                    Succeeded = false, 
                    Errors = new List<string> { "An error occurred during login." } 
                };
            }
        }

        public async Task<AuthResult> RegisterAsync(RegisterViewModel model)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var registerRequest = new
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword,
                    Role = model.Role,
                    ClientId = model.ClientId
                };

                var response = await client.PostAsJsonAsync("Auth/register", registerRequest);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResult>();
                    return result ?? new AuthResult { Succeeded = false, Errors = new List<string> { "Invalid response from server" } };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Registration failed. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return new AuthResult { 
                    Succeeded = false, 
                    Errors = new List<string> { "Registration failed. Please try again." } 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Email}", model.Email);
                return new AuthResult { 
                    Succeeded = false, 
                    Errors = new List<string> { "An error occurred during registration." } 
                };
            }
        }

        // Retrieves all form assignments for a given client
        public async Task<List<FormAssignmentViewModel>> GetFormAssignmentsAsync(int clientId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");            
            var endpoint = $"FormAssignments/{clientId}";
            try
            {
                var assignments = await client.GetFromJsonAsync<List<FormAssignmentViewModel>>(endpoint);
                return assignments ?? new List<FormAssignmentViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving form assignments for client {ClientId}", clientId);
                throw new ApplicationException("Error retrieving form assignments.", ex);
            }
        }        // Retrieves a specific form assignment by assignment ID
        public async Task<FormAssignmentViewModel> GetFormAssignmentAsync(int assignmentId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = $"FormAssignments/assignment/{assignmentId}";
            try
            {
                var assignment = await client.GetFromJsonAsync<FormAssignmentViewModel>(endpoint);
                return assignment ?? new FormAssignmentViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving form assignment {AssignmentId}", assignmentId);
                throw new ApplicationException("Error retrieving form assignment.", ex);
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
                _logger.LogError(ex, "Error retrieving form template {AssignmentId}", assignmentId);
                throw new ApplicationException("Error retrieving form template.", ex);
            }
        }        
        
        // Submits a form submission
        public async Task<SubmissionResponseViewModel> SubmitFormAsync(SubmissionViewModel submission)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = "Submissions";  // Added 'api/' prefix
            try
            {                
                var submissionRequest = submission;

                var response = await client.PostAsJsonAsync(endpoint, submissionRequest);
                if (response.IsSuccessStatusCode)
                {
                    var submissionResponse = await response.Content.ReadFromJsonAsync<SubmissionResponseViewModel>();
                    return submissionResponse ?? new SubmissionResponseViewModel();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error submitting form. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    throw new ApplicationException($"Error submitting form. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting form");
                throw new ApplicationException("Error submitting form.", ex);
            }
        }

        // Retrieves the latest submission for a given assignment
        public async Task<SubmissionResponseViewModel?> GetLatestSubmissionAsync(int assignmentId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = $"Submissions/latest/{assignmentId}";
            try
            {
                var submission = await client.GetFromJsonAsync<SubmissionResponseViewModel>(endpoint);
                return submission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest submission for assignment {AssignmentId}", assignmentId);
                return null; // Return null for no submission
            }
        }

        // Retrieves activity history for a given submission
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
                _logger.LogError(ex, "Error retrieving activity history for submission {SubmissionId}", submissionId);
                throw new ApplicationException("Error retrieving activity history for the submission.", ex);
            }
        }

        // Retrieves activity history by user id
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
                _logger.LogError(ex, "Error retrieving activity history for user {UserId}", userId);
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

        // Retrieves all form templates
        public async Task<List<FormTemplateViewModel>> GetFormTemplatesAsync()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = "FormTemplates";
            try
            {
                var templates = await client.GetFromJsonAsync<List<FormTemplateViewModel>>(endpoint);
                return templates ?? new List<FormTemplateViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving form templates");
                throw new ApplicationException("Error retrieving form templates.", ex);
            }
        }

        // Updates an existing form template
        public async Task<bool> UpdateFormTemplateAsync(FormTemplateViewModel template)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = $"FormTemplates/{template.TemplateId}";
            try
            {
                _logger.LogInformation("Updating form template: {TemplateName}", template.Name);

                var response = await client.PutAsJsonAsync(endpoint, template);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated form template: {TemplateName}", template.Name);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update form template. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating form template: {TemplateName}", template.Name);
                throw new ApplicationException("Error updating form template.", ex);
            }
        }

        // Deletes a form template
        public async Task<bool> DeleteFormTemplateAsync(int templateId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = $"FormTemplates/{templateId}";
            try
            {
                _logger.LogInformation("Deleting form template: {TemplateId}", templateId);

                var response = await client.DeleteAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully deleted form template: {TemplateId}", templateId);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete form template. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting form template: {TemplateId}", templateId);
                throw new ApplicationException("Error deleting form template.", ex);
            }
        }

        // Get all clients with their assigned forms count
        public async Task<List<ClientViewModel>> GetClientsAsync()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = "Clients";
            try
            {
                var clients = await client.GetFromJsonAsync<List<ClientViewModel>>(endpoint);
                return clients ?? new List<ClientViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clients");
                throw new ApplicationException("Error retrieving clients.", ex);
            }
        }

        // Get a specific client by ID
        public async Task<ClientViewModel> GetClientAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = $"Clients/{id}";
            try
            {
                var clientInfo = await client.GetFromJsonAsync<ClientViewModel>(endpoint);
                return clientInfo ?? new ClientViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client {ClientId}", id);
                throw new ApplicationException("Error retrieving client details.", ex);
            }
        }

        // Create a new client
        public async Task<bool> CreateClientAsync(ClientViewModel client)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await httpClient.PostAsJsonAsync("Clients", client);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client: {ClientName}", client.Name);
                throw new ApplicationException("Error creating client.", ex);
            }
        }

        // Update an existing client
        public async Task<bool> UpdateClientAsync(ClientViewModel client)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await httpClient.PutAsJsonAsync($"Clients/{client.Id}", client);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client: {ClientId}", client.Id);
                throw new ApplicationException("Error updating client.", ex);
            }
        }

        // Delete a client
        public async Task<bool> DeleteClientAsync(int id)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await httpClient.DeleteAsync($"Clients/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client: {ClientId}", id);
                throw new ApplicationException("Error deleting client.", ex);
            }
        }

        // Assign a form template to a client
        public async Task<bool> AssignFormTemplateAsync(int clientId, int formTemplateId, string notes)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var request = new
                {
                    ClientId = clientId,
                    FormTemplateId = formTemplateId,
                    Notes = notes
                };

                var response = await httpClient.PostAsJsonAsync("FormAssignments/assign", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning form template {TemplateId} to client {ClientId}", 
                    formTemplateId, clientId);
                throw new ApplicationException("Error assigning form template.", ex);
            }
        }

        public async Task<List<SubmissionResponseViewModel>> GetSubmissionsByAssignmentIdAsync(int assignmentId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = $"Submissions/byAssignment/{assignmentId}";
            try
            {
                var submissions = await client.GetFromJsonAsync<List<SubmissionResponseViewModel>>(endpoint);
                return submissions ?? new List<SubmissionResponseViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving submissions for assignment {AssignmentId}", assignmentId);
                throw new ApplicationException("Error retrieving submissions.", ex);
            }
        }

        public async Task<SubmissionResponseViewModel> GetSubmissionDetailsAsync(int submissionId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var endpoint = $"Submissions/details/{submissionId}";
            try
            {
                var submission = await client.GetFromJsonAsync<SubmissionResponseViewModel>(endpoint);
                if (submission == null)
                {
                    throw new ApplicationException($"Submission {submissionId} not found.");
                }
                return submission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving submission details for {SubmissionId}", submissionId);
                throw new ApplicationException("Error retrieving submission details.", ex);
            }
        }        // Check username availability
        public async Task<bool> CheckUsernameAvailabilityAsync(string username)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.GetFromJsonAsync<AvailabilityResponse>($"Auth/check-username/{username}");
                return response?.IsAvailable ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username availability");
                return false;
            }
        }

        private class AvailabilityResponse
        {
            public bool IsAvailable { get; set; }
        }

        // User Management Methods
        public async Task<List<UserViewModel>> GetUsersAsync()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var users = await client.GetFromJsonAsync<List<UserViewModel>>("Auth/users");
                return users ?? new List<UserViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return new List<UserViewModel>();
            }
        }

        public async Task<UserViewModel> GetUserAsync(string userId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var user = await client.GetFromJsonAsync<UserViewModel>($"Auth/users/{userId}");
                return user ?? new UserViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", userId);
                return new UserViewModel();
            }
        }

        public async Task<AuthResult> UpdateUserAsync(EditUserViewModel model)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await client.PutAsJsonAsync($"Auth/users/{model.Id}", model);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResult>();
                    return result ?? new AuthResult { Succeeded = false, Errors = new List<string> { "Invalid response from server" } };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("User update failed. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return new AuthResult { 
                    Succeeded = false, 
                    Errors = new List<string> { "User update failed. Please try again." } 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", model.Id);
                return new AuthResult { 
                    Succeeded = false, 
                    Errors = new List<string> { "An error occurred during user update." } 
                };
            }
        }

        public async Task<List<RoleViewModel>> GetRolesAsync()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var roles = await client.GetFromJsonAsync<List<RoleViewModel>>("Auth/roles");
                return roles ?? new List<RoleViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return new List<RoleViewModel>();
            }
        }
    }
}
