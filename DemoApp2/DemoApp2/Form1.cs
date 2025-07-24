using OpenTelemetry;
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
        private string lokiEndpoint = "http://192.168.1.112:3100";
        private string tempoEndpoint = "http://192.168.1.112:4318/v1/traces";
        private static readonly Counter LogCounter = Metrics.CreateCounter(
            "app2_log_total",
            "Tổng số log đã được ghi",
            new CounterConfiguration
            {
                LabelNames = new[] { "level", "action" }
            });

        public Form1()
        {
            InitializeComponent();

            // 1. Init OpenTelemetry trước tiên
            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddSource(activitySourceName) // phải trùng với ActivitySource
                .SetSampler(new AlwaysOnSampler())
                .AddProcessor(new SimpleActivityExportProcessor(new CustomJsonExporter(serviceName, activitySourceName, tempoEndpoint)))
                //.AddOtlpExporter(opt =>
                //{
                //    opt.Endpoint = new Uri("http://10.151.2.232:4318"); // Tempo OTLP HTTP endpoint
                //    opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                //})
                .AddConsoleExporter() // Debug ra terminal
                .Build();

            // 2. Init Serilog (sau khi tracer setup)
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.With<ActivityIdEnricher>()
                .Enrich.With<TraceContextEnricher>()
                .Enrich.FromLogContext()
                .WriteTo.GrafanaLoki(
                    lokiEndpoint,
                    labels: new List<LokiLabel>
                    {
                        new LokiLabel { Key = "app", Value = serviceName },
                        new LokiLabel { Key = "host", Value = Environment.MachineName },
                    },
                    propertiesAsLabels: new[] { "level", "traceId", "spanId" },
                    textFormatter: new CustomJsonFormatter()
                )
                .WriteTo.File("logs\\app.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _metricServer = new MetricServer(port: 1235);
            _metricServer.Start();

            // 3. Log thử nghiệm sau khi setup
            Log.Information("App initialized");
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
                LogCounter.WithLabels("info", "generate_log_success").Inc();
            }
            else
            {
                Log.Error("Generated ERROR log from button. traceId={TraceId} spanId={SpanId}",
                        Activity.Current?.TraceId.ToHexString(),
                        Activity.Current?.SpanId.ToHexString());
                LogCounter.WithLabels("error", "generate_log_fail").Inc();
            }
        }
    }
}

