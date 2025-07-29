using Newtonsoft.Json.Linq;
using Serilog.Events;
using Serilog.Formatting;
using System.Diagnostics;

namespace DemoApp2
{
    public class CustomJsonFormatter : ITextFormatter
    {
        public void Format(LogEvent logEvent, TextWriter output)
        {
            var json = new JObject
            {
                ["timestamp"] = logEvent.Timestamp.ToString("O"), // Thay @t bằng timestamp
                ["message"] = logEvent.MessageTemplate.Render(logEvent.Properties), // Thay @mt bằng message
                ["level"] = logEvent.Level.ToString().ToLowerInvariant(),
                ["traceId"] = logEvent.Properties.TryGetValue("TraceId", out var traceIdProp)
                    ? traceIdProp.ToString().Trim('"')
                    : (Activity.Current?.TraceId.ToString() ?? null),
                ["spanId"] = logEvent.Properties.TryGetValue("SpanId", out var spanIdProp)
                    ? spanIdProp.ToString().Trim('"')
                    : (Activity.Current?.SpanId.ToString() ?? null),
                ["activityId"] = logEvent.Properties.TryGetValue("ActivityId", out var activityIdProp)
                    ? activityIdProp.ToString().Trim('"')
                    : (Activity.Current?.Id ?? null)
            };

            // Thêm exception nếu có
            if (logEvent.Exception != null)
            {
                json["exception"] = logEvent.Exception.ToString();
            }

            output.WriteLine(json.ToString(Newtonsoft.Json.Formatting.None));
        }
    }
}
