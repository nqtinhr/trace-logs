using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp
{
    public class TraceContextEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var activity = Activity.Current;

            if (activity != null)
            {
                string traceId = activity.TraceId.ToHexString();
                string spanId = activity.SpanId.ToHexString();
                string activityId = activity.Id ?? "";

                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("traceId", traceId));
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("spanId", spanId));
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("activityId", activityId));

                Debug.WriteLine($"[TraceContextEnricher] OK - traceId={traceId}, spanId={spanId}, activityId={activityId}");
            }
            else
            {
                Debug.WriteLine($"[TraceContextEnricher] FAIL - No Activity.Current - Log msg: {logEvent.MessageTemplate}");
            }
        }

    }

}
