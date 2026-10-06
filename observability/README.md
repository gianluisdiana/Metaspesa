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
`https://mimir:8080/prometheus/api/v1/rules` and recorded series through
`https://mimir:8080/prometheus/api/v1/query`, from the Docker network. Grafana Explore supplies this access
without host port publication. Rate series need at least two observations.


## Secure telemetry transport

Applications send OTLP/gRPC to Alloy over mutual TLS. Alloy exports to Loki,
Mimir and Tempo with server verification and client certificates; Grafana uses
mutual TLS for backend queries. All signals require a trusted client certificate.

Generate persistent, Git-ignored certificates before startup:

```sh
docker compose -f compose.tools.yaml run --rm certificates
docker network create metaspesa_telemetry_default
docker compose -f compose.observability.yaml up -d
docker compose up --build -d
```

Prepare .env and existing application secrets separately. The tooling command
preserves existing certificates and application credentials. On Linux, create
secrets/ with mode 700 and run the certificate service with
--user "$(id -u):$(id -g)"; on Windows, restrict directory access using filesystem
permissions. Leaf keys are readable by non-root container users; protect their
host directory. The CA private key is never mounted into runtime services.

Certificate files in ignored `secrets/` are:

```text
telemetry_ca.crt / telemetry_ca.key
telemetry_client.crt / telemetry_client.key
telemetry_alloy.crt / telemetry_alloy.key
telemetry_loki.crt / telemetry_loki.key
telemetry_mimir.crt / telemetry_mimir.key
telemetry_tempo.crt / telemetry_tempo.key
```

For HTTPS backend queries, provide the CA plus telemetry client certificate/key.
Grafana loads these from Docker secrets; clients verify the backend DNS names.

CA lifetime is ten years; client/server certificates last one year. Alloy's
certificate covers alloy; backend certificates cover their
Docker DNS names. Renew leaf certificates explicitly, preserving the CA:

```sh
docker compose -f compose.tools.yaml run --rm certificates renew
docker compose -f compose.observability.yaml up -d --force-recreate
docker compose up -d --force-recreate
```

Back up operational secrets. Replacing the CA requires reissuing certificates
and recreating all clients and servers together. TLS authentication grants access
to this telemetry stack, not per-user authorization. Missing client credentials,
untrusted certificates and server-only certificates used as clients are rejected.

Application REST/Grafana UI traffic retains existing HTTP behavior; production
still needs HTTPS ingress. Single-process backend internal RPC and collector
health endpoints remain private-network traffic. CI generates disposable
certificates and runs native validators using deployed Compose image versions.
Live delivery, certificate rejection and backend query checks are separate from
configuration validation.
