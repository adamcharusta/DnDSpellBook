using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DnDSpellBook.Infrastructure.Smtp;

public static class DependencyInjection
{
    public static IServiceCollection UseSmtpServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SmtpSettings>()
            .Bind(configuration.GetSection("Smtp"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ISmtpService, SmtpService>();

        return services;
    }
}
