using DnDSpellBook.Application.Common.Interfaces;
using MediatR.Pipeline;
using Serilog;

namespace DnDSpellBook.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest>(IUser user, IIdentityService identityService) : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = user.Id ?? string.Empty;
        var userName = string.Empty;

        if (!string.IsNullOrEmpty(userId))
        {
            userName = await identityService.GetUserNameAsync(userId);
        }

        Log.Information("Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, request);
    }
}
