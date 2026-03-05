using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CarWebAPI.Telemetry
{
    public static class CarMetrics
    {
        public static readonly Meter CarMeter = new Meter("CarWebAPI", "1.0.0");

        private static readonly Stopwatch UptimeStopwatch = Stopwatch.StartNew();
        //public static readonly Counter<int> CarsCreatedCounter = CarMeter.CreateCounter<int>("cars_created", description: "Number of cars created");
        public static readonly ObservableGauge<int> Uptime = CarMeter.CreateObservableGauge<int>(
                               "uptime_seconds",
                               () => (int)UptimeStopwatch.Elapsed.TotalSeconds,
                               description: "Uptime of the application in seconds"
        );
        public static readonly Counter<int> RequestsPerSecond = CarMeter.CreateCounter<int>("requests_per_second", description: "Number of HTTP requests per second");
        public static readonly Histogram<double> Latency = CarMeter.CreateHistogram<double>("request_latency_second", description: "Latency of HTTP requests in seconds");
        public static readonly Counter<int> Errors = CarMeter.CreateCounter<int>("errors", description: "Number of errors");

        //public static readonly Histogram<double> RequestDurationHistogram = CarMeter.CreateHistogram<double>("request_duration_seconds", description: "Duration of HTTP requests in seconds");
    }
}
