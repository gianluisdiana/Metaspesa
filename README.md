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
a health-checked endpoint. The root Compose file includes the observability stack.
Its deployment files now live inside `observability/`; see its guide for running
only telemetry services. Metaspesa dashboards, alerts, PostgreSQL datasource,
and recording rules now live in `telemetry/`. The root includes that application
adapter together with the reusable stack, so full startup remains unchanged.
See [the application telemetry guide](telemetry/README.md) for asset ownership.

## Run locally

Prerequisites: Docker with Compose 2.24 or newer.

Copy `.env.example` to `.env` and replace its password placeholders. Create
`secrets/jwt_secret_key` (a random signing key of at least 32 bytes),
`secrets/scraper_username`, and `secrets/scraper_password` with your local values.
Keep these files private; they are ignored by Git. Preserve existing values.

```sh
docker compose -f observability/compose.tools.yaml run --rm certificates
docker compose up --build -d
```

The certificate command generates missing telemetry certificates and preserves
existing ones; it never changes application credentials or `.env`.
Telemetry certificates now live in `observability/secrets/`. Preserve the existing
CA and certificate pairs when moving from root `secrets/`; do not generate a
second CA for clients still using the original trust root. Application JWT and
scraper credentials remain in root `secrets/`.
Run migrations explicitly for a fresh database or a release with schema changes:

```sh
docker compose run --rm migrations
```

The API waits for a healthy database, not the migration runner. Open the client at
http://localhost:3000, the API at http://localhost:4001, and Grafana at
http://localhost:3001 (user `admin`, password `GRAFANA_PASSWORD` in `.env`).

```sh
docker compose down
```

See [the observability guide](observability/README.md) for label policy, batching
and memory limits, trace retention, Mimir recording rules, secure telemetry
transport, certificate renewal, HA behavior, and validation procedures.
