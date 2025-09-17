using DnDSpellBook.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DnDSpellBook.Infrastructure.Services;

internal static class DependencyInjection
{
    public static IServiceCollection UseServices(this IServiceCollection services)
    {
        services.AddTransient<IEmailService, EmailService>();

        return services;
    }
}
