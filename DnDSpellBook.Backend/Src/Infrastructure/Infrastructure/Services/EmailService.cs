using DnDSpellBook.Application.Common.Interfaces;
using DnDSpellBook.Contracts;
using DnDSpellBook.Infrastructure.RabbitMq;

namespace DnDSpellBook.Infrastructure.Services;

public class EmailService(IRabbitMqService rabbitMqService) : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var settings = new EmailContractSettings();
        var emailContract = new EmailContract(to, subject, body);

        await rabbitMqService.PublishAsync(settings, emailContract);
    }
}
