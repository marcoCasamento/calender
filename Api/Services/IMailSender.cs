using System.Net.Mail;

namespace Api.Services;

public interface IMailSender
{
    public Task SendMailAsync(MailMessage mailMessage, CancellationToken cancellationToken);
    //other methods can be added as needed like 

}
