#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

URL="${1:-${ASPNETCORE_URLS:-http://127.0.0.1:5050}}"

if [[ -f ".env.local" ]]; then
  set -a
  # shellcheck disable=SC1091
  source ".env.local"
  set +a
fi

export ASPNETCORE_URLS="$URL"

echo "Restoring packages..."
dotnet restore LeadGen.sln

echo "Building app..."
dotnet build LeadGen.sln --no-restore

echo "Starting LeadGen.Web"
echo "Site:   $URL"
echo "Health: $URL/api/health"
echo "Press Ctrl+C to stop."

dotnet run --no-build --project src/LeadGen.Web/LeadGen.Web.csproj --urls "$URL"
