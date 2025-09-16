using DnDSpellBook.Application.Common.Interfaces;
using DnDSpellBook.Infrastructure.Data;
using DnDSpellBook.Web.Infrastructure;
using DnDSpellBook.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DnDSpellBook.Web;

public static class DependencyInjection
{
    public static IServiceCollection UseWebServices(this IServiceCollection services)
    {
        services.AddScoped<IUser, CurrentUser>();
        services.AddHttpContextAccessor();
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddExceptionHandler<ExceptionHandler>();

        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        return services;
    }

    public static async Task<WebApplication> AddWebApplicationAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHsts();
        }

        await app.InitialiseDatabaseAsync();

        app.UseHealthChecks("/health");
        app.UseHttpsRedirection();
        app.UseExceptionHandler(options => { });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapEndpoints();
        return app;
    }
}
