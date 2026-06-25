using Microsoft.EntityFrameworkCore;
using Polly;
using WeatherApi.Data;
using WeatherApi.Infrastructure.HttpClients;
using WeatherApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services
    .AddHttpClient<IWeatherApiClient, WeatherApiClient>(client =>
    {
        client.BaseAddress =
            new Uri(builder.Configuration["WeatherApi:BaseUrl"]!);

        client.Timeout = TimeSpan.FromSeconds(2);
    })
    .AddPolicyHandler(
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                3,
                _ => TimeSpan.FromMilliseconds(100)));

builder.Services.AddScoped<IWeatherService, WeatherService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();