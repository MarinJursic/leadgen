#!/usr/bin/env python3
import json
import shutil
import sys
from datetime import datetime, timezone
from pathlib import Path


def repo_root() -> Path:
    return Path(__file__).resolve().parents[2]


def append_text_log(log_path: Path, lines: list[str]) -> None:
    log_path.parent.mkdir(parents=True, exist_ok=True)
    with log_path.open("a", encoding="utf-8") as handle:
        handle.write("\n".join(lines) + "\n")


def append_raw_log(raw_path: Path, payload: dict) -> None:
    raw_path.parent.mkdir(parents=True, exist_ok=True)
    with raw_path.open("a", encoding="utf-8") as handle:
        handle.write(json.dumps(payload, ensure_ascii=False) + "\n")


def add_common_fields(lines: list[str], payload: dict) -> None:
    lines.append(f"session_id: {payload.get('session_id', '')}")
    lines.append(f"turn_id: {payload.get('turn_id', '')}")
    lines.append(f"cwd: {payload.get('cwd', '')}")
    lines.append(f"model: {payload.get('model', '')}")
    transcript_path = payload.get("transcript_path") or ""
    lines.append(f"transcript_path: {transcript_path}")


def add_pre_or_post_tool_fields(lines: list[str], payload: dict) -> None:
    lines.append(f"tool_name: {payload.get('tool_name', '')}")
    lines.append(f"tool_use_id: {payload.get('tool_use_id', '')}")

    tool_input = payload.get("tool_input", {}) or {}
    if tool_input:
        lines.append("tool_input:")
        lines.append(json.dumps(tool_input, ensure_ascii=False, indent=2))

    tool_response = payload.get("tool_response")
    if tool_response is not None:
        lines.append("tool_response:")
        if isinstance(tool_response, str):
            lines.append(tool_response)
        else:
            lines.append(json.dumps(tool_response, ensure_ascii=False, indent=2))


def session_ids_from_transcript(transcript_path: Path) -> list[str]:
    discovered: list[str] = []
    seen = set()

    try:
        with transcript_path.open("r", encoding="utf-8") as handle:
            for raw_line in handle:
                raw_line = raw_line.strip()
                if not raw_line:
                    continue

                try:
                    entry = json.loads(raw_line)
                except json.JSONDecodeError:
                    continue

                payload = entry.get("payload", {}) or {}
                if payload.get("type") != "collab_agent_spawn_end":
                    continue

                session_id = payload.get("new_thread_id")
                if session_id and session_id not in seen:
                    seen.add(session_id)
                    discovered.append(session_id)
    except FileNotFoundError:
        return []

    return discovered


def find_session_transcript(session_id: str) -> Path | None:
    sessions_root = Path.home() / ".codex" / "sessions"
    matches = sorted(sessions_root.rglob(f"*{session_id}.jsonl"))
    return matches[0] if matches else None


def transcript_line_hits(
    transcript_path: Path, patterns: list[tuple[str, str, int]]
) -> list[str]:
    hits: list[str] = []
    counts = {label: 0 for label, _, _ in patterns}

    try:
        with transcript_path.open("r", encoding="utf-8") as handle:
            for line_number, raw_line in enumerate(handle, start=1):
                for label, needle, limit in patterns:
                    if counts[label] >= limit:
                        continue
                    if needle in raw_line:
                        counts[label] += 1
                        hits.append(
                            f"- `{transcript_path.relative_to(repo_root())}:{line_number}` {label}"
                        )
    except FileNotFoundError:
        return []

    return hits


def write_capture_summary(
    summary_path: Path,
    payload: dict,
    exported_files: list[Path],
    child_transcripts: list[Path],
    evidence_lines: list[str],
) -> None:
    transcript_path = payload.get("transcript_path") or ""
    timestamp = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S UTC")

    lines = [
        "# Hook Capture Summary",
        "",
        f"- Generated at: {timestamp}",
        f"- Hook event: {payload.get('hook_event_name', '')}",
        f"- Session id: `{payload.get('session_id', '')}`",
        f"- Turn id: `{payload.get('turn_id', '')}`",
        f"- Transcript source: `{transcript_path}`",
        "",
        "## Notes",
        "",
        "- Shell hook events record prompt and Bash lifecycle data in `.github/hooks/agent_log.txt` and `.github/hooks/agent_log.jsonl`.",
        "- Non-shell tool calls such as `spawn_agent` are preserved in the exported session transcript JSONL.",
        "- Child sub-agent transcripts are copied when the parent session transcript references spawned thread ids.",
        "",
        "## Exported Files",
        "",
    ]

    for file_path in exported_files:
        lines.append(f"- `{file_path.relative_to(repo_root())}`")

    if child_transcripts:
        lines.extend(["", "## Child Session Transcripts", ""])
        for file_path in child_transcripts:
            lines.append(f"- `{file_path.relative_to(repo_root())}`")

    if evidence_lines:
        lines.extend(["", "## Key Evidence", ""])
        lines.extend(evidence_lines)

    summary_path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def export_stop_capture(payload: dict, text_log_path: Path, raw_log_path: Path) -> None:
    transcript_value = payload.get("transcript_path")
    if not transcript_value:
        return

    transcript_path = Path(transcript_value).expanduser()
    if not transcript_path.exists():
        return

    export_dir = repo_root() / "lab-2" / "hook-capture"
    export_dir.mkdir(parents=True, exist_ok=True)

    transcript_dest = export_dir / transcript_path.name
    if transcript_dest.exists():
        return

    exported_files: list[Path] = []

    if text_log_path.exists():
        text_dest = export_dir / "agent_log.txt"
        shutil.copy2(text_log_path, text_dest)
        exported_files.append(text_dest)

    if raw_log_path.exists():
        raw_dest = export_dir / "agent_log.jsonl"
        shutil.copy2(raw_log_path, raw_dest)
        exported_files.append(raw_dest)

    shutil.copy2(transcript_path, transcript_dest)
    exported_files.append(transcript_dest)

    evidence_lines = transcript_line_hits(
        transcript_dest,
        [
            ("spawn_agent call", '"name":"spawn_agent"', 1),
            ("spawn completion event", '"type":"collab_agent_spawn_end"', 1),
            ("wait_agent call", '"name":"wait_agent"', 1),
        ],
    )

    child_transcripts: list[Path] = []
    for session_id in session_ids_from_transcript(transcript_path):
        child_path = find_session_transcript(session_id)
        if child_path is None:
            continue

        child_dest = export_dir / child_path.name
        shutil.copy2(child_path, child_dest)
        exported_files.append(child_dest)
        child_transcripts.append(child_dest)
        evidence_lines.extend(
            transcript_line_hits(
                child_dest,
                [
                    ("child session metadata", '"type":"session_meta"', 1),
                    ("child assistant output", '"type":"agent_message"', 1),
                ],
            )
        )

    write_capture_summary(
        export_dir / "README.md",
        payload,
        exported_files,
        child_transcripts,
        evidence_lines,
    )


def main() -> None:
    try:
        payload = json.load(sys.stdin)
    except Exception as exc:
        print(f"Failed to parse hook JSON: {exc}", file=sys.stderr)
        sys.exit(1)

    root = repo_root()
    text_log_path = root / ".github" / "hooks" / "agent_log.txt"
    raw_log_path = root / ".github" / "hooks" / "agent_log.jsonl"

    append_raw_log(raw_log_path, payload)

    event = payload.get("hook_event_name", "unknown")
    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    lines: list[str] = [f"[{timestamp}] {event}"]
    add_common_fields(lines, payload)

    if event == "UserPromptSubmit":
        lines.append("prompt:")
        lines.append(payload.get("prompt", ""))
    elif event in {"PreToolUse", "PostToolUse"}:
        add_pre_or_post_tool_fields(lines, payload)
    elif event == "Stop":
        lines.append(f"stop_hook_active: {payload.get('stop_hook_active', '')}")
        lines.append("last_assistant_message:")
        lines.append(payload.get("last_assistant_message", "") or "")
    else:
        lines.append("payload:")
        lines.append(json.dumps(payload, ensure_ascii=False, indent=2))

    lines.append("-" * 60)
    append_text_log(text_log_path, lines)

    if event == "Stop":
        export_stop_capture(payload, text_log_path, raw_log_path)
        sys.stdout.write(json.dumps({"continue": True}))


if __name__ == "__main__":
    main()
