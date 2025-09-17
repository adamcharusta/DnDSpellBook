namespace DnDSpellBook.Infrastructure.Smtp;

public interface ISmtpService
{
    void SendEmail(string to, string subject, string body);
}
