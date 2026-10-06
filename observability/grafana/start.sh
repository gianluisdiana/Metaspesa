#!/bin/sh
set -eu

# Provisioning expands environment variables, not Docker secret file paths.
export TELEMETRY_CA_PEM="$(cat /run/secrets/telemetry_ca)"
export TELEMETRY_CLIENT_CERT_PEM="$(cat /run/secrets/telemetry_client_cert)"
export TELEMETRY_CLIENT_KEY_PEM="$(cat /run/secrets/telemetry_client_key)"
exec /run.sh
