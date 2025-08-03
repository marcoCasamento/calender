using System.Net.Mail;

namespace Api.Services;

public class FakeMailSender : IMailSender
{
    //a real implementation would use an SMTP client or an email service API,
    //take parameters from configuration and secrets...
    public Task SendMailAsync(MailMessage mailMessage, CancellationToken cancellationToken)
    {
        // Simulate sending mail by writing a message to the console
        // In a real implementation, you would use an SMTP client or an email service API
        Console.WriteLine($"Sending email to: {mailMessage.To}");
        Console.WriteLine($"Subject: {mailMessage.Subject}");
        Console.WriteLine($"Body: {mailMessage.Body}");
        return Task.CompletedTask;
    }
}