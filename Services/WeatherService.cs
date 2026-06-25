using Microsoft.EntityFrameworkCore;
using WeatherApi.Data;
using WeatherApi.Infrastructure.HttpClients;
using WeatherApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace WeatherApi.Services;

public class WeatherService : IWeatherService
{
    private readonly IWeatherApiClient _apiClient;
    private readonly AppDbContext _db;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        IWeatherApiClient apiClient,
        AppDbContext db,
        ILogger<WeatherService> logger)
    {
        _apiClient = apiClient;
        _db = db;
        _logger = logger;
    }

    private static string ComputeHash(string json)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(json);

        var root = doc.RootElement;

        var hourly = root.GetProperty("hourly");
        var temps = hourly.GetProperty("temperature_2m");
        var times = hourly.GetProperty("time");

        var normalized = new StringBuilder();

        normalized.Append("temp:");
        foreach (var t in temps.EnumerateArray())
            normalized.Append(t.GetDecimal()).Append(",");

        normalized.Append("|time:");
        foreach (var tm in times.EnumerateArray())
            normalized.Append(tm.GetString()).Append(",");

        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(normalized.ToString()));
        return Convert.ToHexString(bytes);
    }

    public async Task<string?> GetWeatherAsync(CancellationToken cancellationToken)
    {
        try
        {
            var freshData = await _apiClient.GetWeatherFromApiAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(freshData))
                return await GetCachedData(cancellationToken);

            var newHash = ComputeHash(freshData);

            var exists = await _db.WeatherRecords
                .AsNoTracking()
                .AnyAsync(x => x.Hash == newHash, cancellationToken);

            if (!exists)
            {
                _db.WeatherRecords.Add(new WeatherRecord
                {
                    RawJson = freshData,
                    Hash = newHash,
                    CreatedAt = DateTime.UtcNow
                });

                await _db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("New weather data saved.");
            }
            else
            {
                _logger.LogInformation("Duplicate weather data ignored.");
            }

            return freshData;
        }
        catch
        {
            return await GetCachedData(cancellationToken);

    }
    private async Task<string?> GetCachedData(CancellationToken cancellationToken)
    {
        var cachedData = await _db.WeatherRecords
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (cachedData != null)
        {
            _logger.LogWarning("Returning cached weather data.");
            return cachedData.RawJson;
        }

        _logger.LogWarning("No cache available.");
        return null;
    }
}