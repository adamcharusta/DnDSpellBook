namespace DnDSpellBook.Infrastructure.Smtp;

public class SmtpSettings
{
    public string? Host { get; set; }
    public int Port { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public bool UseSsl { get; set; }
    public string? SenderName { get; set; }
}
