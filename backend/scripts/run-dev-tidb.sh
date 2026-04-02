#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
MYSQL_CONNECTION_STRING="${1:-}"
BOOTSTRAP_DB="${2:-false}"
RESTORE_FIRST="${3:-false}"

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
  echo "Usage: ./scripts/run-dev-tidb.sh \"mysql://<USER>:<PASSWORD>@<HOST>:4000/<DB>\" [--bootstrap-db]"
  echo "Or create backend/src/ConstructionPayment.Api/appsettings.Development.local.json"
  exit 1
fi

if [[ "$MYSQL_CONNECTION_STRING" == *"<PASSWORD>"* ]]; then
  echo "[CPMS][ERROR] MySql connection string still contains <PASSWORD>."
  echo "Pass your real connection string as arg #1 or set ConnectionStrings__MySqlConnection."
  exit 1
fi

# Tránh giá trị env cũ ghi đè gây lỗi <PASSWORD>.
unset ConnectionStrings__MySqlConnection || true
unset DatabaseProvider || true

export ASPNETCORE_ENVIRONMENT=Development
export DatabaseProvider=MySql
export Database__AutoMigrateOnStartup=false
export Database__SeedDemoData=false
export Database__AllowSqliteFallbackInDevelopment=false
export ConnectionStrings__MySqlConnection="$MYSQL_CONNECTION_STRING"
export ASPNETCORE_URLS=http://localhost:5000

cd "$BACKEND_DIR"
if [[ "$RESTORE_FIRST" == "1" || "$RESTORE_FIRST" == "true" || "$RESTORE_FIRST" == "--restore-first" ]]; then
  "$SCRIPT_DIR/dotnetw.sh" restore ConstructionPayment.sln
fi
if [[ "$BOOTSTRAP_DB" == "1" || "$BOOTSTRAP_DB" == "true" || "$BOOTSTRAP_DB" == "--bootstrap-db" ]]; then
  # Chỉ bootstrap khi chủ động yêu cầu.
  "$SCRIPT_DIR/dotnetw.sh" run --project src/ConstructionPayment.Api/ConstructionPayment.Api.csproj -- --bootstrap-db
fi
# Chạy watch cho dev hằng ngày.
"$SCRIPT_DIR/dotnetw.sh" watch --project src/ConstructionPayment.Api/ConstructionPayment.Api.csproj run
