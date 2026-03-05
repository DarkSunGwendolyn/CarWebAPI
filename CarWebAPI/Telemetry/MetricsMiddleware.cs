using System.Diagnostics;

namespace CarWebAPI.Telemetry
{
    public class MetricsMiddleware
    {
        private readonly RequestDelegate _next;
        public MetricsMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            CarMetrics.RequestsPerSecond.Add(1);
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                CarMetrics.Latency.Record(stopwatch.Elapsed.TotalSeconds);
                if(context.Response.StatusCode >= 400)
                {
                    CarMetrics.Errors.Add(1);
                }
            }
        }
    }
}
