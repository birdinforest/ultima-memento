#!/usr/bin/env python3
"""Normalize locale JSON files with a curated glossary and locale-specific rules.

Default zh-Hans workflow:
  python3 World/Source/Tools/sync_localization_glossary.py

What it does:
  1. Normalize bracketed proper nouns: 【Sosaria】 -> 【索萨利亚】
  2. Normalize whole-value glossary variants: "蒙托尔" -> "蒙托城"
  3. Apply locale-specific literal replacements from a rules JSON
  4. Apply exact value overrides for context-sensitive strings

Use --check for CI or post-build verification.
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]
LOCALIZATION_ROOT = ROOT / "Data" / "Localization"
BRACKET_RE = re.compile(r"【([^】]+)】")


def load_json(path: Path) -> dict:
    with path.open(encoding="utf-8") as f:
        data = json.load(f)
    if not isinstance(data, dict):
        raise ValueError(f"{path} must contain a top-level JSON object")
    return data


def write_json(path: Path, obj: dict) -> None:
    path.write_text(json.dumps(obj, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def build_variant_map(glossary: dict) -> tuple[dict[str, str], dict[str, list[str]]]:
    mapping: dict[str, str] = {}
    conflicts: dict[str, list[str]] = {}
    terms = glossary.get("terms", {})
    if not isinstance(terms, dict):
        raise ValueError("glossary 'terms' must be a JSON object")

    canonical_owners: dict[str, set[str]] = {}
    for english_term, entry in terms.items():
        if not isinstance(entry, dict):
            continue
        canonical = entry.get("canonical")
        if isinstance(canonical, str) and canonical:
            canonical_owners.setdefault(canonical, set()).add(english_term)

    for english_term, entry in terms.items():
        if not isinstance(entry, dict):
            continue

        canonical = entry.get("canonical")
        if not isinstance(canonical, str) or not canonical:
            continue

        variants = [english_term]
        alternatives = entry.get("alternatives", [])
        if isinstance(alternatives, list):
            variants.extend(v for v in alternatives if isinstance(v, str) and v)

        for variant in variants:
            if not variant or variant == canonical:
                continue

            owners = canonical_owners.get(variant, set())
            if owners and owners != {english_term}:
                other_canonicals = sorted(owners)
                conflicts[variant] = other_canonicals
                mapping.pop(variant, None)
                continue

            previous = mapping.get(variant)
            if previous is None:
                if variant not in conflicts:
                    mapping[variant] = canonical
                continue

            if previous == canonical:
                continue

            conflicts[variant] = sorted({previous, canonical})
            mapping.pop(variant, None)

    return mapping, conflicts


def load_rules(path: Path) -> dict:
    if not path.is_file():
        return {"literal_replacements": [], "exact_value_overrides": []}
    rules = load_json(path)
    rules.setdefault("literal_replacements", [])
    rules.setdefault("exact_value_overrides", [])
    if not isinstance(rules["literal_replacements"], list):
        raise ValueError("'literal_replacements' must be a list")
    if not isinstance(rules["exact_value_overrides"], list):
        raise ValueError("'exact_value_overrides' must be a list")
    return rules


def rule_matches(rule: dict, file_name: str, key: str) -> bool:
    single_file = rule.get("file")
    if single_file is not None:
        if not isinstance(single_file, str):
            raise ValueError(f"invalid file filter in rule: {rule}")
        if file_name != single_file:
            return False

    single_key = rule.get("key")
    if single_key is not None:
        if not isinstance(single_key, str):
            raise ValueError(f"invalid key filter in rule: {rule}")
        if key != single_key:
            return False

    files = rule.get("files")
    if files is not None:
        if not isinstance(files, list) or any(not isinstance(v, str) for v in files):
            raise ValueError(f"invalid files filter in rule: {rule}")
        if file_name not in files:
            return False

    keys = rule.get("keys")
    if keys is not None:
        if not isinstance(keys, list) or any(not isinstance(v, str) for v in keys):
            raise ValueError(f"invalid keys filter in rule: {rule}")
        if key not in keys:
            return False

    return True


def normalize_brackets(value: str, mapping: dict[str, str]) -> tuple[str, int]:
    count = 0

    def repl(match: re.Match[str]) -> str:
        nonlocal count
        inner = match.group(1)
        canonical = mapping.get(inner)
        if canonical and canonical != inner:
            count += 1
            return f"【{canonical}】"
        return match.group(0)

    return BRACKET_RE.sub(repl, value), count


def apply_literal_rules(
    value: str,
    file_name: str,
    key: str,
    rules: list[dict],
) -> tuple[str, int]:
    replacements = 0
    for rule in rules:
        if not rule_matches(rule, file_name, key):
            continue
        source = rule.get("from")
        target = rule.get("to")
        if not isinstance(source, str) or not isinstance(target, str):
            raise ValueError(f"literal replacement requires string from/to: {rule}")
        if not source or source == target or source not in value:
            continue
        replacements += value.count(source)
        value = value.replace(source, target)
    return value, replacements


def apply_overrides(
    value: str,
    file_name: str,
    key: str,
    overrides: list[dict],
) -> tuple[str, int]:
    for rule in overrides:
        if not rule_matches(rule, file_name, key):
            continue
        target = rule.get("value")
        if not isinstance(target, str):
            raise ValueError(f"exact override requires string value: {rule}")
        if value != target:
            return target, 1
        return value, 0
    return value, 0


def iter_target_files(locale_dir: Path, selected_files: list[str]) -> list[Path]:
    if selected_files:
        resolved: list[Path] = []
        for name in selected_files:
            path = Path(name)
            if not path.is_absolute():
                path = locale_dir / name
            if not path.is_file():
                raise FileNotFoundError(path)
            resolved.append(path)
        return resolved
    return sorted(path for path in locale_dir.glob("*.json") if path.is_file())


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--locale",
        default="zh-Hans",
        help="Locale directory under Data/Localization (default: zh-Hans).",
    )
    parser.add_argument(
        "--glossary",
        help="Path to curated glossary JSON. Default: Data/Localization/glossary-approved-zh.json",
    )
    parser.add_argument(
        "--rules",
        help="Path to locale-specific sync rules JSON. Default: Data/Localization/<locale>-glossary-sync-rules.json",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Report changes without writing files.",
    )
    parser.add_argument(
        "--check",
        action="store_true",
        help="Exit non-zero if any file would change; does not write files.",
    )
    parser.add_argument(
        "--verbose",
        action="store_true",
        help="Print changed keys for each file.",
    )
    parser.add_argument(
        "files",
        nargs="*",
        help="Optional locale JSON files to process (basename or absolute path).",
    )
    args = parser.parse_args()

    if args.dry_run and args.check:
        parser.error("--dry-run and --check are mutually exclusive")

    locale_dir = LOCALIZATION_ROOT / args.locale
    if not locale_dir.is_dir():
        print(f"missing locale directory: {locale_dir}", file=sys.stderr)
        return 1

    glossary_path = Path(args.glossary) if args.glossary else LOCALIZATION_ROOT / "glossary-approved-zh.json"
    rules_path = Path(args.rules) if args.rules else LOCALIZATION_ROOT / f"{args.locale}-glossary-sync-rules.json"

    glossary = load_json(glossary_path)
    rules = load_rules(rules_path)
    variant_map, conflicts = build_variant_map(glossary)

    if conflicts:
        print(
            f"warning: skipped {len(conflicts)} conflicting glossary variants",
            file=sys.stderr,
        )
        for variant, canonicals in sorted(conflicts.items())[:20]:
            print(f"  {variant!r} -> {canonicals}", file=sys.stderr)

    literal_rules = sorted(
        (rule for rule in rules["literal_replacements"] if isinstance(rule, dict)),
        key=lambda rule: len(rule.get("from", "")),
        reverse=True,
    )
    overrides = [rule for rule in rules["exact_value_overrides"] if isinstance(rule, dict)]

    changed_files = 0
    changed_keys_total = 0
    files = iter_target_files(locale_dir, args.files)

    for path in files:
        data = load_json(path)
        file_name = path.name
        file_changed = False
        file_changed_keys: list[str] = []
        whole_count = 0
        bracket_count = 0
        literal_count = 0
        override_count = 0

        for key, value in list(data.items()):
            if not isinstance(value, str):
                continue

            original = value

            canonical = variant_map.get(value)
            if canonical and canonical != value:
                value = canonical
                whole_count += 1

            value, bracket_hits = normalize_brackets(value, variant_map)
            bracket_count += bracket_hits

            value, literal_hits = apply_literal_rules(value, file_name, key, literal_rules)
            literal_count += literal_hits

            value, override_hits = apply_overrides(value, file_name, key, overrides)
            override_count += override_hits

            if value != original:
                data[key] = value
                file_changed = True
                file_changed_keys.append(key)

        if not file_changed:
            print(f"{file_name}: no changes")
            continue

        changed_files += 1
        changed_keys_total += len(file_changed_keys)
        print(
            f"{file_name}: changed_keys={len(file_changed_keys)} "
            f"whole={whole_count} bracket={bracket_count} "
            f"literal={literal_count} override={override_count}"
        )
        if args.verbose:
            for key in file_changed_keys:
                print(f"  {key}")

        if not args.dry_run and not args.check:
            write_json(path, data)

    print(
        f"summary: files={len(files)} changed_files={changed_files} changed_keys={changed_keys_total}"
    )

    if args.check and changed_files:
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
