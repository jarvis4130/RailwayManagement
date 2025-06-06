
namespace RailwayManagementApi.Interfaces
{
    public interface INotificationService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendResetEmailAsync(string toEmail, string token);
    }

}