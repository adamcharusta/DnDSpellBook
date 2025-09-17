namespace DnDSpellBook.Contracts.Common;

public abstract class BaseContractSettings
{
    public abstract string QueueName { get; }
    public abstract bool Durable { get; }
    public abstract bool Exclusive { get; }
    public abstract bool AutoDelete { get; }
    public abstract bool Passive { get; }
    public abstract string Exchange { get; }
    public abstract string RoutingKey { get; }
    public abstract IDictionary<string, object?>? Arguments { get; }
}
