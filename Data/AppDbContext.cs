using Microsoft.EntityFrameworkCore;
using WeatherApi.Models;


namespace WeatherApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<WeatherRecord> WeatherRecords => Set<WeatherRecord>();
}