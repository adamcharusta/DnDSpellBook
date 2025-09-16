using Ardalis.GuardClauses;
using DnDSpellBook.Application;
using DnDSpellBook.Infrastructure;
using DnDSpellBook.Web;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var seqConnection = builder.Configuration.GetConnectionString("SeqConnection");
Guard.Against.Null(seqConnection, message: "Connection string 'SeqConnection' not found.");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "web")
    .WriteTo.Console()
    .WriteTo.Seq(seqConnection)
    .CreateLogger();

try
{
    builder.Services
        .UseInfrastructureServices(builder.Configuration)
        .UseApplicationServices()
        .UseWebServices();

    var app = builder.Build();

    await app.AddWebApplicationAsync();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
