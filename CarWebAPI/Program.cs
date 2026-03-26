using CarWebAPI.Mappers;
using CarWebAPI.Models;
using CarWebAPI.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Caching.Distributed;
using Prometheus;
using Microsoft.AspNetCore.Connections;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOpenTelemetry().WithMetrics(metrics =>
{
    metrics.AddAspNetCoreInstrumentation();
    metrics.AddHttpClientInstrumentation();
    metrics.AddRuntimeInstrumentation();
    metrics.AddMeter(CarWebAPI.Telemetry.CarMetrics.CarMeter.Name);
    metrics.AddPrometheusExporter();
}).WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
});
builder.Services.AddControllers().AddJsonOptions(
    options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<CarSelectionDatabaseSettings>(builder.Configuration.GetSection("CarSelectionDatabase"));
builder.Services.AddSingleton<CarsService>();
builder.Services.AddScoped<ICarMapper, CarMapper>();
builder.WebHost.UseUrls("http://+:5219");
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration["RedisCacheOptions:Configuration"];
    options.InstanceName = builder.Configuration["RedisCacheOptions:InstanceName"];
});
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = "redis:6379";
//    options.InstanceName = "CarWebAPI_";
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapPrometheusScrapingEndpoint();

app.MapControllers();

app.Run();
