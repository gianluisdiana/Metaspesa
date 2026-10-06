# Metaspesa telemetry assets

This directory belongs to the application, not to the reusable LGTM deployment.
It owns the two Grafana dashboards, their provider, the HTTP error-rate alert,
PostgreSQL datasource, and Mimir HTTP recording rules. Their contents and UIDs
are unchanged by this move.

The repository root Compose includes a merge of:

- `observability/compose.yaml`: platform services, TLS, storage and LGTM datasources.
- `telemetry/compose.yaml`: application assets and PostgreSQL settings only.

The adapter is not another environment or a separate stack. It has no image tags
and is not runnable alone. Its bind paths are relative to `observability/`, the
first included file. Normal startup remains `docker compose up` from the root;
database migrations remain opt-in.

The PostgreSQL datasource uses `db:5432`, the read-only `grafana` role, and the
root `.env` values `POSTGRES_DB` and `GRAFANA_DB_PASSWORD`. Database initialization
and migrations remain application-owned. These values are not required by the
standalone observability stack.

## Recording rules

`mimir/rules/anonymous/metaspesa.yaml` records five-minute HTTP request rate,
error ratio and p95 duration every minute. The overview dashboard and error alert
consume these series. Rate rules need at least two HTTP metric observations;
allow up to two further minutes for Mimir's evaluation delay and the rule interval.

Validate from the repository root using the Prometheus version in tooling Compose:

```sh
docker compose -f observability/compose.tools.yaml run --rm --volume "${PWD}/telemetry/mimir/rules:/rules:ro" rules-validation check rules /rules/anonymous/metaspesa.yaml
```

The volume argument selects application-owned files without making the platform
tooling depend on this directory. After editing rules, apply them from the root:

```sh
docker compose restart mimir
```

Validate the complete application integration without starting services:

```sh
docker compose --env-file .env.example config --quiet --no-env-resolution
```

CI separately checks the application model/rules and starts Grafana to verify all
four datasources, both dashboards and the existing alert are provisioned. The
platform job checks that a fresh standalone Grafana has only LGTM datasources.

Existing project, network and volume names remain unchanged in this phase. Do not
start both modes against the same volumes concurrently or delete their volumes.
Previously stored Grafana resources require deliberate cleanup when reusing an
application volume in standalone mode; removing mounts alone is not a migration.
