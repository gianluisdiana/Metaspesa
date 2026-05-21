# TODO:
## Server
- [ ] Think if still make sense to have nullable quantity
- [ ] Divide db context, one per schema
- [ ] Consider using Pkl to create and validate setting file

## Scraper
- [ ] Think what to do about same product (name, brand, market and quantity) with different format (vase, can, etc.)
- [ ] Add support for other retailers (e.g. Carrefour, Dialprix, etc.)
- [ ] Make it run periodically using docker / k8s

## Client
- [ ] Improve SEO (meta tags, sitemap, etc.)
- [ ] Add i18n support (English + Spanish)
- [ ] Add functional test with playwright to test critical user flows (search, product details, price history, etc.)
- [ ] Create interactivity with the server.
- [ ] Add user authentication and profiles to save favorite products, set price alerts, etc.

## Observability

### Alloy
- Stage 3 — label control for Loki

  By default Alloy promotes all OTLP resource attributes as Loki labels, which causes high cardinality. Lock it down to only the labels you actually filter by:
  ```alloy
  otelcol.exporter.loki "default" {
    forward_to = [loki.write.default.receiver]
    default_labels_enabled {
      exporter = false
      job      = true
      instance = false
      level    = true
    }
  }
  ```
  `service_name` is always promoted. level lets you filter by severity. Everything else stays in the log body.

- Stage 4 — attribute processing

  Add a `otelcol.processor.batch` before the exporters to buffer small writes into efficient batches, and `otelcol.processor.memory_limiter` to prevent OOM under traffic spikes:
  ```alloy
  otelcol.processor.batch "default" {
    output {
      logs    = [otelcol.exporter.otlphttp.loki.input]
      traces  = [otelcol.exporter.otlp.tempo.input]
      metrics = [otelcol.exporter.otlphttp.mimir.input]
    }
  }
  ```
  The receiver's output then points to the batch processor instead of directly to exporters.

- Stage 5 — sampling (high trace volume)

  When trace volume grows, recording every span becomes expensive. Add tail-based sampling to keep only interesting traces (errors, slow requests):
  ```alloy
  otelcol.processor.tail_sampling "default" {
    policy {
      name = "errors"
      type = "status_code"
      status_code { status_codes = ["ERROR"] }
    }
    policy {
      name = "slow"
      type = "latency"
      latency { threshold_ms = 500 }
    }
    output {
      traces = [otelcol.exporter.otlp.tempo.input]
    }
  }
  ```

- Stage 6 — production (TLS + auth)

  Inside Docker everything is plaintext. For production, Alloy becomes an authenticated proxy:
  ```alloy
  loki.write "default" {
    endpoint {
      url = "https://logs.grafana.net/loki/api/v1/push"
      basic_auth {
        username = env("LOKI_USER")
        password = env("LOKI_API_KEY")
      }
    }
  }
  ```

  Same pattern for Mimir and Tempo — swap the endpoint URLs and add auth blocks. Works identically for Grafana Cloud or self-hosted with TLS.

- Stage 7 — Alloy cluster (high availability)

  Run multiple Alloy instances behind a load balancer. For tail sampling to work correctly across instances, they need to communicate — enable clustering:
  ```alloy
  clustering {
    enabled = true
  }
  ```
  Alloy uses gossip to ensure spans belonging to the same trace always reach the same instance for sampling decisions.

### Mimir

- Stage 3 — persistent dev / staging

  Move storage out of /tmp so metrics survive restarts. The mimir_data volume is already mounted at /tmp/mimir — just change the paths:
  ```yml
  blocks_storage:
    filesystem:
      dir: /var/mimir/blocks
    bucket_store:
      sync_dir: /var/mimir/tsdb-sync

  compactor:
    data_dir: /var/mimir/compactor

  ruler_storage:
    local:
      directory: /var/mimir/ruler
  ```

- Stage 4 — alerting rules

  Define recording rules (pre-aggregate expensive queries) and alerting rules (fire when gRPC error rate spikes):
  ```yml
  # observability/mimir/rules/metaspesa.yaml
  groups:
    - name: grpc
      rules:
        - alert: HighErrorRate
          expr: rate(grpc_server_handled_total{grpc_code!="OK"}[5m]) > 0.05
          for: 2m
  ```
  The ruler_storage directory is already wired — drop rule files there and Mimir picks them up.

- Stage 5 — Grafana dashboards as code

  Provision dashboards alongside the datasource so the Grafana container is fully reproducible:
  ```
  observability/grafana/provisioning/
    dashboards/
      dashboards.yaml       # dashboard provider config
      grpc-overview.json    # exported from Grafana UI
  ```

- Stage 6 — production (object storage)

  Same pattern as Loki and Tempo:
  ```yml
  blocks_storage:
    backend: s3
    s3:
      bucket_name: metaspesa-metrics
      endpoint: s3.amazonaws.com
    bucket_store:
      sync_dir: /var/mimir/tsdb-sync   # stays local, acts as read cache
  ```
  WAL and compactor scratch space remain on local disk. Only completed blocks go to S3.

- Stage 7 — distributed Mimir (high metric cardinality)

  Split into distributor, ingester, store-gateway, querier, compactor, ruler. Ring moves from inmemory to memberlist. Enables horizontal scaling of each component independently. Mimir is designed for this from day one — the single-binary config is a subset of the same config file the distributed deployment uses.

### Loki
- Stage 3 — useful queries (structured logging)

  The .NET server already emits structured logs via OTLP. Add a pipeline in Alloy to promote important OTLP resource attributes into Loki labels so you can filter efficiently:
  ```alloy
  otelcol.exporter.loki "default" {
    forward_to = [loki.write.default.receiver]
    default_labels_enabled {
      exporter = false
      job      = true
      instance = true
      level    = true
    }
  }
  ```

  Fewer labels = better performance. Only promote high-cardinality filters you actually use (service_name, level). Never label by trace_id or user_id — that's what the log body is for.

- Stage 4 — trace-to-log correlation

  Same as Tempo Stage 4 — wire up the Grafana datasource so clicking a span in Tempo jumps to Loki logs for that trace_id. The OTLP pipeline already injects trace_id into log records, so no server code changes needed.

- Stage 5 — persistent staging / production

  Replace filesystem with object storage. Schema config gets a new entry (never edit the old one):
  ```yml
  schema_config:
    configs:
      - from: 2020-10-24       # keep existing entry untouched
        store: tsdb
        object_store: filesystem
        schema: v13
        index:
          prefix: index_
          period: 24h
      - from: <migration-date>  # new entry takes effect from this date
        store: tsdb
        object_store: s3
        schema: v13
        index:
          prefix: index_
          period: 24h

  storage_config:
    aws:
      s3: s3://metaspesa-logs
  ```

- Stage 6 — distributed Loki (high log volume)

  Split into distributor, ingester, querier, ruler, compactor components. The ring.kvstore moves from inmemory to memberlist (gossip protocol) or etcd. Mirrors the Tempo distributed path — only needed if a single node saturates under log ingestion load.

### Tempo
- Stage 3 — persistent dev / staging

  Replace /tmp with a named Docker volume so traces survive restarts:
  ```yml
  storage:
    trace:
      backend: local
      local:
        path: /var/tempo/blocks
      wal:
        path: /var/tempo/wal
  ```
  Also add retention so disk doesn't fill:
  ```yml
  compactor:
    compaction:
      block_retention: 48h
  ```

- Stage 4 — trace-to-log correlation

  Link Tempo ↔ Loki in Grafana provisioning so you can jump from a trace span directly to the logs emitted during that span:
  ```yml
  # grafana/provisioning/datasources/tempo.yaml
    jsonData:
      tracesToLogsV2:
        datasourceUid: loki
        filterByTraceID: true
        filterBySpanID: true
  ```
  This requires the server to emit trace_id and span_id as structured log fields — which the .NET OTLP exporter already does.

- Stage 5 — production (object storage)

  Swap local backend for S3/GCS so traces are durable and Tempo can scale horizontally:
  ```yml
  storage:
    trace:
      backend: s3
      s3:
        bucket: metaspesa-traces
        endpoint: s3.amazonaws.com
  ```
  WAL stays local (fast ephemeral disk on the instance), blocks go to object store.

- Stage 6 — distributed Tempo (high traffic)

  Split single-binary into microservices (distributor, ingester, querier, compactor) behind a load balancer. The config gains a member-list ring for coordination. Only relevant if trace volume becomes large enough to saturate a single node.
