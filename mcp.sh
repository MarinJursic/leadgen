#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

if [[ -f ".env.local" ]]; then
  set -a
  # shellcheck disable=SC1091
  source ".env.local"
  set +a
fi

DEFAULT_DB="$ROOT_DIR/src/LeadGen.Web/App_Data/leadgen.db"
export ConnectionStrings__DefaultConnection="${ConnectionStrings__DefaultConnection:-Data Source=$DEFAULT_DB}"

PROJECT="src/LeadGen.Mcp/LeadGen.Mcp.csproj"

ensure_built() {
  local build_log
  build_log="$(mktemp)"
  if ! dotnet build "$PROJECT" --nologo --verbosity quiet >"$build_log" 2>&1; then
    cat "$build_log" >&2
    rm -f "$build_log"
    return 1
  fi
  rm -f "$build_log"
}

run_mcp() {
  ensure_built
  dotnet run --no-build --project "$PROJECT"
}

usage() {
  cat <<'EOF'
Usage:
  ./mcp.sh                         Start the local stdio MCP server
  ./mcp.sh health                  Check MCP health
  ./mcp.sh campaigns               List campaigns
  ./mcp.sh search <query>          Search leads
  ./mcp.sh dossier <lead-id>       Get a lead dossier
  ./mcp.sh note <lead-id> <body>   Add a lead note
  ./mcp.sh status <lead-id> <New|Reviewed|Qualified|Rejected|Archived>
  ./mcp.sh tool <tool> [args...]   Call any raw MCP tool

Examples:
  ./mcp.sh health
  ./mcp.sh search CRM
  ./mcp.sh dossier <lead-id>
  ./mcp.sh note <lead-id> "MCP demo note"
EOF
}

run_tool() {
  ensure_built
  dotnet run --no-build --project "$PROJECT" -- --tool "$@"
}

if [[ $# -eq 0 ]]; then
  echo "Starting LeadGen.Mcp" >&2
  echo "Database: ${ConnectionStrings__DefaultConnection#Data Source=}" >&2
  echo "Press Ctrl+C to stop." >&2
  run_mcp
  exit 0
fi

case "$1" in
  -h|--help|help)
    usage
    ;;
  health)
    run_tool leadgen_health
    ;;
  campaigns|list-campaigns|list_campaigns)
    run_tool list_campaigns
    ;;
  search)
    shift
    if [[ $# -eq 0 ]]; then
      echo "Missing search query." >&2
      usage >&2
      exit 1
    fi
    run_tool search_leads --query "$*"
    ;;
  dossier)
    shift
    if [[ $# -ne 1 ]]; then
      echo "Usage: ./mcp.sh dossier <lead-id>" >&2
      exit 1
    fi
    run_tool get_lead_dossier --leadId "$1"
    ;;
  note)
    shift
    if [[ $# -lt 2 ]]; then
      echo "Usage: ./mcp.sh note <lead-id> <body>" >&2
      exit 1
    fi
    lead_id="$1"
    shift
    run_tool add_lead_note --leadId "$lead_id" --body "$*"
    ;;
  status)
    shift
    if [[ $# -ne 2 ]]; then
      echo "Usage: ./mcp.sh status <lead-id> <New|Reviewed|Qualified|Rejected|Archived>" >&2
      exit 1
    fi
    run_tool update_lead_status --leadId "$1" --status "$2"
    ;;
  tool)
    shift
    if [[ $# -eq 0 ]]; then
      echo "Usage: ./mcp.sh tool <tool> [args...]" >&2
      exit 1
    fi
    run_tool "$@"
    ;;
  *)
    run_tool "$@"
    ;;
esac
