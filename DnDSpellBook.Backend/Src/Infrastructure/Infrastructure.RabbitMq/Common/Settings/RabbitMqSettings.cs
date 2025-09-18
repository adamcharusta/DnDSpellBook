using System.ComponentModel.DataAnnotations;

namespace DnDSpellBook.Infrastructure.RabbitMq.Common.Settings;

public class RabbitMqSettings
{
    [Required] public required string HostName { get; set; }

    [Range(1, 65535)] public int Port { get; set; }

    [Required] [MaxLength(128)] public required string UserName { get; set; }

    [Required] [MaxLength(128)] public required string Password { get; set; }

    [Required] [MaxLength(128)] public required string VirtualHost { get; set; }
}
