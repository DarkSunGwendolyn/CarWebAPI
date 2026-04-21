using CarWebAPI.Mappers;
using CarWebAPI.Messaging;
using CarWebAPI.Models;
using CarWebAPI.Services;
using Confluent.Kafka;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});
builder.Services.AddAuthorization();
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
builder.Services.AddSingleton<KafkaRequestProducer>();
builder.Services.AddHostedService<KafkaResponseConsumer>();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapPrometheusScrapingEndpoint();

app.MapControllers();

app.Run();
