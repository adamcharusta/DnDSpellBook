using DnDSpellBook.Application.Common.Interfaces;
using DnDSpellBook.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DnDSpellBook.Infrastructure.Identity;

internal static class DependencyInjection
{
    public static IServiceCollection UseIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentityCore<ApplicationUser>(o =>
            {
                o.User.RequireUniqueEmail = true;
                o.SignIn.RequireConfirmedEmail = true;

                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = true;
                o.Password.RequireNonAlphanumeric = true;
                o.Password.RequireUppercase = true;
                o.Password.RequiredLength = 8;
                o.Password.RequiredUniqueChars = 1;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddApiEndpoints();

        services.AddTransient<IIdentityService, IdentityService>();

        services.AddAuthorization();
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
                options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
            })
            .AddBearerToken(IdentityConstants.BearerScheme);

        services.AddDataProtection();

        return services;
    }
}
