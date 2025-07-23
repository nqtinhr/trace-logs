using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Trace;

public class CustomJsonExporter : BaseExporter<Activity>
{
    private readonly string _serviceName;
    private readonly string _sourceName;
    private readonly string _tempoEndpoint;

    public CustomJsonExporter(string serviceName = "WinFormsApp", string sourceName = "WinFormsAppTracer", string? tempoEndpoint = null)
    {
        _serviceName = serviceName;
        _sourceName = sourceName;
        _tempoEndpoint = tempoEndpoint ?? "http://192.168.1.112:4318/v1/traces";
    }

    public override ExportResult Export(in Batch<Activity> batch)
    {
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

        var resourceSpans = new[] { resourceObject };

        var payload = new { resourceSpans };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        Console.Write(json); // Xem trace gửi gì

        // Gửi HTTP POST đến Tempo
        try
        {
            using var client = new HttpClient();
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = client.PostAsync(_tempoEndpoint, content).Result;

            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Send trace fail. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                return ExportResult.Failure;
            }

            Console.WriteLine($"Send trace successfully: {spansList.Count} span(s). TraceId = {((Dictionary<string, object>)spansList[0])["traceId"]}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("❌ Lỗi khi gửi trace: " + ex.Message);
            return ExportResult.Failure;
        }

        return ExportResult.Success;
    }
}
