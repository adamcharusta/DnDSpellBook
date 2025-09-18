using System.Text.Json;
using DnDSpellBook.Contracts.Common;
using DnDSpellBook.Infrastructure.RabbitMq.Common.Settings;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DnDSpellBook.Infrastructure.RabbitMq.Common.Factory;

public abstract class RabbitMqServiceFactory : IAsyncDisposable
{
    protected readonly Lazy<Task<IConnection>> _connLazy;
    private readonly ConnectionFactory _factory;
    protected readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    protected RabbitMqServiceFactory(IOptions<RabbitMqSettings> options)
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

    protected static async Task SetupQueueAndExchangeAsync<TContract>(
        IChannel ch, TContract contract, CancellationToken ct)
        where TContract : BaseContractSettings
    {
        if (contract.Passive)
        {
            await ch.QueueDeclarePassiveAsync(contract.QueueName, ct);
        }
        else
        {
            await ch.QueueDeclareAsync(
                contract.QueueName,
                contract.Durable,
                contract.Exclusive,
                contract.AutoDelete,
                contract.Arguments,
                cancellationToken: ct);
        }

        if (!string.IsNullOrWhiteSpace(contract.Exchange))
        {
            await ch.ExchangeDeclareAsync(contract.Exchange, ExchangeType.Topic,
                true, false, cancellationToken: ct);
            await ch.QueueBindAsync(contract.QueueName, contract.Exchange, contract.RoutingKey, cancellationToken: ct);
        }
    }
}
