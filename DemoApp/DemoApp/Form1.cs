using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;
using System.Diagnostics;

namespace DemoApp
{
    public partial class Form1 : Form
    {
        private bool _isAppStarted = false;
        private TracerProvider _tracerProvider;
        private MetricServer _metricServer;
        private const string serviceName = "WinFormsApp";
        private const string activitySourceName = "WinFormsAppTracer";
        private static readonly ActivitySource ActivitySource = new(activitySourceName);
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

            // 1. Init OpenTelemetry trước tiên
            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddSource(activitySourceName)
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

            _metricServer = new MetricServer(port: 1234);
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

        private void btnStartApp_Click(object sender, EventArgs e)
        {
            _isAppStarted = true;
            using var activity = ActivitySource.StartActivity("start_app", ActivityKind.Internal);
            Log.Information("StartApp button clicked. traceId={TraceId} spanId={SpanId}",
                Activity.Current?.TraceId.ToHexString(),
                Activity.Current?.SpanId.ToHexString());
            LogCounter.WithLabels("info").Inc();
        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            if (!_isAppStarted)
            {
                MessageBox.Show("Please start the app first.");
                return;
            }

            using var activity = ActivitySource.StartActivity("load_file", ActivityKind.Internal);

            try
            {
                string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                string filePath = Path.Combine(logsDir, "test.txt");

                var content = File.ReadAllText(filePath);

                Log.Information("Loaded file '{Path}', length={Length}, traceId={TraceId}, spanId={SpanId}",
                    filePath, content.Length,
                    Activity.Current?.TraceId.ToString(), Activity.Current?.SpanId.ToString());
                LogCounter.WithLabels("info").Inc();

                activity?.SetTag("file.name", "test.txt");
                activity?.SetTag("file.path", filePath);
                activity?.SetTag("file.length", content.Length);

                MessageBox.Show("File loaded successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load file. traceId={TraceId} spanId={SpanId}",
                    Activity.Current?.TraceId.ToString(), Activity.Current?.SpanId.ToString());
                LogCounter.WithLabels("error").Inc();

                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.SetTag("otel.status_description", ex.Message);

                MessageBox.Show("Error loading file.");
            }
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            if (!_isAppStarted)
            {
                MessageBox.Show("Please start the app first.");
                return;
            }

            using var activity = ActivitySource.StartActivity("save_file", ActivityKind.Internal);
            var sw = Stopwatch.StartNew();

            string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            string filePath = Path.Combine(logsDir, "test.txt");

            try
            {
                if (!Directory.Exists(logsDir))
                    Directory.CreateDirectory(logsDir);

                File.WriteAllText(filePath, "Hello from WinForms!");
                sw.Stop();

                Log.Information("Saved file at {Path} in {Ms}ms, traceId={TraceId}, spanId={SpanId}",
                    filePath, sw.ElapsedMilliseconds,
                    Activity.Current?.TraceId.ToString(), Activity.Current?.SpanId.ToString());
                LogCounter.WithLabels("info").Inc();

                activity?.SetTag("file.path", filePath);
                activity?.SetTag("duration_ms", sw.ElapsedMilliseconds);

                MessageBox.Show($"Đã lưu file: {filePath}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi khi lưu file. traceId={TraceId} spanId={SpanId}",
                    Activity.Current?.TraceId.ToString(), Activity.Current?.SpanId.ToString());
                LogCounter.WithLabels("error").Inc();

                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.SetTag("otel.status_description", ex.Message);

                MessageBox.Show("Lỗi khi lưu file.");
            }
        }

        private void btnSimulateError_Click(object sender, EventArgs e)
        {
            using var activity = ActivitySource.StartActivity("simulate_error", ActivityKind.Internal);

            try
            {
                throw new InvalidOperationException("Simulated test error");
            }
            catch (Exception ex)
            {
                // Đánh dấu trạng thái lỗi cho span (trace)
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                // Ghi log có chứa traceId / spanId
                Log.Error(ex,
                    "Simulated exception occurred. traceId={TraceId} spanId={SpanId}",
                    Activity.Current?.TraceId.ToHexString(),
                    Activity.Current?.SpanId.ToHexString());
                LogCounter.WithLabels("error").Inc();
            }
        }

        private void btnTraceOnly_Click(object sender, EventArgs e)
        {
            using var activity = ActivitySource.StartActivity("manual_trace_only", ActivityKind.Internal);

            activity?.SetTag("test.mode", "manual");
            activity?.SetTag("user.id", Environment.UserName);

            Log.Information("Trace-only span sent manually. traceId={TraceId} spanId={SpanId}",
                Activity.Current?.TraceId.ToString(), Activity.Current?.SpanId.ToString());
            LogCounter.WithLabels("info").Inc();
        }
    }
}
