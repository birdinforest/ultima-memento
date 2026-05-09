#!/usr/bin/env python3
"""Fill Data/Localization/zh-Hans/*.json from en/*.json using machine translation.

By default (incremental), only sends English to the translator when:
  - the key is missing in zh-Hans, or
  - the current zh value equals the en value (typical placeholder after build_localization_strings.py).

Use --full to re-translate every string (overwrites existing Chinese; use with care).

Requires: pip install deep-translator
"""
from __future__ import annotations

import argparse
import json
import os
import re
import sys
import time
from typing import Dict, List, Tuple

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
EN_DIR = os.path.join(ROOT, "Data", "Localization", "en")
ZH_DIR = os.path.join(ROOT, "Data", "Localization", "zh-Hans")

# Keys that are not hash-based English blobs — usually already hand-tuned; skip unless --full
RE_HASH_KEY = re.compile(r"^s\.[0-9a-f]{16}$")
RE_ASCII_WORD = re.compile(r"[A-Za-z]{2,}")
RE_HTML_TAG = re.compile(r"(<[^>]+>)")
RE_PLACEHOLDER = re.compile(r"(\{[^{}]+\}|%\w|\[[0-9]+\])")
def has_meaningful_english(text: str) -> bool:
    return bool(RE_ASCII_WORD.search(text))


def is_markup_only(text: str) -> bool:
    stripped = text.strip()
    if not stripped:
        return True
    if "<" in stripped and ">" in stripped:
        without_tags = RE_HTML_TAG.sub("", stripped).strip()
        if not without_tags:
            return True
    if not has_meaningful_english(stripped):
        return True
    return False


def protect_placeholders(text: str) -> tuple[str, dict[str, str]]:
    replacements: dict[str, str] = {}

    def repl(match: re.Match[str]) -> str:
        token = f"ZXQPH{len(replacements)}QXZ"
        replacements[token] = match.group(0)
        return token

    return RE_PLACEHOLDER.sub(repl, text), replacements


def restore_placeholders(text: str, replacements: dict[str, str]) -> str:
    for token, original in replacements.items():
        text = text.replace(token, original)
    return text


def normalize_translation(original: str, translated: str) -> str:
    translated = translated.strip()
    if not translated:
        return original
    # The translator sometimes inserts spaces into our placeholder tokens.
    translated = re.sub(r"Z\s*X\s*Q\s*P\s*H\s*(\d+)\s*Q\s*X\s*Z", r"ZXQPH\1QXZ", translated)
    return translated


def translate_text(translator, text: str) -> str:
    if is_markup_only(text):
        return text

    protected, replacements = protect_placeholders(text)
    try:
        translated = translator.translate(protected)
    except Exception:
        return text

    translated = normalize_translation(text, translated)
    translated = restore_placeholders(translated, replacements)
    return translated or text


def translate_value(translator, value: str) -> str:
    if is_markup_only(value):
        return value

    if "<" not in value or ">" not in value:
        return translate_text(translator, value)

    parts = RE_HTML_TAG.split(value)
    translated_parts: list[str] = []
    for part in parts:
        if not part:
            continue
        if RE_HTML_TAG.fullmatch(part):
            translated_parts.append(part)
        else:
            translated_parts.append(translate_text(translator, part))
    return "".join(translated_parts)


def translate_batch(vals: list[str]) -> list[str]:
    from deep_translator import GoogleTranslator

    t = GoogleTranslator(source="en", target="zh-CN")
    out: list[str] = []
    for i, value in enumerate(vals, start=1):
        out.append(translate_value(t, value))
        if i % 25 == 0 or i == len(vals):
            print(f"translated {i}/{len(vals)}")
        time.sleep(0.1)
    return out


def load_zh(path: str) -> Dict[str, str]:
    if not os.path.isfile(path):
        return {}
    try:
        with open(path, encoding="utf-8") as f:
            return json.load(f)
    except Exception:
        return {}


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument(
        "--full",
        action="store_true",
        help="Re-translate all keys from en (legacy behavior; overwrites zh-Hans).",
    )
    ap.add_argument(
        "--include-named-keys",
        action="store_true",
        help="In incremental mode, also (re)translate logical keys (not s.<hash>), if they match the filter.",
    )
    args = ap.parse_args()

    if not os.path.isdir(EN_DIR):
        print(f"missing {EN_DIR}", file=sys.stderr)
        return 1

    os.makedirs(ZH_DIR, exist_ok=True)

    for fn in sorted(os.listdir(EN_DIR)):
        if not fn.endswith(".json"):
            continue
        en_path = os.path.join(EN_DIR, fn)
        zh_path = os.path.join(ZH_DIR, fn)
        with open(en_path, encoding="utf-8") as f:
            en: dict[str, str] = json.load(f)

        if not en:
            with open(zh_path, "w", encoding="utf-8") as f:
                json.dump({}, f, ensure_ascii=False, indent=2, sort_keys=True)
                f.write("\n")
            continue

        prev_zh = load_zh(zh_path)

        if args.full:
            keys = list(en.keys())
            vals = [en[k] for k in keys]
            translated = translate_batch(vals)
            zh_out = {k: (z or en[k]) for k, z in zip(keys, translated)}
        else:
            to_translate: List[Tuple[str, str]] = []
            for k, en_val in en.items():
                if not args.include_named_keys and not RE_HASH_KEY.match(k):
                    # Keep existing zh for logical keys (e.g. books.dynamic.*) unless missing
                    if k in prev_zh:
                        continue
                cur = prev_zh.get(k)
                if cur is None or cur == en_val:
                    to_translate.append((k, en_val))

            if to_translate:
                print(f"{fn}: incremental translate {len(to_translate)}/{len(en)} keys")
                zh_vals = translate_batch([v for _, v in to_translate])
                updates = {k: (z or en[k]) for (k, _), z in zip(to_translate, zh_vals)}
            else:
                print(f"{fn}: nothing to translate (incremental)")
                updates = {}

            zh_out: Dict[str, str] = {}
            for k, en_val in en.items():
                if k in updates:
                    zh_out[k] = updates[k]
                elif k in prev_zh:
                    zh_out[k] = prev_zh[k]
                else:
                    zh_out[k] = en_val

        with open(zh_path, "w", encoding="utf-8") as f:
            json.dump(dict(sorted(zh_out.items())), f, ensure_ascii=False, indent=2, sort_keys=True)
            f.write("\n")
        print(f"wrote {zh_path}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
