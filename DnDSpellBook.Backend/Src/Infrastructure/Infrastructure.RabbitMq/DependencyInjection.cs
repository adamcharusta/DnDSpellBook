using DnDSpellBook.Infrastructure.RabbitMq.Common.Interfaces;
using DnDSpellBook.Infrastructure.RabbitMq.Common.Settings;
using DnDSpellBook.Infrastructure.RabbitMq.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DnDSpellBook.Infrastructure.RabbitMq;

public static class DependencyInjection
{
    public static IServiceCollection UseRabbitMqPublisherService(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.UseRabbitMqSettings(configuration);
        services.AddSingleton<IRabbitMqPublisherService, RabbitMqPublisherService>();

        return services;
    }

    public static IServiceCollection UseRabbitMqConsumerService(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.UseRabbitMqSettings(configuration);
        services.AddSingleton<IRabbitMqConsumerService, RabbitMqConsumerService>();

        return services;
    }

    private static IServiceCollection UseRabbitMqSettings(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RabbitMqSettings>()
            .Bind(configuration.GetSection("RabbitMq"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
