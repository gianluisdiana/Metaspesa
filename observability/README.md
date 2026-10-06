# Observability

## Loki native OTLP label policy

Loki receives native OTLP at `/otlp`. Its explicit allowlist indexes only
`service.name`, normalized to `service_name`. Other attributes remain structured
metadata; `trace_id`, `span_id`, and `user_id` do not create index streams.
Filter by service first and metadata second:

```logql
{service_name="metaspesa_rest_api"} | trace_id="0123456789abcdef0123456789abcdef"
```

Grafana's Tempo data source uses the same service-name mapping and trace/span
filters for trace-to-log navigation.
