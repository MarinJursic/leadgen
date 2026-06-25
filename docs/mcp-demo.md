# MCP Demo Checklist

This checklist proves that the local LeadGen MCP server can be started and can read/write the same SQLite data as the web app.

## Start the web app

```bash
./run.sh
```

The default local URL is:

```text
http://127.0.0.1:5050
```

## Run direct MCP checks

In a second terminal:

```bash
./mcp.sh health
./mcp.sh campaigns
./mcp.sh search CRM
```

Expected behavior:

- `health` returns `{"status":"ok"}`.
- `campaigns` returns campaign summaries from `src/LeadGen.Web/App_Data/leadgen.db`.
- `search` returns matching lead summaries when matching leads exist.

## Read and update a lead

Use a lead id from `./mcp.sh search <query>`:

```bash
./mcp.sh dossier <lead-id>
./mcp.sh note <lead-id> "MCP demo note"
./mcp.sh status <lead-id> Reviewed
```

Expected behavior:

- `dossier` returns dossier markdown and contact/evidence data.
- `note` returns the created note id.
- `status` returns the updated status.
- The note and status are visible in the web UI on the lead dossier page.

## VS Code config

```json
{
  "servers": {
    "leadgen": {
      "type": "stdio",
      "command": "/bin/bash",
      "args": ["${workspaceFolder}/mcp.sh"],
      "cwd": "${workspaceFolder}"
    }
  }
}
```

After saving `.vscode/mcp.json`, run `MCP: List Servers` in the VS Code Command Palette, select `leadgen`, and start or restart the server. `mcp.sh` loads `.env.local` and defaults `ConnectionStrings__DefaultConnection` to `src/LeadGen.Web/App_Data/leadgen.db`, so MCP and the web app use the same local database.

## Automated proof

The default test suite includes `McpCli_Tools_CanReadAndUpdateLeadData`, which:

- creates an isolated SQLite database,
- seeds one campaign and one lead,
- starts the real `LeadGen.Mcp` executable,
- verifies `leadgen_health`, `list_campaigns`, `search_leads`, `get_lead_dossier`, `add_lead_note`, and `update_lead_status`.

Run it with:

```bash
dotnet test tests/LeadGen.Tests/LeadGen.Tests.csproj --filter McpCli
```
