using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DnDSpellBook.Infrastructure.Smtp;

public static class DependencyInjection
{
    public static IServiceCollection UseSmtpServices(this IServiceCollection services, IConfiguration configuration)
    {
        var smtpSettings = new SmtpSettings();

        configuration.GetSection("Smtp").Bind(smtpSettings);

        Guard.Against.NullOrWhiteSpace(smtpSettings.Host, nameof(smtpSettings.Host));
        Guard.Against.NegativeOrZero(smtpSettings.Port, nameof(smtpSettings.Port));
        Guard.Against.NullOrWhiteSpace(smtpSettings.UserName, nameof(smtpSettings.UserName));
        Guard.Against.NullOrWhiteSpace(smtpSettings.Password, nameof(smtpSettings.Password));
        Guard.Against.NullOrWhiteSpace(smtpSettings.SenderName, nameof(smtpSettings.SenderName));

        services.AddSingleton(smtpSettings);
        services.AddSingleton<ISmtpService, SmtpService>();

        return services;
    }
}
