#!/usr/bin/env python3
"""
For each numeric data line in cliloc.cfg, replace the English (or current) text with
the Text column from CliLoc-cht.csv when the same CliLoc id exists there.

Lines beginning with '#', blanks, and lines without a leading id+tab data row are unchanged.

Defaults (paths relative to World/):
  cliloc.cfg     -> Data/System/CFG/cliloc.cfg
  CliLoc-cht.csv -> Documentation/CliLoc-cht.csv

Usage:
  python3 World/Source/Tools/apply_cliloc_cht_from_csv.py --dry-run
  python3 World/Source/Tools/apply_cliloc_cht_from_csv.py
"""

from __future__ import annotations

import argparse
import re
import shutil
from pathlib import Path

_SCRIPT_DIR = Path(__file__).resolve().parent
_REPO_WORLD = _SCRIPT_DIR.parent.parent
_DEFAULT_CFG = _REPO_WORLD / "Data/System/CFG/cliloc.cfg"
_DEFAULT_CSV = _REPO_WORLD / "Documentation/CliLoc-cht.csv"
_DEFAULT_BACKUP = _REPO_WORLD / "Data/System/CFG/cliloc-cht-apply-backup.cfg"

_ID_TAB_LINE = re.compile(r"^(\d+)\t(.*)$")


def _iter_csv_logical_lines(raw_lines: list[str]) -> list[str]:
    merged: list[str] = []
    buf: list[str] = []
    for line in raw_lines:
        if line.startswith("Number;Text;Flag"):
            continue
        if re.match(r"^\d+;", line):
            if buf:
                merged.append("".join(buf))
            buf = [line]
        else:
            buf.append(line)
    if buf:
        merged.append("".join(buf))
    return merged


def _parse_csv_logical_line(line: str) -> tuple[int, str] | None:
    line = line.rstrip("\n\r")
    m = re.match(r"^(\d+);(.*)$", line, re.DOTALL)
    if not m:
        return None
    rid = int(m.group(1))
    rest = m.group(2)
    for suffix in (";Original", ";Modified"):
        if rest.endswith(suffix):
            rest = rest[: -len(suffix)]
            break
    return rid, rest


def load_csv_cht(csv_path: Path) -> dict[int, str]:
    text = csv_path.read_text(encoding="utf-8", errors="replace")
    raw = text.splitlines(keepends=True)
    out: dict[int, str] = {}
    for log in _iter_csv_logical_lines(raw):
        p = _parse_csv_logical_line(log)
        if p is None:
            continue
        rid, rtext = p
        out[rid] = rtext
    return out


def apply_cht_to_cliloc_lines(
    cfg_lines: list[str],
    cht: dict[int, str],
) -> tuple[list[str], int, list[int]]:
    out: list[str] = []
    replaced = 0
    missing: list[int] = []
    for line in cfg_lines:
        bare = line.rstrip("\n\r")
        m = _ID_TAB_LINE.match(bare) if bare and not bare.lstrip().startswith("#") else None
        if not m:
            out.append(line)
            continue
        cid = int(m.group(1))
        # Match nl suffix from original line
        if line.endswith("\r\n"):
            nl = "\r\n"
        elif line.endswith("\r"):
            nl = "\r"
        else:
            nl = "\n"
        if cid not in cht:
            missing.append(cid)
            out.append(line)
            continue
        out.append(f"{cid}\t{cht[cid]}{nl}")
        replaced += 1
    return out, replaced, missing


def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--cliloc", type=Path, default=_DEFAULT_CFG)
    ap.add_argument("--csv", type=Path, default=_DEFAULT_CSV, help="CliLoc-cht.csv")
    ap.add_argument(
        "--backup",
        type=Path,
        default=_DEFAULT_BACKUP,
        help="Written before overwriting cliloc.cfg (skipped with --no-backup)",
    )
    ap.add_argument("--no-backup", action="store_true")
    ap.add_argument("--dry-run", action="store_true")
    ap.add_argument(
        "-o",
        "--output",
        type=Path,
        default=None,
        help="Write result here instead of cliloc.cfg (no backup)",
    )
    args = ap.parse_args()

    cht = load_csv_cht(args.csv)
    cfg_text = args.cliloc.read_text(encoding="utf-8", errors="replace")
    cfg_lines = cfg_text.splitlines(keepends=True)

    new_lines, replaced, missing = apply_cht_to_cliloc_lines(cfg_lines, cht)
    missing_unique = sorted(set(missing))
    print(
        f"CliLoc-cht.csv rows: {len(cht)} | "
        f"data lines replaced: {replaced} | "
        f"data lines kept (no CSV id): {len(missing_unique)}"
    )
    if missing_unique:
        sample = missing_unique[:15]
        print(f"  sample ids without CSV row: {sample}{' ...' if len(missing_unique) > 15 else ''}")

    if args.dry_run:
        return

    out_path = args.output or args.cliloc
    if args.output is None and not args.no_backup:
        shutil.copy2(args.cliloc, args.backup)
        print(f"Backup: {args.backup}")

    out_path.write_text("".join(new_lines), encoding="utf-8")
    print(f"Wrote {out_path}")


if __name__ == "__main__":
    main()
