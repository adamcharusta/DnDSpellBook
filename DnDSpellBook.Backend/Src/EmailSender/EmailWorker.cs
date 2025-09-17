using DnDSpellBook.Contracts;
using DnDSpellBook.Infrastructure.RabbitMq;
using DnDSpellBook.Infrastructure.Smtp;
using Serilog;

namespace EmailSender;

public class EmailWorker(IRabbitMqService rabbitMqService, ISmtpService smtpService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await rabbitMqService.ConsumeAsync<EmailContractSettings, EmailContract>(new EmailContractSettings(),
                async (msg, ct) =>
                {
                    SendEmail(msg.To, msg.Subject, msg.Body);
                    await Task.CompletedTask;
                }, stoppingToken);

            await Task.Delay(1000, stoppingToken);
        }
    }

    private void SendEmail(string to, string subject, string body)
    {
        smtpService.SendEmail(to, subject, body);
        Log.Information("Sending email to {To} with subject {Subject} and {Body}", to, subject, body);
    }
}
