using DnDSpellBook.Application.WeatherForecasts.Queries.GetWeatherForecasts;
using DnDSpellBook.Web.Infrastructure;
using MediatR;

namespace DnDSpellBook.Web.Endpoints;

public class WeatherForecasts : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetWeatherForecasts);
    }

    private async Task<IEnumerable<WeatherForecast>> GetWeatherForecasts(ISender sender)
    {
        return await sender.Send(new GetWeatherForecastsQuery());
    }
}
