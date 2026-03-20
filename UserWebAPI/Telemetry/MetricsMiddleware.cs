using System.Diagnostics;

namespace UserWebAPI.Telemetry
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
            UserMetrics.RequestsPerSecond.Add(1);
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                UserMetrics.Latency.Record(stopwatch.Elapsed.TotalSeconds);
                if (context.Response.StatusCode >= 400)
                {
                    UserMetrics.Errors.Add(1);
                }
            }
        }
    }
}