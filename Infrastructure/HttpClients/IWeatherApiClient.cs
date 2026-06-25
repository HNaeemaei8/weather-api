namespace WeatherApi.Infrastructure.HttpClients;

public interface IWeatherApiClient
{
    Task<string?> GetWeatherFromApiAsync(CancellationToken cancellationToken);
}