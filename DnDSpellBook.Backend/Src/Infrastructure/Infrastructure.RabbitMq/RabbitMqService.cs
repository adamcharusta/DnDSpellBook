using System.Text.Json;
using Ardalis.GuardClauses;
using DnDSpellBook.Contracts.Common;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Serilog;
using ConnectionFactory = RabbitMQ.Client.ConnectionFactory;
using IChannel = RabbitMQ.Client.IChannel;

namespace DnDSpellBook.Infrastructure.RabbitMq;

public class RabbitMqService : IRabbitMqService, IAsyncDisposable
{
    private readonly Lazy<Task<IConnection>> _connLazy;
    private readonly ConnectionFactory _factory;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public RabbitMqService(IOptions<RabbitMqSettings> options)
    {
        var settings = options.Value;

        _factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            VirtualHost = settings.VirtualHost,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };

        _connLazy = new Lazy<Task<IConnection>>(() => _factory.CreateConnectionAsync(),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connLazy.IsValueCreated)
        {
            var c = await _connLazy.Value;
            try
            {
                if (c.IsOpen)
                {
                    await c.CloseAsync();
                }
            }
            catch { }

            c.Dispose();
        }
    }

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

        await DeclareQueueAsync(ch, contract, ct);
        if (!string.IsNullOrWhiteSpace(contract.Exchange))
        {
            await ch.ExchangeDeclareAsync(contract.Exchange, ExchangeType.Topic,
                true, false, cancellationToken: ct);
            await ch.QueueBindAsync(contract.QueueName, contract.Exchange, contract.RoutingKey, cancellationToken: ct);
        }

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


    public async Task PublishAsync<TContract, TMessage>(TContract contract, TMessage message,
        CancellationToken ct = default)
        where TContract : BaseContractSettings
        where TMessage : class
    {
        var conn = await _connLazy.Value;
        await using var ch = await conn.CreateChannelAsync(cancellationToken: ct);

        await DeclareQueueAsync(ch, contract, ct);

        if (!string.IsNullOrWhiteSpace(contract.Exchange))
        {
            await ch.ExchangeDeclareAsync(contract.Exchange, ExchangeType.Topic,
                true, false, cancellationToken: ct);
            await ch.QueueBindAsync(contract.QueueName, contract.Exchange, contract.RoutingKey, cancellationToken: ct);
        }

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

    private static async Task DeclareQueueAsync(
        IChannel ch, BaseContractSettings s, CancellationToken ct)
    {
        if (s.Passive)
        {
            await ch.QueueDeclarePassiveAsync(s.QueueName, ct);
        }
        else
        {
            await ch.QueueDeclareAsync(
                s.QueueName,
                s.Durable,
                s.Exclusive,
                s.AutoDelete,
                s.Arguments,
                cancellationToken: ct);
        }
    }
}
