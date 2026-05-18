#!/usr/bin/env python3
"""
Compare CliLoc-en.csv vs CliLoc-cht.csv (semicolon-separated: Number;Text;Flag).

Reports rows where:
  - EN has non-empty Text but CHT Text is empty, or the number is missing in CHT
  - Either file has a malformed line (not Number;...;Flag)

Does not list CHT-only numbers (missing_in_en); those are out of scope.

Output: World/Documentation/CliLoc-en-cht-diff.csv

Usage (from repo root):
  python3 World/Source/Tools/cliloc_en_cht_diff.py
  python3 World/Source/Tools/cliloc_en_cht_diff.py -o path/to/out.csv
"""

from __future__ import annotations

import argparse
import csv
from collections import OrderedDict
from pathlib import Path


def parse_cliloc_line(line: str, path: str, lineno: int) -> tuple[str, str, str] | None:
    line = line.rstrip("\n\r")
    if not line or line.startswith("#"):
        return None
    first = line.find(";")
    last = line.rfind(";")
    if first < 0 or last <= first:
        return None
    num = line[:first].strip()
    flag = line[last + 1 :].strip()
    text = line[first + 1 : last]
    if not num or not num[0].isdigit():
        return None
    return (num, text, flag)


def load_csv(path: Path) -> tuple[OrderedDict[str, tuple[str, str]], list[tuple[int, str]]]:
    """Returns (ordered map number -> (text, flag), malformed_lines (lineno, raw))."""
    data: OrderedDict[str, tuple[str, str]] = OrderedDict()
    bad: list[tuple[int, str]] = []
    with path.open(encoding="utf-8", errors="replace") as f:
        for lineno, line in enumerate(f, 1):
            if lineno == 1 and line.strip().lower().startswith("number;"):
                continue
            parsed = parse_cliloc_line(line, str(path), lineno)
            if parsed is None:
                if line.strip():
                    bad.append((lineno, line.rstrip("\n\r")))
                continue
            num, text, flag = parsed
            data[num] = (text, flag)
    return data, bad


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument(
        "--en",
        type=Path,
        default=Path(__file__).resolve().parents[2] / "Documentation" / "CliLoc-en.csv",
        help="Path to CliLoc-en.csv",
    )
    ap.add_argument(
        "--cht",
        type=Path,
        default=Path(__file__).resolve().parents[2] / "Documentation" / "CliLoc-cht.csv",
        help="Path to CliLoc-cht.csv",
    )
    ap.add_argument(
        "-o",
        "--output",
        type=Path,
        default=Path(__file__).resolve().parents[2] / "Documentation" / "CliLoc-en-cht-diff.csv",
    )
    args = ap.parse_args()

    en_map, en_bad = load_csv(args.en)
    cht_map, cht_bad = load_csv(args.cht)

    rows: list[dict[str, str]] = []

    for lineno, raw in en_bad:
        rows.append(
            {
                "number": "",
                "reason": "en_malformed_line",
                "en_text": "",
                "cht_text": "",
                "en_flag": "",
                "cht_flag": "",
                "en_line": str(lineno),
                "note": raw[:500],
            }
        )

    for lineno, raw in cht_bad:
        rows.append(
            {
                "number": "",
                "reason": "cht_malformed_line",
                "en_text": "",
                "cht_text": "",
                "en_flag": "",
                "cht_flag": "",
                "cht_line": str(lineno),
                "note": raw[:500],
            }
        )

    for num in en_map:
        in_en = True
        in_cht = num in cht_map
        en_text, en_flag = en_map[num]
        cht_text, cht_flag = cht_map[num] if in_cht else ("", "")

        en_nonempty = bool(en_text.strip())
        cht_nonempty = bool(cht_text.strip())

        reason: str | None = None
        if not in_cht and en_nonempty:
            reason = "missing_in_cht"
        elif in_cht and en_nonempty and not cht_nonempty:
            reason = "cht_empty_en_has_text"
        if reason:
            rows.append(
                {
                    "number": num,
                    "reason": reason,
                    "en_text": en_text.replace("\r", " ").replace("\n", "\\n"),
                    "cht_text": cht_text.replace("\r", " ").replace("\n", "\\n"),
                    "en_flag": en_flag,
                    "cht_flag": cht_flag,
                }
            )

    args.output.parent.mkdir(parents=True, exist_ok=True)
    fieldnames = [
        "number",
        "reason",
        "en_text",
        "cht_text",
        "en_flag",
        "cht_flag",
        "en_line",
        "cht_line",
        "note",
    ]
    with args.output.open("w", encoding="utf-8", newline="") as out:
        w = csv.DictWriter(out, fieldnames=fieldnames, extrasaction="ignore")
        w.writeheader()
        for r in rows:
            w.writerow({k: r.get(k, "") for k in fieldnames})

    print(f"Wrote {len(rows)} rows to {args.output}")
    print(f"  EN entries: {len(en_map)}, CHT entries: {len(cht_map)}")
    print(f"  EN malformed: {len(en_bad)}, CHT malformed: {len(cht_bad)}")


if __name__ == "__main__":
    main()
