using DnDSpellBook.Infrastructure.Data;
using DnDSpellBook.Infrastructure.Identity;
using DnDSpellBook.Infrastructure.RabbitMq;
using DnDSpellBook.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}
