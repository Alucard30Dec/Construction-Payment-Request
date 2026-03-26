#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
LOCAL_DOTNET="$BACKEND_DIR/.tools/dotnet/dotnet"
CHANNEL="${DOTNET_CHANNEL:-8.0}"

if [[ -x "$LOCAL_DOTNET" ]]; then
  exec "$LOCAL_DOTNET" "$@"
fi

if command -v dotnet >/dev/null 2>&1; then
  exec dotnet "$@"
fi

echo "[CPMS] dotnet is not found. Installing local SDK (channel $CHANNEL)..."
"$SCRIPT_DIR/install-dotnet-local.sh" "$CHANNEL"

if [[ ! -x "$LOCAL_DOTNET" ]]; then
  echo "[CPMS][ERROR] Local .NET installation did not produce executable: $LOCAL_DOTNET"
  exit 1
fi

exec "$LOCAL_DOTNET" "$@"
