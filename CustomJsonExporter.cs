using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using OpenTelemetry;
using OpenTelemetry.Trace;

public class CustomJsonExporter : BaseExporter<Activity>
{
    private readonly string _serviceName;
    private readonly string _sourceName;

    public CustomJsonExporter(string serviceName = "WinFormsApp", string sourceName = "WinFormsAppTracer")
    {
        _serviceName = serviceName;
        _sourceName = sourceName;
    }

    public override ExportResult Export(in Batch<Activity> batch)
    {
        var resourceSpans = new List<object>();
        var spansList = new List<object>();

        foreach (var activity in batch)
        {
            var startTime = activity.StartTimeUtc;
            var endTime = startTime + activity.Duration;

            var span = new Dictionary<string, object?>
            {
                ["traceId"] = activity.TraceId.ToHexString(),
                ["spanId"] = activity.SpanId.ToHexString(),
                ["name"] = activity.DisplayName,
                ["startTimeUnixNano"] = startTime.Ticks * 100,
                ["endTimeUnixNano"] = endTime.Ticks * 100,
                ["kind"] = activity.Kind switch
                {
                    ActivityKind.Internal => "SPAN_KIND_INTERNAL",
                    ActivityKind.Server => "SPAN_KIND_SERVER",
                    ActivityKind.Client => "SPAN_KIND_CLIENT",
                    ActivityKind.Producer => "SPAN_KIND_PRODUCER",
                    ActivityKind.Consumer => "SPAN_KIND_CONSUMER",
                    _ => "SPAN_KIND_INTERNAL"
                },
                ["status"] = new
                {
                    code = activity.Status switch
                    {
                        ActivityStatusCode.Ok => "STATUS_CODE_OK",
                        ActivityStatusCode.Error => "STATUS_CODE_ERROR",
                        _ => "STATUS_CODE_UNSET"
                    }
                },
                ["attributes"] = activity.Tags.Select(tag => new
                {
                    key = tag.Key,
                    value = new { stringValue = tag.Value }
                }).ToList(),
                ["events"] = activity.Events.Select(e => new
                {
                    name = e.Name,
                    timeUnixNano = e.Timestamp.Ticks * 100,
                    attributes = e.Tags.Select(tag => new
                    {
                        key = tag.Key,
                        value = new { stringValue = tag.Value }
                    }).ToList()
                }).ToList(),
                ["links"] = activity.Links.Select(link => new
                {
                    traceId = link.Context.TraceId.ToHexString(),
                    spanId = link.Context.SpanId.ToHexString()
                }).ToList()
            };

            spansList.Add(span);
        }

        var resourceObject = new
        {
            resource = new
            {
                attributes = new[]
                {
                    new { key = "service.name", value = new { stringValue = _serviceName } }
                }
            },
            scopeSpans = new[]
            {
                new
                {
                    scope = new { name = _sourceName },
                    spans = spansList
                }
            }
        };

        resourceSpans.Add(resourceObject);

        var json = JsonSerializer.Serialize(new { resourceSpans }, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        if (!string.IsNullOrEmpty(_outputFilePath))
        {
            try
            {
                File.AppendAllText(_outputFilePath, json + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to write trace JSON to file: " + ex.Message);
                return ExportResult.Failure;
            }
        }
        else
        {
            Console.WriteLine(json);
        }

        return ExportResult.Success;
    }
}
