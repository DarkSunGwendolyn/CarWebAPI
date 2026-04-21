using UserWebAPI.Models;
using UserWebAPI.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Prometheus;
using UserWebAPI.Telemetry;
using UserWebAPI.Mappers;
using Microsoft.Extensions.Caching.Distributed;
using UserWebAPI.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
builder.Services.Configure<UsersDatabaseSettings>(builder.Configuration.GetSection("UsersDatabase"));
builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<KafkaResponseProducer>();
builder.Services.AddHostedService<KafkaRequestConsumer>();
builder.Services.AddScoped<IUserMapper, UserMapper>();
builder.WebHost.UseUrls("http://+:5220");
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration["RedisCacheOptions:Configuration"];
    options.InstanceName = builder.Configuration["RedisCacheOptions:InstanceName"];
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapPrometheusScrapingEndpoint();

//app.UseMiddleware<UserWebAPI.Telemetry.MetricsMiddleware>();

app.MapControllers();

app.Run();