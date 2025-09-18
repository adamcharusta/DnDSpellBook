using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Serilog;

namespace DnDSpellBook.Infrastructure.Smtp;

public class SmtpService(IOptions<SmtpSettings> options) : ISmtpService
{
    private readonly SmtpSettings _settings = options.Value;

    public void SendEmail(string to, string subject, string body)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.UserName));
            email.To.Add(new MailboxAddress(to, to));

            email.Subject = subject;
            //TODO
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            smtp.Connect(_settings.Host, _settings.Port, _settings.UseSsl);

            smtp.Authenticate(_settings.UserName, _settings.Password);

            if (!smtp.IsConnected || !smtp.IsAuthenticated)
            {
                throw new Exception("SMTP connection or authentication failed.");
            }

            smtp.Send(email);
            smtp.Disconnect(true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to send email to {To} with subject {Subject}", to, subject);
        }
    }
}
