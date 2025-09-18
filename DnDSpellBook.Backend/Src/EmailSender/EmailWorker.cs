using DnDSpellBook.Contracts;
using DnDSpellBook.Infrastructure.RabbitMq.Common.Interfaces;
using DnDSpellBook.Infrastructure.Smtp.Common.Interfaces;
using Serilog;

namespace EmailSender;

public class EmailWorker(IRabbitMqConsumerService rabbitMqConsumerService, ISmtpService smtpService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await rabbitMqConsumerService.ConsumeAsync<EmailContractSettings, EmailContract>(
                new EmailContractSettings(),
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
