using DnDSpellBook.Application.Common.Interfaces;
using DnDSpellBook.Contracts;
using DnDSpellBook.Infrastructure.RabbitMq.Common.Interfaces;

namespace DnDSpellBook.Infrastructure.Services;

public class EmailService(IRabbitMqPublisherService rabbitMqPublisherService) : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var settings = new EmailContractSettings();
        var emailContract = new EmailContract(to, subject, body);

        await rabbitMqPublisherService.PublishAsync(settings, emailContract);
    }
}
