using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using Serilog.Enrichers;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Sinks.Http.TextFormatters;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DemoApp2
{
    public partial class Form1 : Form
    {
        private TracerProvider _tracerProvider;
        private MetricServer _metricServer;
        private const string serviceName = "WinFormsApp2";
        private const string activitySourceName = "WinFormsApp2Tracer";
        private static readonly ActivitySource ActivitySource = new(activitySourceName);
        private static readonly Random _random = new();
        private string lokiEndpoint = "http://10.151.2.232:3100";
        private string tempoEndpoint = "http://10.151.2.232:4318/v1/traces";
        private static readonly Counter LogCounter = Metrics.CreateCounter(
            "app_log_total",
            "Tổng số log đã được ghi",
            new CounterConfiguration
            {
                LabelNames = new[] { "level" }
            });

        public Form1()
        {
            InitializeComponent();

            // 1. Init OpenTelemetry - Send traces to Alloy
            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                 .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                 .AddSource(activitySourceName)
                 .SetSampler(new AlwaysOnSampler())
                 .AddOtlpExporter(opt =>
                 {
                     opt.Endpoint = new Uri("http://localhost:4318"); // Gửi trace tới Alloy
                     opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                     opt.Headers = "Content-Type=application/x-protobuf"; // Ensure proper headers
                 })
                 .AddConsoleExporter() // Optional: hiển thị trace ra console
                 .Build();

            // 2. Init Serilog - Write to files for Promtail to collect
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.With<ActivityIdEnricher>()
                .Enrich.With<TraceContextEnricher>()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    formatter: new CustomJsonFormatter(),
                    path: @"C:\Users\admin\OneDrive\Máy tính\trace-logs\monitoring\logs\app2-.log",
                    rollingInterval: RollingInterval.Day,
                    shared: true)
                .WriteTo.Console() // Optional: xem log tại terminal
                .CreateLogger();

            // 3. Start MetricServer - Prometheus will scrape this port
            _metricServer = new MetricServer(port: 1235);
            _metricServer.Start();

            // 4. Test log
            Log.Information("App initialized with service name: {ServiceName}", serviceName);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.Information("App shutting down");

            Thread.Sleep(100);
            Log.CloseAndFlush();
            _tracerProvider?.Dispose();
        }

        private void btnGenerateLog_Click(object sender, EventArgs e)
        {
            using var activity = ActivitySource.StartActivity("generate_log", ActivityKind.Internal);
            var rnd = new Random();
            var logType = _random.Next(2); // 0 hoặc 1

            if (logType == 0)
            {
                Log.Information("Generated INFO log from button. traceId={TraceId} spanId={SpanId}",
                        Activity.Current?.TraceId.ToHexString(),
                        Activity.Current?.SpanId.ToHexString());
                LogCounter.WithLabels("info").Inc();
            }
            else
            {
                Log.Error("Generated ERROR log from button. traceId={TraceId} spanId={SpanId}",
                        Activity.Current?.TraceId.ToHexString(),
                        Activity.Current?.SpanId.ToHexString());
                LogCounter.WithLabels("error").Inc();
            }
        }
    }
}

