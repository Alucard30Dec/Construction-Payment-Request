#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
MYSQL_CONNECTION_STRING="${1:-}"
SKIP_RESTORE="${2:-false}"

read_mysql_from_local_config() {
  local backend_dir="$1"
  local file
  for file in \
    "$backend_dir/src/ConstructionPayment.Api/appsettings.Development.local.json" \
    "$backend_dir/src/ConstructionPayment.Api/appsettings.local.json"; do
    if [[ -f "$file" ]]; then
      local extracted
      extracted="$(grep -oP '"MySqlConnection"\s*:\s*"\K[^"]+' "$file" | head -n1 || true)"
      if [[ -n "${extracted:-}" ]]; then
        echo "$extracted"
        return 0
      fi
    fi
  done

  return 1
}

if [[ -z "$MYSQL_CONNECTION_STRING" ]]; then
  MYSQL_CONNECTION_STRING="${ConnectionStrings__MySqlConnection:-}"
fi
if [[ -z "$MYSQL_CONNECTION_STRING" ]]; then
  MYSQL_CONNECTION_STRING="$(read_mysql_from_local_config "$BACKEND_DIR" || true)"
fi

if [[ -z "$MYSQL_CONNECTION_STRING" ]]; then
  echo "[CPMS][ERROR] Missing MySql connection string."
  echo "Usage: ./scripts/bootstrap-tidb.sh \"mysql://<USER>:<PASSWORD>@<HOST>:4000/<DB>\""
  echo "Or create backend/src/ConstructionPayment.Api/appsettings.Development.local.json"
  exit 1
fi

if [[ "$MYSQL_CONNECTION_STRING" == *"<PASSWORD>"* ]]; then
  echo "[CPMS][ERROR] MySql connection string still contains <PASSWORD>."
  exit 1
fi

# Tránh giá trị env cũ ghi đè.
unset ConnectionStrings__MySqlConnection || true
unset DatabaseProvider || true

export ASPNETCORE_ENVIRONMENT=Development
export DatabaseProvider=MySql
export Database__AutoMigrateOnStartup=true
export Database__SeedDemoData=true
export Database__AllowSqliteFallbackInDevelopment=false
export ConnectionStrings__MySqlConnection="$MYSQL_CONNECTION_STRING"
export ASPNETCORE_URLS=http://localhost:5000

cd "$BACKEND_DIR"
if [[ "$SKIP_RESTORE" != "1" && "$SKIP_RESTORE" != "true" && "$SKIP_RESTORE" != "--skip-restore" ]]; then
  "$SCRIPT_DIR/dotnetw.sh" restore ConstructionPayment.sln
fi

"$SCRIPT_DIR/dotnetw.sh" run --project src/ConstructionPayment.Api/ConstructionPayment.Api.csproj -- --bootstrap-db
echo "[CPMS] TiDB bootstrap thành công (code-first migrate + seed demo data)."
