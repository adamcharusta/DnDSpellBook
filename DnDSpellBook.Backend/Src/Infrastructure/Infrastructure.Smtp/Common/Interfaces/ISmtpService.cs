namespace DnDSpellBook.Infrastructure.Smtp.Common.Interfaces;

public interface ISmtpService
{
    void SendEmail(string to, string subject, string body);
}
