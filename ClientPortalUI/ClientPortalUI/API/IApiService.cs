using ClientPortalUI.Models;

namespace ClientPortalUI.API
{
    public interface IApiService
    {
        Task<List<FormAssignmentViewModel>> GetFormAssignmentsAsync(int clientId);
        Task<FormTemplateViewModel> GetFormTemplateAsync(int assignmentId);
        Task<bool> CreateFormTemplateAsync(FormTemplateViewModel formTemplate);
        Task<List<FieldTypeViewModel>> GetFieldTypesAsync();
        Task<SubmissionResponseViewModel> SubmitFormAsync(SubmissionViewModel submission);
        Task<List<ActivityHistoryViewModel>> GetActivityHistoryBySubmissionAsync(int submissionId);
        Task<List<ActivityHistoryViewModel>> GetActivityHistoryByUserAsync(string userId);
        Task<List<FormTemplateViewModel>> GetFormTemplatesAsync();
        Task<bool> UpdateFormTemplateAsync(FormTemplateViewModel template);
        Task<bool> DeleteFormTemplateAsync(int templateId);
        Task<List<ClientViewModel>> GetClientsAsync();
        Task<ClientViewModel> GetClientAsync(int id);
        Task<bool> CreateClientAsync(ClientViewModel client);
        Task<bool> UpdateClientAsync(ClientViewModel client);
        Task<bool> DeleteClientAsync(int id);
        Task<bool> AssignFormTemplateAsync(int clientId, int formTemplateId, string notes);
    }
}
