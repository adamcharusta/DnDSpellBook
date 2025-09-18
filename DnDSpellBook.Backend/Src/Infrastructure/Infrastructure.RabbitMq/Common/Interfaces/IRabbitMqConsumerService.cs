using DnDSpellBook.Contracts.Common;

namespace DnDSpellBook.Infrastructure.RabbitMq.Common.Interfaces;

public interface IRabbitMqConsumerService
{
    Task ConsumeAsync<TContract, TMessage>(
        TContract contract,
        Func<TMessage, CancellationToken, Task> onMessageReceived,
        CancellationToken ct = default)
        where TContract : BaseContractSettings
        where TMessage : class;
}
