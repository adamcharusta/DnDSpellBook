using System.Threading.RateLimiting;
using Ardalis.GuardClauses;
using DnDSpellBook.Infrastructure.Data;
using DnDSpellBook.Web.Infrastructure;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;

namespace DnDSpellBook.Web;

public static class DependencyInjection
{
    public static IServiceCollection UseWebServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "DnDSpellBook API", Version = "v1" });

            c.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "AccessToken",
                    In = ParameterLocation.Header,
                    Description = "Type: Bearer {token}"
                });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services.AddExceptionHandler<ExceptionHandler>();
        services.AddProblemDetails();

        services.Configure<ApiBehaviorOptions>(o => o.SuppressModelStateInvalidFilter = true);

        services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(15);
            setup.MaximumHistoryEntriesPerEndpoint(60);
            setup.AddHealthCheckEndpoint("App", "/health/ready");
        }).AddInMemoryStorage();

        services.AddRateLimiter(o =>
        {
            o.AddFixedWindowLimiter("default", opt =>
            {
                opt.Window = TimeSpan.FromSeconds(1);
                opt.PermitLimit = 100;
            });

            o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
                RateLimitPartition.GetFixedWindowLimiter("global",
                    _ => new FixedWindowRateLimiterOptions { Window = TimeSpan.FromSeconds(1), PermitLimit = 100 }));
        });

        var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        Guard.Against.NullOrEmpty(origins, nameof(origins)); // ✅ pilnuj niepustej listy

        services.AddCors(o =>
        {
            o.AddPolicy("Default", b =>
                b.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        return services;
    }

    public static async Task<WebApplication> AddWebApplicationAsync(this WebApplication app)
    {
        app.UseExceptionHandler();

        app.UseHttpsRedirection();
        app.UseRateLimiter();

        app.MapHealthChecks("/health/ready",
            new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("ready"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        app.MapHealthChecksUI(o =>
        {
            o.UIPath = "/health-ui";
            o.ApiPath = "/health-ui-api";
        });

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

        app.UseCors("Default");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapEndpoints();

        return app;
    }
}
