// Services/NotificationService.cs
using System.Net;
using System.Net.Mail;
using RailwayManagementApi.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

public class NotificationService : INotificationService
{
    private readonly IConfiguration _config;

    public NotificationService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendResetEmailAsync(string toEmail, string token)
    {
        var clientAppUrl = _config["ClientAppUrl"];
        var resetLink = $"{clientAppUrl}/reset-password?token={token}";

        string subject = "Reset Your Password";
        string body = $@"
        <p>Hello,</p>
        <p>You requested a password reset. Click the link below to reset your password:</p>
        <p><a href='{resetLink}'>Reset Password</a></p>
        <p>If you did not request this, please ignore this email.</p>
        <p>Regards,<br/>Railway Management</p>";

        await SendEmailAsync(toEmail, subject, body);
    }


    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var apiKey = _config["SendGrid:ApiKey"];
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(_config["SendGrid:From"], "Railway Management");
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: body);
        var response = await client.SendEmailAsync(msg);

        if ((int)response.StatusCode >= 400)
        {
            var responseBody = await response.Body.ReadAsStringAsync();
            Console.WriteLine($"Email send failed: {response.StatusCode}\n{responseBody}");
            throw new Exception("SendGrid email send failed.");
        }
    }
}
