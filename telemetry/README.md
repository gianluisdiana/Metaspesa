# Metaspesa telemetry integration

These consumer-owned adapters keep the application and LGTM as separate Compose
projects. The default root `docker compose up` runs only the application and
does not require collector credentials or a Grafana administrator password.

## Enable telemetry

Prepare the root `.env` and application secrets as usual. Clone the observability
repository separately and prepare its `.env` and certificates using its README. Preserve existing
certificates; Compose creates its own project-scoped storage. On upgrades, follow
the ownership instructions there before starting the independent stack.

Set `OBSERVABILITY_SECRETS_DIR` in the root `.env` to the separate checkout's
secrets directory, preferably as an absolute path. From this repository's root,
select the observability checkout and set an absolute asset path. In Bash:

```sh
observabilityDir="/absolute/path/to/observability"
export METASPESA_TELEMETRY_DIR="$PWD/telemetry"
```

The following Docker commands work in either shell after that setup. Keep these
settings in the same shell for startup, shutdown and assets validation:

```sh
docker compose --env-file .env --env-file "${observabilityDir}/.env" -f "${observabilityDir}/compose.yaml" -f telemetry/compose.observability.yaml up -d
docker compose -f compose.yaml -f telemetry/compose.yaml up --build -d
```

The first command starts project `observability` and creates its shared network.
The second starts project `metaspesa`, consuming that network as external.
There is no cross-project `depends_on`, automatic migration, or automatic LGTM
startup. The collectors/backend endpoints remain private Docker-network traffic.

- `compose.yaml`: optional application export, TLS client secrets, network and
  Grafana database-user initialization.
- `compose.observability.yaml`: optional Grafana assets/PostgreSQL settings and rules.

The assets adapter attaches two dashboards, their provider, the HTTP error-rate
alert, PostgreSQL datasource, and Mimir HTTP rules. Existing contents/UIDs are
unchanged. It is merged into the observability project, never the application
project; it contains no platform image tags. Standalone observability can run
without this adapter.

`OBSERVABILITY_NETWORK` must match in both projects (default
`observability_telemetry`). `TELEMETRY_SECRETS_GID` must also match if customized.
Application client certificate paths use `OBSERVABILITY_SECRETS_DIR`, relative
to root Compose unless absolute. Asset paths use `METASPESA_TELEMETRY_DIR`,
relative to the observability Compose directory unless absolute. The setup above
overrides the old `../telemetry` default with this repository's absolute asset
path, so the checkouts can live anywhere without copying assets or private keys.

The assets command loads root settings first and observability settings second.
Keep application-only `POSTGRES_DB`, `GRAFANA_DB_PASSWORD` and asset-path values
out of the observability checkout's `.env`, especially stale empty values from old templates.
The PostgreSQL datasource uses `db:5432` on the shared network, the read-only
`grafana` role, and root DB settings. Database roles/migrations remain application
owned. The export adapter attaches `db` only for this consumer datasource.

The Grafana initialization script now lives at `telemetry/db/init/01-grafana-user.sh`.
Only the telemetry override mounts it into `/docker-entrypoint-initdb.d`, using
`./telemetry/db/init` relative to the root Compose file. Plain application startup
does not create the `grafana` role. Set `GRAFANA_DB_PASSWORD` in the root `.env`
before initializing a new database with telemetry enabled.

PostgreSQL runs initialization scripts only for an empty data directory. Enabling
telemetry on an existing `db_data` volume does not run this script automatically.
Keep the volume and provision access explicitly as the database administrator:

- Create the `grafana` role if missing, using the password from the root `.env`.
  The initialization script can create it and set default privileges, but its
  `CREATE USER` statement fails if the role already exists.
- Grant database connection access and configure default privileges as shown in
  the script, using the same role that runs application migrations.
- For an already migrated database, also grant access to existing schemas and
  tables; default privileges only apply to objects created afterward:

  ```sql
  GRANT USAGE ON SCHEMA identity, market, purchasing, shopping TO grafana;
  GRANT SELECT ON ALL TABLES IN SCHEMA identity, market, purchasing, shopping TO grafana;
  ```

## Independent shutdown and opting out

```sh
docker compose -f compose.yaml -f telemetry/compose.yaml down
docker compose --env-file .env --env-file "${observabilityDir}/.env" -f "${observabilityDir}/compose.yaml" -f telemetry/compose.observability.yaml down
```

Either project can stop without stopping the other. An attached consumer may keep
the shared network in use; its owner can remove it only after consumers detach.
Each project owns its own data volumes; ordinary shutdown preserves them.
Do not use `--volumes` for ordinary shutdown: application data is still managed
by the application project.

Switch a running application back to its telemetry-free model:

```sh
docker compose up --build -d
```

Compose recreates changed application containers, removes their telemetry secret
mounts/network attachments and sets an empty exporter endpoint, even if a legacy
root `.env` contains one. The server/migration runner register no OTLP exporter;
client and scraper skip exporter startup. Observability continues independently.
Application migrations use the same selected files, for example:

```sh
docker compose -f compose.yaml -f telemetry/compose.yaml run --rm migrations
```

## Rules and validation

`mimir/rules/anonymous/metaspesa.yaml` records five-minute HTTP request rate,
error ratio and p95 duration every minute. Rate rules need two HTTP observations;
allow up to two further minutes for Mimir's delay and rule interval.

```sh
docker run --rm --entrypoint /bin/promtool --volume "${PWD}/telemetry/mimir/rules:/rules:ro" prom/prometheus:v3.15.0 check rules /rules/anonymous/metaspesa.yaml
docker compose --env-file .env.example config --quiet --no-env-resolution
docker compose --env-file .env.example -f compose.yaml -f telemetry/compose.yaml config --quiet --no-env-resolution
docker compose --env-file .env.example --env-file "${observabilityDir}/.env.example" -f "${observabilityDir}/compose.yaml" -f telemetry/compose.observability.yaml config --quiet --no-env-resolution
```

The pinned Prometheus image supplies
[`promtool check rules`](https://prometheus.io/docs/prometheus/latest/command-line/promtool/#promtool-check-rules)
without requiring an observability checkout, running collector, or secrets.
The two application Compose checks also run independently. The merged assets
check requires the separate checkout and the path setup above; it checks the
integration model, not live connectivity or certificate validity.

The `telemetry-validation` job in `.github/workflows/ci.yml` validates the two
application Compose models and Metaspesa recording rules with the independent
commands above. Platform configuration and certificate lifecycle checks belong
in the observability repository's CI.

Restart Mimir with the assets command's files after editing its local rules.
Removing asset mounts alone is not cleanup of previously
stored Grafana resources; use fresh storage or plan deliberate cleanup.
