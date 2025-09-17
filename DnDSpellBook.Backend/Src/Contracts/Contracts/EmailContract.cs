using DnDSpellBook.Contracts.Common;

namespace DnDSpellBook.Contracts;

public record EmailContract(string To, string Subject, string Body);

public class EmailContractSettings : BaseContractSettings
{
    public override string QueueName => "email_queue";
    public override bool Durable => true;
    public override bool Exclusive => false;
    public override bool AutoDelete => false;
    public override bool Passive => false;
    public override string Exchange => "notifications";
    public override string RoutingKey => "email.send";
    public override IDictionary<string, object?>? Arguments => null;
}
