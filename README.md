# TODO:
- [ ] Bug: fix case when product comes in pack. ej . 6x1L, 12x1L, etc.

## Server
- [ ] Add functional tests
- [ ] Think how to seed master data (unit of measure, etc.)
- [ ] Divide db context, one per schema
- [ ] Consider using Pkl to create and validate setting file

## Scraper
- [ ] Bug: fix remove already implied words, such as "grande", "pequeño", "familiar", "individual", etc.
- [ ] Bug: fix extra spaces when processing product
- [ ] Bug: Think what to do about same product (name, brand, market and quantity) with different format (vase, can, etc.)
- [ ] Add support for other retailers (e.g. Carrefour, Dialprix, etc.)
- [ ] Make it run periodically using docker / k8s

## Client
- [ ] Improve SEO (meta tags, sitemap, etc.)
- [ ] Add i18n support (English + Spanish)
- [ ] Add functional test with playwright to test critical user flows (search, product details, price history, etc.)
- [ ] Add user authentication and profiles to save favorite products, set price alerts, etc.

## Observability

The local observability stack collects OpenTelemetry logs, metrics, and traces with
Alloy and stores them in Loki, Mimir, and Tempo. Grafana is provisioned with the
matching data sources, dashboards, alert, and trace-to-log correlation.

Telemetry uses authenticated TLS by default, with two Alloy collectors behind
a health-checked endpoint. Observability now runs as its own Compose project;
the default application stack does not start LGTM or require telemetry secrets.
Metaspesa dashboards, alerts, PostgreSQL datasource and rules live in `telemetry/`.
See [the application telemetry guide](telemetry/README.md) to opt into telemetry
and [the observability guide](observability/README.md) for standalone deployment.

## Run locally

Prerequisites: Docker with Compose 2.24 or newer.

Copy `.env.example` to `.env` and replace its password placeholders. Create
`secrets/jwt_secret_key` (a random signing key of at least 32 bytes),
`secrets/scraper_username`, and `secrets/scraper_password` with your local values.
Keep these files private; they are ignored by Git. Preserve existing values.

```sh
docker compose up --build -d
```

No collector endpoint is configured in this default mode: client and scraper
exporters stay off, and the server/migration runner register no OTLP exporter.
Application JWT and scraper credentials remain in root `secrets/`. The read-only
Grafana database role remains application-owned; `GRAFANA_DB_PASSWORD` is its
password, not the Grafana administrator password.
Run migrations explicitly for a fresh database or a release with schema changes:

```sh
docker compose run --rm migrations
```

The API waits for a healthy database, not the migration runner. Open the client at
http://localhost:3000 and the API at http://localhost:4001.

```sh
docker compose down
```

## Optional telemetry

Prepare `observability/.env` and certificates according
to the observability guide. For existing deployments, follow its ownership
migration instructions before starting the new project. Preserve the CA;
Compose creates fresh observability-owned storage. Run from the repository root:

```sh
docker compose --env-file .env --env-file observability/.env -f observability/compose.yaml -f telemetry/compose.observability.yaml up -d
docker compose -f compose.yaml -f telemetry/compose.yaml up --build -d
```

Observability owns the shared network and starts first. The application adapter
attaches services and database without starting or owning any LGTM services.
Grafana is at http://localhost:3001 (`admin`, password from `observability/.env`).
Both projects stop independently; see the telemetry guide for matching shutdown
commands and switching back to application-only mode.

See [the observability guide](observability/README.md) for label policy, batching
and memory limits, trace retention, Mimir recording rules, secure telemetry
transport, certificate renewal, HA behavior, and validation procedures.
