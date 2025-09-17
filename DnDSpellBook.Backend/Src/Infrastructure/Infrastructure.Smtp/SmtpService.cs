using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Serilog;

namespace DnDSpellBook.Infrastructure.Smtp;

public class SmtpService(SmtpSettings settings) : ISmtpService
{
    public void SendEmail(string to, string subject, string body)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(settings.SenderName, settings.UserName));
            email.To.Add(new MailboxAddress(to, to));

            email.Subject = subject;
            //TODO
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            smtp.Connect(settings.Host, settings.Port, settings.UseSsl);

            smtp.Authenticate(settings.UserName, settings.Password);

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
