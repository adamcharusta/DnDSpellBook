using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DnDSpellBook.Infrastructure.RabbitMq;

public static class DependencyInjection
{
    public static IServiceCollection UseRabbitMqServices(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqSettings = new RabbitMqSettings();

        configuration.GetSection("RabbitMq").Bind(rabbitMqSettings);

        Guard.Against.NullOrWhiteSpace(rabbitMqSettings.HostName, nameof(rabbitMqSettings.HostName));
        Guard.Against.NegativeOrZero(rabbitMqSettings.Port, nameof(rabbitMqSettings.Port));
        Guard.Against.NullOrWhiteSpace(rabbitMqSettings.UserName, nameof(rabbitMqSettings.UserName));
        Guard.Against.NullOrWhiteSpace(rabbitMqSettings.Password, nameof(rabbitMqSettings.Password));
        Guard.Against.NullOrWhiteSpace(rabbitMqSettings.VirtualHost, nameof(rabbitMqSettings.VirtualHost));

        services.AddSingleton(rabbitMqSettings);

        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        return services;
    }
}
