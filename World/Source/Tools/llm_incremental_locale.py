#!/usr/bin/env python3
"""
Incremental LLM translation helpers for Data/Localization (zh-Hans).

Does NOT call any translation API. Use this to emit only strings that need
work, then paste the LLM JSON response back with ``apply``.

  # 1) Emit queue (JSON lines: one object per line with file, key, en)
  python3 llm_incremental_locale.py queue -o Data/Localization/tools-output/llm-translation-queue.jsonl

  # 2) Optional: split a large queue for smaller LLM requests (80 lines per file)
  python3 llm_incremental_locale.py split-queue -i Data/Localization/tools-output/llm-translation-queue.jsonl \\
    --lines 80 -o Data/Localization/tools-output/llm-batches/

  # 3) After LLM returns translations as JSON (see AGENTS.md §3.4), merge into zh-Hans:
  python3 llm_incremental_locale.py apply -i llm-translation-response.json

Response file format (single JSON object, keys are locale filenames):

  {
    "scripts-quests.json": {
      "s.abcdef0123456789": "……中文……"
    },
    "scripts-system.json": { ... }
  }

Alternative: JSON array / JSONL where each object is {\"file\":\"...\",\"key\":\"...\",\"zh\":\"...\"}
"""
from __future__ import annotations

import argparse
import json
import os
import re
import sys
from typing import Any, Dict, Iterable, Iterator, List, Tuple

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
EN_DIR = os.path.join(ROOT, "Data", "Localization", "en")
ZH_DIR = os.path.join(ROOT, "Data", "Localization", "zh-Hans")

RE_HASH_KEY = re.compile(r"^s\.[0-9a-f]{16}$")

# When zh-Hans mirrors ``en`` on purpose (markup, placeholders, internal ids), do not queue.
_IDENTITY_HASH_EN_VALUES = frozenset(
    {
        # UO / Ultima runic circle words and similar fictive incantations: zh-Hans must mirror en.
        "In Vas Mani",
        "Xtee Mee Glau",
        "English",
        "en",
        "zh-Hans",
        "zZz",
    }
)


def _en_catalog_already_cjk(en_val: str) -> bool:
    """True when the ``en`` string is already player-facing Chinese (mis-keyed as en)."""
    cjk = sum(1 for c in en_val if "\u4e00" <= c <= "\u9fff")
    return cjk >= 2


def _hash_en_identity_ok_for_zh_hans(en_val: str) -> bool:
    """
    Hash-key entries where keeping zh identical to en is intentional and should not
    count as ``needs_translation`` (gump HTML, numeric tokens, internal labels).
    """
    t = en_val.strip()
    if not t:
        return False
    if _en_catalog_already_cjk(t):
        return True
    if t in _IDENTITY_HASH_EN_VALUES:
        return True
    if "m_Artifact" in t or ".PaganName" in t:
        return True
    if t.startswith("<") or t.startswith("</"):
        return True
    if t in ("&lt;", "&gt;", "&amp;"):
        return True
    if re.fullmatch(r"<[^>]+>", t):
        return True
    if len(t) <= 2 and re.fullmatch(r"[\x00-\x7f]+", t):
        return True
    if re.fullmatch(r"\[\d+\]", t):
        return True
    if re.fullmatch(r"- \d+ -", t):
        return True
    if re.fullmatch(r"=+", t):
        return True
    if re.fullmatch(r"[0-9]+(\.[0-9]+)?", t):
        return True
    if len(t) >= 2 and set(t) == {"*"}:
        return True
    if "{" in t and re.fullmatch(r"[0-9A-Za-z\t \[\]{}]+", t):
        return True
    return False


def load_json(path: str) -> Dict[str, str]:
    if not os.path.isfile(path):
        return {}
    try:
        with open(path, encoding="utf-8") as f:
            o = json.load(f)
        return o if isinstance(o, dict) else {}
    except Exception:
        return {}


def needs_translation(
    key: str,
    en_val: str,
    prev_zh: Dict[str, str],
    *,
    include_named_keys: bool,
) -> bool:
    if not isinstance(en_val, str) or not en_val.strip():
        return False

    if RE_HASH_KEY.match(key):
        cur = prev_zh.get(key)
        if cur is None:
            return True
        if cur != en_val:
            return False
        # cur == en_val: only queue if a real localized string is still expected
        return not _hash_en_identity_ok_for_zh_hans(en_val)

    if not include_named_keys:
        return key not in prev_zh

    cur = prev_zh.get(key)
    return cur is None or cur == en_val


def iter_queue_rows(include_named_keys: bool) -> Iterator[Tuple[str, str, str]]:
    if not os.path.isdir(EN_DIR):
        return
    for fn in sorted(os.listdir(EN_DIR)):
        if not fn.endswith(".json"):
            continue
        en_path = os.path.join(EN_DIR, fn)
        zh_path = os.path.join(ZH_DIR, fn)
        en_map = load_json(en_path)
        prev_zh = load_json(zh_path)
        for k, en_val in en_map.items():
            if needs_translation(k, en_val, prev_zh, include_named_keys=include_named_keys):
                yield fn, k, en_val


def cmd_stats(args: argparse.Namespace) -> int:
    rows = list(iter_queue_rows(include_named_keys=args.include_named_keys))
    by_file: Dict[str, int] = {}
    for fn, _, _ in rows:
        by_file[fn] = by_file.get(fn, 0) + 1
    print(f"needs_translation: {len(rows)} entries")
    for fn in sorted(by_file):
        print(f"  {fn}: {by_file[fn]}")
    return 0


def cmd_queue(args: argparse.Namespace) -> int:
    rows = list(iter_queue_rows(include_named_keys=args.include_named_keys))
    out = args.output
    if out is None or out == "-":
        f = sys.stdout
        close_f = False
    else:
        os.makedirs(os.path.dirname(out) or ".", exist_ok=True)
        f = open(out, "w", encoding="utf-8")
        close_f = True

    try:
        for fn, k, en_val in rows:
            rec = {"file": fn, "key": k, "en": en_val}
            f.write(json.dumps(rec, ensure_ascii=False) + "\n")
    finally:
        if close_f:
            f.close()

    print(f"queue: {len(rows)} entries (hash + optional logical per rules)", file=sys.stderr)
    if args.summary:
        by_file: Dict[str, int] = {}
        for fn, _, _ in rows:
            by_file[fn] = by_file.get(fn, 0) + 1
        for fn in sorted(by_file):
            print(f"  {fn}: {by_file[fn]}", file=sys.stderr)
    return 0


def _merge_translations_into_zh(
    zh_path: str,
    en_map: Dict[str, str],
    updates: Dict[str, str],
    *,
    dry_run: bool,
) -> Tuple[int, List[str]]:
    prev = load_json(zh_path)
    errors: List[str] = []
    applied = 0
    for k, zh_val in updates.items():
        if k not in en_map:
            errors.append(f"unknown key in {os.path.basename(zh_path)}: {k}")
            continue
        if not isinstance(zh_val, str):
            errors.append(f"non-string zh for {k}")
            continue
        applied += 1
    if errors:
        return applied, errors

    zh_out: Dict[str, str] = {}
    for k in en_map:
        if k in updates:
            zh_out[k] = updates[k]
        elif k in prev:
            zh_out[k] = prev[k]
        else:
            zh_out[k] = en_map[k]

    if not dry_run:
        os.makedirs(os.path.dirname(zh_path) or ".", exist_ok=True)
        with open(zh_path, "w", encoding="utf-8") as wf:
            json.dump(dict(sorted(zh_out.items())), wf, ensure_ascii=False, indent=2)
            wf.write("\n")
    return applied, []


def load_apply_payload(path: str, *, base_file: str | None) -> Dict[str, Dict[str, str]]:
    with open(path, encoding="utf-8") as f:
        raw: Any = json.load(f)

    if isinstance(raw, dict) and base_file:
        if all(isinstance(v, str) for v in raw.values()):
            return {base_file: dict(raw)}

    if isinstance(raw, list):
        out: Dict[str, Dict[str, str]] = {}
        for item in raw:
            if not isinstance(item, dict):
                continue
            fn = item.get("file")
            k = item.get("key")
            zh = item.get("zh")
            if isinstance(fn, str) and isinstance(k, str) and isinstance(zh, str):
                out.setdefault(fn, {})[k] = zh
        return out

    if isinstance(raw, dict):
        first = next(iter(raw.values()), None)
        if first is None:
            return {}
        if isinstance(first, dict):
            return {str(k): dict(v) for k, v in raw.items() if isinstance(v, dict)}
        if isinstance(first, str) and all(isinstance(v, str) for v in raw.values()):
            raise ValueError(
                "flat key map is ambiguous across locale files. "
                "Use nested {\"scripts-quests.json\": {\"s....\": \"...\"}}, JSONL with file/key/zh, "
                "or pass apply --base-file scripts-quests.json when the JSON is a single-file map."
            )

    raise ValueError("unsupported JSON shape for apply payload")


def cmd_split_queue(args: argparse.Namespace) -> int:
    if not os.path.isfile(args.input):
        print(f"missing {args.input}", file=sys.stderr)
        return 1
    os.makedirs(args.output_dir, exist_ok=True)
    lines = open(args.input, encoding="utf-8").read().splitlines()
    n = max(1, int(args.lines))
    base = args.prefix
    chunk_idx = 0
    for i in range(0, len(lines), n):
        chunk_idx += 1
        part = lines[i : i + n]
        out_path = os.path.join(args.output_dir, f"{base}{chunk_idx:03d}.jsonl")
        with open(out_path, "w", encoding="utf-8") as wf:
            wf.write("\n".join(part))
            if part:
                wf.write("\n")
        print(f"wrote {out_path} ({len(part)} lines)", file=sys.stderr)
    print(f"split-queue: {len(lines)} lines -> {chunk_idx} file(s)", file=sys.stderr)
    return 0


def cmd_apply(args: argparse.Namespace) -> int:
    payload = load_apply_payload(args.input, base_file=args.base_file)
    total = 0
    all_errors: List[str] = []

    for fn, updates in sorted(payload.items()):
        if not fn.endswith(".json"):
            all_errors.append(f"skip non-json filename: {fn}")
            continue
        en_path = os.path.join(EN_DIR, fn)
        zh_path = os.path.join(ZH_DIR, fn)
        if not os.path.isfile(en_path):
            all_errors.append(f"missing en file for {fn}")
            continue
        en_map = load_json(en_path)
        n, errs = _merge_translations_into_zh(zh_path, en_map, updates, dry_run=args.dry_run)
        total += n
        all_errors.extend(errs)
        if not errs and not args.dry_run:
            print(f"apply: {fn} <- {len(updates)} keys (merged {n})", file=sys.stderr)

    if all_errors:
        for e in all_errors:
            print(f"error: {e}", file=sys.stderr)
        return 1

    print(f"apply: total keys written {total}" + (" (dry-run)" if args.dry_run else ""), file=sys.stderr)
    return 0


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = ap.add_subparsers(dest="cmd", required=True)

    q = sub.add_parser("queue", help="Write JSONL of {file,key,en} rows needing translation")
    q.add_argument(
        "-o",
        "--output",
        default=os.path.join(ROOT, "Data", "Localization", "tools-output", "llm-translation-queue.jsonl"),
        help="Output path (default: tools-output/llm-translation-queue.jsonl). Use - for stdout.",
    )
    q.add_argument(
        "--include-named-keys",
        action="store_true",
        help="Also queue logical (non-hash) keys when zh is missing or equals en (default: only missing).",
    )
    q.add_argument("--summary", action="store_true", help="Print per-file counts to stderr")
    q.set_defaults(func=cmd_queue)

    st = sub.add_parser("stats", help="Print counts of strings needing translation (no files written)")
    st.add_argument(
        "--include-named-keys",
        action="store_true",
        help="Same as queue: include logical keys where zh missing or equals en.",
    )
    st.set_defaults(func=cmd_stats)

    a = sub.add_parser("apply", help="Merge LLM translation JSON / JSONL into zh-Hans/*.json")
    a.add_argument("-i", "--input", required=True, help="Path to LLM response JSON or JSONL")
    a.add_argument(
        "--base-file",
        metavar="NAME.json",
        help="If the JSON is a flat {key: zh} map for one locale file, set that filename (e.g. scripts-quests.json).",
    )
    a.add_argument("--dry-run", action="store_true", help="Validate only; do not write zh-Hans")
    a.set_defaults(func=cmd_apply)

    s = sub.add_parser("split-queue", help="Split a JSONL queue into fixed-size batch files")
    s.add_argument("-i", "--input", required=True, help="Source JSONL from queue command")
    s.add_argument("--lines", type=int, default=80, help="Max lines per batch file (default: 80)")
    s.add_argument(
        "-o",
        "--output-dir",
        required=True,
        help="Directory for llm-batch-001.jsonl, ...",
    )
    s.add_argument("--prefix", default="llm-batch-", help="Filename prefix (default: llm-batch-)")
    s.set_defaults(func=cmd_split_queue)

    args = ap.parse_args()
    return int(args.func(args))


if __name__ == "__main__":
    raise SystemExit(main())
