using Ardalis.GuardClauses;
using EmailSender;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

var seqConnection = builder.Configuration.GetConnectionString("SeqConnection");
Guard.Against.Null(seqConnection, message: "Connection string 'SeqConnection' not found.");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "email-sender")
    .WriteTo.Console()
    .WriteTo.Seq(seqConnection)
    .CreateLogger();

try
{
    builder.Services.AddHostedService<Worker>();

    var host = builder.Build();
    host.Run();
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
