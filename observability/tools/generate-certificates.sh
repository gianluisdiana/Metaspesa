#!/bin/sh
set -eu
cd /secrets
umask 077

mode="${1:-create}"

check_pair() {
  if { [ -f "$1.crt" ] && [ ! -f "$1.key" ]; } ||
     { [ ! -f "$1.crt" ] && [ -f "$1.key" ]; }; then
    echo "Incomplete certificate/key pair: $1; restore the missing file." >&2
    exit 1
  fi
}

check_pair telemetry_ca
if [ ! -f telemetry_ca.crt ]; then
  openssl req -x509 -newkey rsa:3072 -nodes -days 3650 \
    -keyout telemetry_ca.key -out telemetry_ca.crt \
    -subj '/CN=Metaspesa telemetry CA' \
    -addext 'basicConstraints=critical,CA:TRUE' \
    -addext 'keyUsage=critical,keyCertSign,cRLSign' 2>/dev/null
fi
openssl x509 -in telemetry_ca.crt -checkend 86400 -noout

issue_certificate() {
  name="$1"
  usage="$2"
  san="$3"
  check_pair "$name"
  if [ -f "$name.crt" ] && [ "$mode" != renew ]; then
    openssl x509 -in "$name.crt" -checkend 86400 -noout || {
      echo 'Certificate expiry requires: docker compose -f compose.tools.yaml run --rm certificates renew' >&2
      exit 1
    }
    openssl verify -CAfile telemetry_ca.crt "$name.crt"
    return
  fi
  openssl req -new -newkey rsa:2048 -nodes -subj "/CN=$name" \
    -keyout "$name.key.new" -out "$name.csr" 2>/dev/null
  printf 'basicConstraints=critical,CA:FALSE\nkeyUsage=critical,digitalSignature,keyEncipherment\nextendedKeyUsage=%s\nsubjectAltName=%s\n' \
    "$usage" "$san" > "$name.ext"
  openssl x509 -req -in "$name.csr" -CA telemetry_ca.crt -CAkey telemetry_ca.key \
    -set_serial "0x$(openssl rand -hex 16)" -days 365 \
    -extfile "$name.ext" -out "$name.crt.new" 2>/dev/null
  mv "$name.key.new" "$name.key"
  mv "$name.crt.new" "$name.crt"
  rm "$name.csr" "$name.ext"
  # Compose file secrets are bind mounts; non-root container users must read them.
  # The host secrets directory remains private; the CA private key is never mounted.
  chmod 644 "$name.crt" "$name.key"
}

issue_certificate telemetry_client clientAuth DNS:telemetry-client
issue_certificate telemetry_alloy serverAuth DNS:alloy
for service in loki mimir tempo; do
  issue_certificate "telemetry_$service" serverAuth "DNS:$service"
done
chmod 644 telemetry_ca.crt
