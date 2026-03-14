using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace UserWebAPI.Telemetry
{
    public static class UserMetrics
    {
        public static readonly Meter UserMeter = new Meter("UserWebAPI", "1.0.0");

        private static readonly Stopwatch UptimeStopwatch = Stopwatch.StartNew();
        public static readonly ObservableGauge<int> Uptime = UserMeter.CreateObservableGauge<int>(
                               "uptime_seconds",
                               () => (int)UptimeStopwatch.Elapsed.TotalSeconds,
                               description: "Uptime of the application in seconds"
        );

        
        public static readonly Counter<int> RequestsPerSecond = UserMeter.CreateCounter<int>(
            "requests_per_second",
            description: "Number of HTTP requests per second"
        );

        
        public static readonly Histogram<double> Latency = UserMeter.CreateHistogram<double>(
            "request_latency_second",
            description: "Latency of HTTP requests in seconds"
        );

        
        public static readonly Counter<int> Errors = UserMeter.CreateCounter<int>(
            "errors",
            description: "Number of errors"
        );

        public static readonly Counter<int> UsersAdded = UserMeter.CreateCounter<int>(
            "users_added_total",
            description: "Total number of users added"
        );
    }
}