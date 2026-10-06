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


## Batching and bounded memory

Each collector uses the same pipeline:

```text
OTLP receiver -> memory limiter -> batch processor -> signal exporter
```

The limiter checks memory every second, has a 512 MiB hard limit and a 128 MiB
spike allowance, and starts refusing data at the 384 MiB soft limit. Clients can
retry refused data. Each container has a 768 MiB memory limit to provide runtime
headroom. Batches flush after one second or 1,024 items, with a 2,048-item cap.
These are initial bounds, not a guarantee that every overload can be absorbed.
Exporter queues and batches are in memory; abrupt collector loss can lose data
already acknowledged to clients. The HA topology provides continued collection,
not durable or exactly-once delivery.


## Tempo retention

Tempo 3.1's backend scheduler and worker use 48-hour block retention. Expired
traces are removed from `tempo_data`; persistence across restarts does not
override expiry. Keep backups if traces must survive beyond the retention window.


## Mimir recording rules

Mimir's `local` ruler-storage backend reads the mounted Prometheus-format file
`mimir/rules/anonymous/metaspesa.yaml`. `anonymous` is the default tenant when
multitenancy is disabled. No rule-upload service is needed. The local backend
does not support rule creation/deletion through the configuration API; edit rules
in Git and restart Mimir to apply changes deterministically:

```sh
docker compose restart mimir
```

Validate expressions with `docker compose -f compose.tools.yaml run --rm
rules-validation`. After at least two HTTP metric observations, allow up to two
further minutes for the one-minute evaluation delay and evaluation interval.
Check all three recorded series in Grafana Explore.

The rules evaluate every minute and record five-minute HTTP request rate, error
ratio, and p95 duration. Grafana's overview dashboard and existing error alert
consume those series. Mimir explicitly enables OTLP metric unit/type suffixes
and promotes only the `service.name` resource attribute to `service_name` for
service-level metric grouping. Query evaluated rules through
`http://mimir:8080/prometheus/api/v1/rules` and recorded series through
`http://mimir:8080/prometheus/api/v1/query`, from the Docker network. Grafana Explore supplies this access
without host port publication. Rate series need at least two observations.
