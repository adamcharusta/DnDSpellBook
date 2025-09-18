using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DnDSpellBook.Infrastructure.RabbitMq;

public static class DependencyInjection
{
    public static IServiceCollection UseRabbitMqServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RabbitMqSettings>()
            .Bind(configuration.GetSection("RabbitMq"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        return services;
    }
}
