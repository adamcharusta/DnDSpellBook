using System.ComponentModel.DataAnnotations;

namespace DnDSpellBook.Infrastructure.Smtp;

public class SmtpSettings
{
    [Required] public required string Host { get; init; }

    [Range(1, 65535)] public int Port { get; init; }

    [Required] [EmailAddress] public required string UserName { get; set; }

    [Required] public required string Password { get; set; }

    public bool UseSsl { get; set; } = false;

    [MaxLength(128)] public required string SenderName { get; set; }
}
