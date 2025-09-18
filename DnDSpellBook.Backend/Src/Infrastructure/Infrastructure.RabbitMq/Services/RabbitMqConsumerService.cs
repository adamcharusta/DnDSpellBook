using System.Text.Json;
using Ardalis.GuardClauses;
using DnDSpellBook.Contracts.Common;
using DnDSpellBook.Infrastructure.RabbitMq.Common.Factory;
using DnDSpellBook.Infrastructure.RabbitMq.Common.Interfaces;
using DnDSpellBook.Infrastructure.RabbitMq.Common.Settings;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Serilog;

namespace DnDSpellBook.Infrastructure.RabbitMq.Services;

public class RabbitMqConsumerService(IOptions<RabbitMqSettings> options)
    : RabbitMqServiceFactory(options), IRabbitMqConsumerService
{
    public async Task ConsumeAsync<TContract, TMessage>(
        TContract contract,
        Func<TMessage, CancellationToken, Task> onMessageReceived,
        CancellationToken ct = default)
        where TContract : BaseContractSettings
        where TMessage : class
    {
        Guard.Against.Null(onMessageReceived, nameof(onMessageReceived));

        var conn = await _connLazy.Value;
        await using var ch = await conn.CreateChannelAsync(cancellationToken: ct);

        await SetupQueueAndExchangeAsync(ch, contract, ct);

        await ch.BasicQosAsync(0, 1, false, ct);

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var reg = ct.Register(() => tcs.TrySetCanceled(ct));

        var consumer = new AsyncEventingBasicConsumer(ch);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var payload = JsonSerializer.Deserialize<TMessage>(ea.Body.Span, _json)!;

                await onMessageReceived(payload, ct);

                await ch.BasicAckAsync(ea.DeliveryTag, false, ct);
                tcs.TrySetResult();
            }
            catch (Exception ex)
            {
                await ch.BasicNackAsync(ea.DeliveryTag, false, false, ct);
                tcs.TrySetException(ex);
                Log.Error(ex, "Error processing message from queue {QueueName}.", contract.QueueName);
            }
        };

        var tag = await ch.BasicConsumeAsync(contract.QueueName, false, consumer, ct);

        try { await tcs.Task; }
        finally
        {
            try { await ch.BasicCancelAsync(tag, cancellationToken: ct); }
            catch (OperationCanceledException ex)
            {
                Log.Debug(ex, "Cancelled BasicCancelAsync in ConsumeAsync.");
            }
            catch (ObjectDisposedException ex)
            {
                Log.Debug(ex, "Object already destroyed on BasicCancelAsync.");
            }
            catch (AlreadyClosedException ex)
            {
                Log.Debug(ex, "Connection already closed on CloseAsync.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in BasicCancelAsync in ConsumeAsync.");
            }

            await ch.CloseAsync(ct);
        }
    }
}
