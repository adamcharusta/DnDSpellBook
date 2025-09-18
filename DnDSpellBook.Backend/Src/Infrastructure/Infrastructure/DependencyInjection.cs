using DnDSpellBook.Infrastructure.Data;
using DnDSpellBook.Infrastructure.Identity;
using DnDSpellBook.Infrastructure.RabbitMq;
using DnDSpellBook.Infrastructure.RabbitMq.Common.Settings;
using DnDSpellBook.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DnDSpellBook.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection UseInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.UseDataServices(configuration);
        services.UseIdentityServices(configuration);
        services.UseRabbitMqPublisherService(configuration);
        services.UseServices();
        services.UseHealthChecks(configuration);

        return services;
    }

    private static IServiceCollection UseHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton<IConnection>(sp =>
            {
                var s = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
                var factory = new ConnectionFactory
                {
                    HostName = s.HostName,
                    Port = s.Port,
                    UserName = s.UserName,
                    Password = s.Password,
                    VirtualHost = s.VirtualHost
                };
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            })
            .AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("Db", HealthStatus.Unhealthy, ["ready"])
            .AddRabbitMQ(name: "RabbitMQ", failureStatus: HealthStatus.Unhealthy, tags: ["ready"]);

        return services;
    }
}
