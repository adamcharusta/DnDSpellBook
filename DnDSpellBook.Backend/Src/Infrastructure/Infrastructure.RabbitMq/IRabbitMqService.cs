using DnDSpellBook.Contracts.Common;

namespace DnDSpellBook.Infrastructure.RabbitMq;

public interface IRabbitMqService
{
    Task PublishAsync<TContract, TMessage>(TContract contract, TMessage message, CancellationToken ct = default)
        where TContract : BaseContractSettings
        where TMessage : class;

    Task ConsumeAsync<TContract, TMessage>(
        TContract contract,
        Func<TMessage, CancellationToken, Task> onMessageReceived,
        CancellationToken ct = default)
        where TContract : BaseContractSettings
        where TMessage : class;
}
