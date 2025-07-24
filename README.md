```
{
  "resourceSpans": [
    {
      "resource": {
        "attributes": [
          { "key": "service.name", "value": { "stringValue": "WinFormsApp" } }
        ]
      },
      "scopeSpans": [
        {
          "scope": {
            "name": "WinFormsAppTracer"
          },
          "spans": [
            {
              "traceId": "32aa1ce10da3c6b82b8992972df36f17",
              "spanId": "e806ebb6abb7bb0e",
              "name": "start_app",
              "startTimeUnixNano": 1753273946384000000,
              "endTimeUnixNano": 1753273946393000000,
              "kind": "SPAN_KIND_INTERNAL",
              "status": {
                "code": "STATUS_CODE_OK"
              },
              "attributes": [],
              "events": [],
              "links": []
            }
          ]
        }
      ]
    }
  ]
}
```
netsh http add urlacl url=http://+:1234/ user="TỉnhNguyễn"

netsh http delete urlacl url=http://+:1234/