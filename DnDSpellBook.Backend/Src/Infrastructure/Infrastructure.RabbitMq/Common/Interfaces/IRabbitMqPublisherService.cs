using DnDSpellBook.Contracts.Common;

namespace DnDSpellBook.Infrastructure.RabbitMq.Common.Interfaces;

public interface IRabbitMqPublisherService
{
    Task PublishAsync<TContract, TMessage>(TContract contract, TMessage message, CancellationToken ct = default)
        where TContract : BaseContractSettings
        where TMessage : class;
}
