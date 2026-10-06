#!/bin/sh
set -eu
umask 077
cd "${SECRETS_DIR:-/secrets}"
# Protect existing directories too, before touching any private key.
chmod 700 .
if [ "$(stat -c %a .)" != 700 ]; then
  echo 'Cannot protect secrets directory; restrict host permissions before continuing.' >&2
  exit 1
fi

mode="${1:-create}"
case "$mode" in
  create|renew) ;;
  *) echo 'Usage: generate-certificates.sh [create|renew]' >&2; exit 1 ;;
esac
key_gid="${TELEMETRY_SECRETS_GID:-1999}"
case "$key_gid" in
  ''|*[!0-9]*) echo 'TELEMETRY_SECRETS_GID must be a numeric group ID.' >&2; exit 1 ;;
esac

verify_key() {
  cert_public=$(openssl x509 -in "$1" -pubkey -noout) || return 1
  key_public=$(openssl pkey -in "$2" -pubout) || return 1
  if [ "$cert_public" != "$key_public" ]; then
    echo "Mismatched certificate/private key: $1; restore a matching pair from backup before retrying." >&2
    return 1
  fi
}

protect_leaf_key() {
  chgrp "$key_gid" "$1" || {
    echo 'Cannot assign telemetry key group; run the certificate tooling as root.' >&2
    exit 1
  }
  chmod 640 "$1"
}

check_pair() {
  if { [ -f "$1.crt" ] && [ ! -f "$1.key" ]; } ||
     { [ ! -f "$1.crt" ] && [ -f "$1.key" ]; }; then
    echo "Incomplete certificate/key pair: $1; restore the missing file." >&2
    exit 1
  fi
  if [ -f "$1.key" ]; then
    if [ "$1" = telemetry_ca ]; then
      chmod 600 "$1.key"
    else
      protect_leaf_key "$1.key"
    fi
    chmod 644 "$1.crt"
    verify_key "$1.crt" "$1.key"
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
  # Renewal keeps the existing key: only the certificate needs atomic replacement.
  # A failure before the final rename leaves the previous matching pair intact.
  key_path="$name.key"
  if [ ! -f "$key_path" ]; then
    key_path="$name.key.new"
    openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 \
      -out "$key_path" 2>/dev/null
    protect_leaf_key "$key_path"
  fi
  openssl req -new -key "$key_path" -subj "/CN=$name" \
    -out "$name.csr" 2>/dev/null
  printf 'basicConstraints=critical,CA:FALSE\nkeyUsage=critical,digitalSignature,keyEncipherment\nextendedKeyUsage=%s\nsubjectAltName=%s\n' \
    "$usage" "$san" > "$name.ext"
  openssl x509 -req -in "$name.csr" -CA telemetry_ca.crt -CAkey telemetry_ca.key \
    -set_serial "0x$(openssl rand -hex 16)" -days 365 \
    -extfile "$name.ext" -out "$name.crt.new" 2>/dev/null
  verify_key "$name.crt.new" "$key_path"
  openssl verify -CAfile telemetry_ca.crt "$name.crt.new"
  chmod 644 "$name.crt.new"
  if [ "$key_path" != "$name.key" ]; then
    mv "$key_path" "$name.key"
  fi
  mv "$name.crt.new" "$name.crt"
  rm "$name.csr" "$name.ext"
}

issue_certificate telemetry_client clientAuth DNS:telemetry-client
issue_certificate telemetry_alloy serverAuth DNS:alloy,DNS:alloy-1,DNS:alloy-2
for service in loki mimir tempo; do
  issue_certificate "telemetry_$service" serverAuth "DNS:$service"
done
chmod 644 telemetry_ca.crt
