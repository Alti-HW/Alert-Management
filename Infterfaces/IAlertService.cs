using Alert_Management.Models;

namespace Alert_Management.Infterfaces
{
    public interface IAlertService
    {
        Task<List<Alert>> GetAllAlertsAsync();

        // Method to resolve an alert
        Task<bool> ResolveAlertAsync(Guid alertId);

        // Method to delete an alert
        Task<bool> DeleteAlertAsync(Guid alertId);
    }
}
