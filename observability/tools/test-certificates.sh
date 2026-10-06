#!/bin/sh
# Integration tests: run in the certificate tooling image, never against live secrets.
set -eu
umask 077
test_root=$(mktemp -d)
trap 'rm -rf "$test_root"' EXIT
generator=/tools/generate-certificates.sh
export SECRETS_DIR="$test_root/secrets"
mkdir "$SECRETS_DIR"
cd "$SECRETS_DIR"

generate() {
  /bin/sh "$generator" "$@" > "$test_root/output" 2>&1
}

expect_failure() {
  if generate "$@"; then
    echo 'Expected certificate validation to fail.' >&2
    exit 1
  fi
}

chmod 777 .
generate
[ "$(stat -c %a .)" = 700 ]
echo 'PASS: permissive secrets directory is restricted before issuance'
[ "$(stat -c %a telemetry_ca.key)" = 600 ]
echo 'PASS: CA key is owner-only'
for name in telemetry_client telemetry_alloy telemetry_loki telemetry_mimir telemetry_tempo; do
  [ "$(stat -c %a "$name.key")" = 640 ]
  [ "$(stat -c %g "$name.key")" = "${TELEMETRY_SECRETS_GID:-1999}" ]
done
echo 'PASS: all leaf keys are restricted to the telemetry group'
for cert in ./*.crt; do
  [ "$(stat -c %a "$cert")" = 644 ]
done
echo 'PASS: public certificate permissions are separate from key permissions'

before=$(sha256sum telemetry_client.key telemetry_client.crt)
chmod 777 .
chmod 666 telemetry_client.key telemetry_ca.key
generate
[ "$(stat -c %a .)" = 700 ]
[ "$(stat -c %a telemetry_client.key)" = 640 ]
[ "$(stat -c %a telemetry_ca.key)" = 600 ]
echo 'PASS: reused keys and directory permissions are hardened'
[ "$before" = "$(sha256sum telemetry_client.key telemetry_client.crt)" ]
echo 'PASS: create preserves existing credential contents'

mv telemetry_client.key "$test_root/client.key"
expect_failure
grep -q 'Incomplete certificate/key pair' "$test_root/output"
mv "$test_root/client.key" telemetry_client.key
echo 'PASS: missing leaf key is rejected without silently replacing credentials'

cp telemetry_client.key "$test_root/client.key"
cp telemetry_alloy.key telemetry_client.key
expect_failure
grep -q 'Mismatched certificate/private key' "$test_root/output"
expect_failure renew
cp "$test_root/client.key" telemetry_client.key
echo 'PASS: mismatched leaf pair is rejected in both create and renew modes'

cp telemetry_ca.key "$test_root/ca.key"
cp telemetry_alloy.key telemetry_ca.key
expect_failure
grep -q 'Mismatched certificate/private key' "$test_root/output"
cp "$test_root/ca.key" telemetry_ca.key
echo 'PASS: mismatched CA pair is rejected'

# Interrupt renewal immediately before the atomic certificate replacement.
# The PATH shim runs only in this disposable test container.
mkdir "$test_root/bin"
printf '#!/bin/sh\ncase "$1" in *.crt.new) exit 75 ;; esac\nexec /bin/mv "$@"\n' > "$test_root/bin/mv"
chmod 700 "$test_root/bin/mv"
before=$(sha256sum telemetry_client.key telemetry_client.crt)
if PATH="$test_root/bin:$PATH" generate renew; then
  echo 'Expected interrupted renewal to fail.' >&2
  exit 1
fi
[ "$before" = "$(sha256sum telemetry_client.key telemetry_client.crt)" ]
generate
echo 'PASS: interrupted renewal leaves the live pair unchanged and reusable'

keys_before=$(sha256sum ./*.key)
cert_before=$(sha256sum telemetry_client.crt)
generate renew
[ "$keys_before" = "$(sha256sum ./*.key)" ]
echo 'PASS: renewal preserves all private keys'
[ "$cert_before" != "$(sha256sum telemetry_client.crt)" ]
echo 'PASS: renewal replaces the leaf certificate'
generate
echo 'PASS: renewed pairs pass subsequent validation'

expect_failure unsupported
echo 'PASS: unsupported operation is rejected'
if TELEMETRY_SECRETS_GID=invalid generate; then
  echo 'Expected invalid telemetry group ID to fail.' >&2
  exit 1
fi
echo 'PASS: invalid telemetry group ID is rejected'
