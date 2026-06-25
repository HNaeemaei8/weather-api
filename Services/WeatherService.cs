using Microsoft.EntityFrameworkCore;
using WeatherApi.Data;
using WeatherApi.Infrastructure.HttpClients;
using WeatherApi.Models;

namespace WeatherApi.Services;

public class WeatherService : IWeatherService
{
    private readonly IWeatherApiClient _apiClient;
    private readonly AppDbContext _db;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(IWeatherApiClient apiClient, AppDbContext db, ILogger<WeatherService> logger)
    {
        _apiClient = apiClient;
        _db = db;
        _logger = logger;
    }

    public async Task<string?> GetWeatherAsync(CancellationToken cancellationToken)
    {
        string? freshData = null;

        try
        {
            freshData = await _apiClient.GetWeatherFromApiAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Network error. Falling back to cache.");
            return await GetCachedData(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(freshData))
            return await GetCachedData(cancellationToken);

        try
        {
            await SaveWeatherData(freshData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error while saving. Returning fresh data without saving.");
        }

        return freshData;
    }

    private async Task SaveWeatherData(string data, CancellationToken cancellationToken)
    {
        var record = await _db.WeatherRecords.OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken);

        if (record == null)
        {
            _db.WeatherRecords.Add(new WeatherRecord { RawJson = data, CreatedAt = DateTime.UtcNow });
        }
        else
        {
            record.RawJson = data;
            record.CreatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<string?> GetCachedData(CancellationToken cancellationToken)
    {
        try
        {
            var cachedData = await _db.WeatherRecords
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return cachedData?.RawJson; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error while reading cache.");
            return null;
        }
    }
}