using DnDSpellBook.Application.Common.Interfaces;
using MediatR;

namespace DnDSpellBook.Application.WeatherForecasts.Queries.GetWeatherForecasts;

public record GetWeatherForecastsQuery : IRequest<IEnumerable<WeatherForecast>>;

public class GetWeatherForecastsQueryHandler(IEmailService emailService)
    : IRequestHandler<GetWeatherForecastsQuery, IEnumerable<WeatherForecast>>
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public async Task<IEnumerable<WeatherForecast>> Handle(GetWeatherForecastsQuery request,
        CancellationToken cancellationToken)
    {
        var rng = new Random();

        await emailService.SendEmailAsync("test@test.com", "Test",
            "This is a test email from the WeatherForecasts service.");

        var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = rng.Next(-20, 55),
            Summary = Summaries[rng.Next(Summaries.Length)]
        });

        return await Task.FromResult(result);
    }
}
