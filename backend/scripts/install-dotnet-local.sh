#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
INSTALL_DIR="$BACKEND_DIR/.tools/dotnet"
DOTNET_BIN="$INSTALL_DIR/dotnet"
CHANNEL="${1:-8.0}"

if [[ -x "$DOTNET_BIN" ]]; then
  echo "[CPMS] Local .NET already installed at $INSTALL_DIR"
  "$DOTNET_BIN" --info
  exit 0
fi

if command -v curl >/dev/null 2>&1; then
  FETCH_CMD=(curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$SCRIPT_DIR/.dotnet-install.sh")
elif command -v wget >/dev/null 2>&1; then
  FETCH_CMD=(wget -qO "$SCRIPT_DIR/.dotnet-install.sh" https://dot.net/v1/dotnet-install.sh)
else
  echo "[CPMS][ERROR] Neither curl nor wget is available."
  exit 1
fi

echo "[CPMS] Downloading dotnet-install.sh..."
"${FETCH_CMD[@]}"
chmod +x "$SCRIPT_DIR/.dotnet-install.sh"

echo "[CPMS] Installing .NET SDK channel $CHANNEL to $INSTALL_DIR..."
"$SCRIPT_DIR/.dotnet-install.sh" --channel "$CHANNEL" --install-dir "$INSTALL_DIR" --no-path
rm -f "$SCRIPT_DIR/.dotnet-install.sh"

echo "[CPMS] Local .NET installed successfully."
"$DOTNET_BIN" --info
