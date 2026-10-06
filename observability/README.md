# Observability

Phase 2: deployment files, environment template, certificates and platform
datasources live in this directory. Alloy, HAProxy, Loki, Mimir, Tempo and Grafana
can be started from here without application assets, credentials or a database.

Applications send OTLP/gRPC over mutual TLS to `alloy:4317`. HAProxy passes each
connection to one of two collectors. Collectors and Grafana use mutual TLS when
accessing the telemetry backends.

The application root Compose merges `observability/compose.yaml` with its own
`telemetry/compose.yaml` adapter. Application dashboards, alerts, rules and the
PostgreSQL datasource are owned by `telemetry/`, not this directory. This is not
yet a completed repository split: project/network ownership is still shared.

## Setup and startup

Install Docker with Compose 2.24+. Work from this directory:

```sh
cd observability
```

For a new installation, copy `.env.example` to `.env` and set a strong
`GRAFANA_PASSWORD`. For an existing installation, preserve the current Grafana
password and the existing CA/certificate pairs. Never replace operational values
with template placeholders. Root application startup continues to use its own
`.env`; standalone startup uses this directory's `.env`.

Telemetry certificates live in `observability/secrets/`. When upgrading, copy
only the twelve `telemetry_*.crt`/`telemetry_*.key` files from root `secrets/`,
without overwriting any existing destination files. Check matching pairs before
continuing. Application JWT and scraper credentials stay in root `secrets/`.
The existing files have been copied locally for this workspace; originals remain.

```sh
docker compose -f compose.tools.yaml run --rm certificates
docker compose up -d
```

The tooling command generates only missing certificates and checks expiry and
trust for existing pairs. It requires neither host OpenSSL nor Python. Tooling
services are not included in normal startup. Grafana remains available at
http://localhost:3001; collector/backend endpoints remain private-network only.

Run the certificate service as its default root user so it can assign file
groups. It enforces mode 700 on the secrets directory, 600 on the CA key, 640 on
leaf keys, and 644 on certificates, including reused pairs. Leaf keys use the
dedicated numeric group `TELEMETRY_SECRETS_GID` (default 1999); every service
mounting a leaf key receives that supplementary group. If overridden, use the
same value for tooling and both runtime Compose files. Do not grant this group
to unrelated containers/users. On Windows, also restrict host directory ACLs:
Linux container modes do not guarantee Windows host ACL protection. Existing
certificate/key public keys must match; restore a matching pair from backup if
validation fails.

## Existing deployment identity and data

Defaults preserve the current project (`metaspesa`), network
(`metaspesa_telemetry`) and the four existing `metaspesa_*_data` telemetry volumes.
Volume/network names are explicit so moving this directory does not silently
select empty storage. The example environment exposes those names for a later
ownership migration or a genuinely new installation.

Do not run a second deployment against the same volumes concurrently. Changing
project, network or volume names is a separate migration: inspect existing Docker
resources, stop the old deployment and back up data before switching ownership.
Do not use `down --volumes` or `--remove-orphans` during this incremental split;
the project is still shared with the application and data must be preserved.

From the repository root, full application startup remains `docker compose up`.
Application migrations remain opt-in. From this directory, `docker compose up`
starts telemetry services only; it does not start the application database.

## Certificates and transport boundary

Local certificate files are ignored by Git:

```text
telemetry_ca.crt / telemetry_ca.key
telemetry_client.crt / telemetry_client.key
telemetry_alloy.crt / telemetry_alloy.key
telemetry_loki.crt / telemetry_loki.key
telemetry_mimir.crt / telemetry_mimir.key
telemetry_tempo.crt / telemetry_tempo.key
```

The CA private key is used only for issuance and is never mounted in runtime
services. The Alloy certificate covers `alloy`, `alloy-1` and `alloy-2`; backend
certificates cover their Docker DNS names. Leaf certificates last one year and
the CA lasts ten years. Renewal preserves the CA and leaf private keys and
atomically replaces each certificate; interruption leaves either the old or new
matching pair. This renews expiry, not key rotation. Compromised keys require
deliberate replacement/reissuance; stop affected services before replacing pairs
for key rotation. Renew leaves explicitly from this directory:

```sh
docker compose -f compose.tools.yaml run --rm certificates renew
docker compose up -d --force-recreate
```

Renewal preserves the CA. After renewing, also recreate application containers
from the repository root so their bind-mounted client credentials are refreshed.
Back up operational secrets. CA replacement requires reissuing certificates and
recreating every client and server together; never mix trust roots. Compose file
secrets are bind mounts, so leaf keys must be readable by non-root users; protect
the host directory. Certificates authenticate membership of this telemetry stack,
not individual application users.

Grafana provisioning reads the mounted secret files directly with its file
provider; no PEM credentials are exported into the process environment.

Application REST and Grafana UI retain existing HTTP behavior; public deployment
still needs HTTPS ingress. Single-process backend RPC and Alloy health/metrics
remain private-network traffic.

## Application integration boundary

A fresh standalone Grafana provisions only Loki, Mimir and Tempo. It requires
no PostgreSQL settings and provisions no application dashboards or alerts.
Mimir's local rule directory is empty by default. Consumer-owned adapters can
mount dashboards, alerts, extra datasource files and a tenant-organized rules
directory without duplicating the platform datasource definitions or image tags.

Metaspesa's adapter and its instructions are in `../telemetry/`; they are not
required to run this directory independently. Database roles/grants remain owned
by the application, not this stack. Removing a mount is not a cleanup strategy
for previously stored Grafana resources: use fresh isolated storage for a clean
standalone installation, or plan deliberate cleanup of existing resources.

Next slices will make application telemetry opt-in and establish separate Compose
project/network ownership. This slice does not change processing, retention,
existing resource identities, datasource/dashboard UIDs or alert UIDs.

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

Mimir's `local` ruler-storage backend reads Prometheus-format files from the
mounted `mimir/rules/` directory. Put files under the tenant's subdirectory;
`anonymous` is the default tenant when multitenancy is disabled. The directory
ships empty. Application adapters can replace this mount with their own rules.
No rule-upload service is needed. The local backend does not support creation or
deletion through the configuration API; edit rules in Git and restart Mimir:

```sh
docker compose restart mimir
```

Validate an installed rule file with the tooling service:

```sh
docker compose -f compose.tools.yaml run --rm rules-validation check rules /rules/anonymous/example.yaml
```

Mimir explicitly enables OTLP metric unit/type suffixes and promotes only the
`service.name` resource attribute to `service_name` for service-level grouping.
Query evaluated rules through
`https://mimir:8080/prometheus/api/v1/rules` and recorded series through
`https://mimir:8080/prometheus/api/v1/query`, from the Docker network with the
telemetry CA and client certificate/key. Grafana Explore supplies this access
without host port publication. Rate series need at least two observations.

## Collector availability

HAProxy passes TLS through and assigns each TCP connection to exactly one healthy
collector. Its checks use Alloy's internal `/-/healthy` endpoint, which checks
component health rather than only whether initial configuration loaded. When a
collector fails, clients reconnect and retry observed failures against the other
collector. No collector label is added and telemetry is not broadcast, avoiding
duplicate metric series. Network retries can still repeat a sample; this is not
an exactly-once delivery guarantee.

Both collectors share one host in local Compose. HAProxy and the backends remain
single instances, so this protects against collector failure, not host failure
or failure of every component. It also uses more memory than one collector.

Tail sampling is disabled. If introduced, route spans through a load-balancing
Alloy gateway with `otelcol.exporter.loadbalancing` and
`routing_key = "traceID"` before the sampling tier. TCP connection balancing or
Alloy clustering alone does not keep every span of a trace on the same sampler.

## Validation and CI

CI runs native commands explicitly, using Compose services to select the actual
deployed images, configuration mounts, and secrets. No Python wrapper or repeated
image tags are needed. After generating certificates, run the same checks locally:

```sh
docker compose --env-file .env.example config --quiet --no-env-resolution
docker compose -f compose.tools.yaml config --quiet
docker compose --env-file .env.example run --rm --no-deps alloy-1 validate /etc/alloy/config.alloy
docker compose --env-file .env.example run --rm --no-deps alloy haproxy -c -f /usr/local/etc/haproxy/haproxy.cfg
docker compose --env-file .env.example run --rm --no-deps loki '-config.file=/etc/loki/loki.yaml' -verify-config
docker compose --env-file .env.example run --rm --no-deps mimir '-config.file=/etc/mimir/mimir.yaml' -modules
docker compose --env-file .env.example run --rm --no-deps tempo '-config.file=/etc/tempo/tempo.yaml' '-config.verify=true'
```

All validation commands above run from this directory. The separate application
CI job validates its adapter and recording rules; see `../telemetry/README.md`.

The separate tooling Compose file uses ordinary service `image` declarations
so the repository's Docker Compose Dependabot configuration can track them.
CI generates disposable runner-local certificates before checking configuration.
Certificate lifecycle regression tests use disposable container-local files,
never the mounted operational secrets:

```sh
docker compose -f compose.tools.yaml run --rm --entrypoint /bin/sh certificates /tools/test-certificates.sh
```

CI runs these tests for permissions, reused credentials, missing/mismatched
pairs (including the CA), interrupted renewal, and successful renewal.

CI also starts a fresh standalone Grafana and checks that only the three platform
datasources are provisioned, with no application dashboards or alerts. Native
validators do not establish live delivery or failover: after startup,
verify all three signals in Grafana, stop one collector, and confirm collection
continues through the remaining instance. Mimir delays rule evaluation by one
minute by default; account for that delay and each group's evaluation interval.
