using UserWebAPI.Models;
using UserWebAPI.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Prometheus;
using UserWebAPI.Telemetry;
using UserWebAPI.Mappers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOpenTelemetry().WithMetrics(metrics =>
{
    metrics.AddAspNetCoreInstrumentation();
    metrics.AddHttpClientInstrumentation();
    metrics.AddRuntimeInstrumentation();
    metrics.AddMeter(UserMetrics.UserMeter.Name);
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
builder.Services.Configure<UsersDatabaseSettings>(builder.Configuration.GetSection("UserDatabase"));
builder.Services.AddSingleton<UsersService>();
builder.Services.AddScoped<IUserMapper, UserMapper>();
builder.WebHost.UseUrls("http://+:5220");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapPrometheusScrapingEndpoint();

app.UseMiddleware<UserWebAPI.Telemetry.MetricsMiddleware>();

app.MapControllers();

app.Run();