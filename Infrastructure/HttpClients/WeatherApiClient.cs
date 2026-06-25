namespace WeatherApi.Infrastructure.HttpClients;

public class WeatherApiClient : IWeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WeatherApiClient> _logger;

    public WeatherApiClient(HttpClient httpClient,IConfiguration configuration, ILogger<WeatherApiClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string?> GetWeatherFromApiAsync(CancellationToken cancellationToken)
    {
        var endpoint =_configuration["WeatherApi:ForecastEndpoint"];

        var response =await _httpClient.GetAsync(endpoint,cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Weather API request failed. StatusCode: {StatusCode}",response.StatusCode);

            response.EnsureSuccessStatusCode();
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}