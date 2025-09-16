using DnDSpellBook.Infrastructure.Identity;
using DnDSpellBook.Web.Infrastructure;

namespace DnDSpellBook.Web.Endpoints;

public class Auth : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapIdentityApi<ApplicationUser>();
    }
}
