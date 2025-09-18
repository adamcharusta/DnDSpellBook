using System.Text.Json;
using DnDSpellBook.Contracts.Common;
using DnDSpellBook.Infrastructure.RabbitMq.Common.Factory;
using DnDSpellBook.Infrastructure.RabbitMq.Common.Interfaces;
using DnDSpellBook.Infrastructure.RabbitMq.Common.Settings;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DnDSpellBook.Infrastructure.RabbitMq.Services;

public class RabbitMqPublisherService(IOptions<RabbitMqSettings> options)
    : RabbitMqServiceFactory(options), IRabbitMqPublisherService
{
    public async Task PublishAsync<TContract, TMessage>(TContract contract, TMessage message,
        CancellationToken ct = default)
        where TContract : BaseContractSettings
        where TMessage : class
    {
        var conn = await _connLazy.Value;
        await using var ch = await conn.CreateChannelAsync(cancellationToken: ct);

        await SetupQueueAndExchangeAsync(ch, contract, ct);

        var body = JsonSerializer.SerializeToUtf8Bytes(message, _json);

        await ch.BasicPublishAsync(
            contract.Exchange,
            string.IsNullOrWhiteSpace(contract.Exchange)
                ? contract.QueueName
                : contract.RoutingKey,
            body,
            ct
        );

        await ch.CloseAsync(ct);
    }
}
