using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
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
        client.BaseAddress = new Uri(builder.Configuration["WeatherApi:BaseUrl"]!);
    })
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(4.8)))
    .AddPolicyHandler(
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TaskCanceledException>()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(100))
    );

builder.Services.AddScoped<IWeatherService, WeatherService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();