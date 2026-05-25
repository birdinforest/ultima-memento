#!/usr/bin/env python3
"""
Step 2: Finalize shotkey migration for scripts-mobiles.json.
Handles:
  (a) Apply LLM translations to world-player-text.json (en + zh-Hans)
  (b) Prune migrated hash keys from scripts-mobiles.json
  (c) Patch C# files: StringCatalog.Resolve → ResolveByKey, ResolveFormat → ResolveFormatByKey
"""
import json
import re
import os
import hashlib

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SCRIPTS_DIR = os.path.join(REPO, "Source", "Scripts", "Mobiles")
EN_SM_PATH = os.path.join(REPO, "Data", "Localization", "en", "scripts-mobiles.json")
ZH_SM_PATH = os.path.join(REPO, "Data", "Localization", "zh-Hans", "scripts-mobiles.json")
WPT_EN_PATH = os.path.join(REPO, "Data", "Localization", "en", "world-player-text.json")
WPT_ZH_PATH = os.path.join(REPO, "Data", "Localization", "zh-Hans", "world-player-text.json")
MAPPING_PATH = os.path.join(REPO, "Data", "Localization", "tools-output", "mobile-shotkey-mapping.json")
LLM_TRAN_PATH = os.path.join(REPO, "Data", "Localization", "tools-output", "mobile-llm-translation.json")
WPT_EN_DRAFT = os.path.join(REPO, "Data", "Localization", "tools-output", "mobile-wpt-en-draft.json")


def en_hash(en_text):
    """Compute the s. prefix + SHA-256 hash used by the extractor."""
    return "s." + hashlib.sha256(en_text.encode("utf-8")).hexdigest()[:16]


def load_json(p):
    if os.path.exists(p):
        with open(p, encoding="utf-8") as f:
            return json.load(f)
    return {}


def apply_llm_translations():
    """Apply LLM translations to the wpt drafts, then merge into world-player-text.json."""
    mapping = load_json(MAPPING_PATH)
    wpt_en = load_json(WPT_EN_DRAFT)
    wpt_zh = load_json(WPT_ZH_PATH)
    llm = load_json(LLM_TRAN_PATH)

    # Recover existing ZH: copy from scripts-mobiles via hash
    sm_zh = load_json(ZH_SM_PATH)
    sm_en = load_json(EN_SM_PATH)

    for hk, sk in mapping.items():
        if sk not in wpt_zh:
            wpt_zh[sk] = sm_zh.get(hk, sm_en.get(hk, ""))

    # Apply LLM translations (sk → zh_text)
    for sk, zh_text in llm.items():
        wpt_zh[sk] = zh_text
        wpt_en[sk] = dict(llm).get(sk, wpt_en.get(sk, ""))

    # Also ensure all EN values present
    for hk, sk in mapping.items():
        en_text = sm_en.get(hk, "")
        if sk not in wpt_en:
            wpt_en[sk] = en_text
        if sk not in wpt_zh:
            ba = en_text
            bare = en_text.strip()
            for art in ["the ", "an ", "a "]:
                if bare.startswith(art):
                    bare = bare[len(art):]
                    break
            if bare.strip() == en_text.strip():
                wpt_zh[sk] = en_text  # fallback
            else:
                wpt_zh[sk] = wpt_zh.get(sk, en_text)

    # Merge with existing world-player-text (existing keys take priority)
    existing_wpt_en = load_json(WPT_EN_PATH)
    existing_wpt_zh = load_json(WPT_ZH_PATH)

    for k, v in existing_wpt_en.items():
        wpt_en[k] = v
    for k, v in existing_wpt_zh.items():
        wpt_zh[k] = v

    # Write merged
    with open(WPT_EN_PATH, "w", encoding="utf-8") as f:
        json.dump(dict(sorted(wpt_en.items())), f, ensure_ascii=False, indent=2)
        f.write("\n")
    with open(WPT_ZH_PATH, "w", encoding="utf-8") as f:
        json.dump(dict(sorted(wpt_zh.items())), f, ensure_ascii=False, indent=2)
        f.write("\n")

    print(f"Applied LLM translations: {len(llm)}")
    print(f"EN world-player-text.json: {len(wpt_en)} keys")
    print(f"ZH world-player-text.json: {len(wpt_zh)} keys")


def prune_scripts_mobiles():
    """Remove migrated hash keys from scripts-mobiles.json."""
    mapping = load_json(MAPPING_PATH)
    en = load_json(EN_SM_PATH)
    zh = load_json(ZH_SM_PATH)

    n_en = len(en)
    n_zh = len(zh)

    for hk in mapping:
        en.pop(hk, None)
        zh.pop(hk, None)

    with open(EN_SM_PATH, "w", encoding="utf-8") as f:
        json.dump(en, f, ensure_ascii=False, indent=2)
        f.write("\n")
    with open(ZH_SM_PATH, "w", encoding="utf-8") as f:
        json.dump(zh, f, ensure_ascii=False, indent=2)
        f.write("\n")

    # Also clean up from other category files where hash keys may have been duplicated
    for cat in ["scripts-items.json", "scripts-system.json", "scripts-engines-and-systems.json", "scripts-books.json"]:
        for locale in ["en", "zh-Hans"]:
            p = os.path.join(os.path.dirname(EN_SM_PATH), "..", locale, cat)
            if os.path.exists(p):
                d = load_json(p)
                changed = False
                for hk in mapping:
                    if hk in d:
                        del d[hk]
                        changed = True
                if changed:
                    with open(p, "w", encoding="utf-8") as f:
                        json.dump(d, f, ensure_ascii=False, indent=2)
                        f.write("\n")
                    print(f"  Cleaned {hk} from {locale}/{cat}")

    print(f"EN scripts-mobiles.json: {n_en} → {len(en)} keys")
    print(f"ZH scripts-mobiles.json: {n_zh} → {len(zh)} keys")


def patch_cs_files():
    """
    Patch C# files: replace StringCatalog.Resolve(text) → StringCatalog.ResolveByKey(key)
    and StringCatalog.ResolveFormat(text, ...) → StringCatalog.ResolveFormatByKey(key, ...)
    
    Strategy: Build English-text → shotkey lookup, then scan each C# file.
    """
    mapping = load_json(MAPPING_PATH)
    en_sm = load_json(EN_SM_PATH)

    # Build reverse lookup: English text → shotkey
    text_to_key = {}
    for hk, sk in mapping.items():
        txt = en_sm.get(hk, "")
        if txt:
            text_to_key[txt] = sk

    print(f"Reverse lookup: {len(text_to_key)} texts → shotkeys")

    total_files = 0
    total_replaced = 0
    skipped_texts = set()

    for root, dirs, files in os.walk(SCRIPTS_DIR):
        for fn in files:
            if not fn.endswith(".cs"):
                continue
            path = os.path.join(root, fn)
            total_files += 1

            with open(path, encoding="utf-8") as f:
                content = f.read()

            original = content
            changed = False

            # Pattern 1: StringCatalog.Resolve(null, "EXACT TEXT")
            def replace_resolve(m):
                nonlocal changed
                full = m.group(0)
                # Extract the text literal
                inner = m.group(3) if m.group(3) else ""
                if not inner:
                    return full
                if inner not in text_to_key:
                    skipped_texts.add(inner)
                    return full
                sk = text_to_key[inner]
                prefix = m.group(1)  # e.g. "null" or "from.Account"
                # Determine if it's Resolve or ResolveFormat
                meth = m.group(2)  # "Resolve" or "ResolveFormat"
                args_after = m.group(4) if m.group(4) else ""  # e.g. ", arg0" 
                
                if meth == "ResolveFormat":
                    new = f'StringCatalog.ResolveFormatByKey({prefix}, "{sk}"{args_after})'
                else:
                    new = f'StringCatalog.ResolveByKey({prefix}, "{sk}")'
                changed = True
                return new

            # Use regex to find StringCatalog.Resolve( or StringCatalog.ResolveFormat(
            # Need to handle both Resolve(null, "text") and Resolve(account, "text")
            pattern = r'StringCatalog\.(Resolve(?:Format)?)\(\s*([^,]+?)\s*,\s*"((?:[^"\\]|\\.)*)"\s*((?:,\s*[^)]+)*)\s*\)'

            new_content = re.sub(pattern, replace_resolve, content)

            if new_content != original:
                with open(path, "w", encoding="utf-8") as f:
                    f.write(new_content)
                file_changes = re.findall(pattern, content)
                for m in re.finditer(pattern, content):
                    inner = m.group(3) if m.group(3) else ""
                    if inner in text_to_key:
                        total_replaced += 1
                print(f"  Patched: {os.path.relpath(path, REPO)}")
            elif changed:
                print(f"  Issues: {os.path.relpath(path, REPO)}")

    print(f"\nTotal files scanned: {total_files}")
    print(f"Total replacements: {total_replaced}")
    if skipped_texts:
        print(f"Unmatched texts ({len(skipped_texts)}):")
        for t in sorted(skipped_texts)[:20]:
            h = en_hash(t)
            in_mapping = h in mapping
            print(f"  '{t[:60]}'  hash={h} in_mapping={in_mapping}")
        if len(skipped_texts) > 20:
            print(f"  ... ({len(skipped_texts) - 20} more)")


def dry_run():
    """Report what would be patched without making changes."""
    mapping = load_json(MAPPING_PATH)
    en_sm = load_json(EN_SM_PATH)

    text_to_key = {}
    for hk, sk in mapping.items():
        txt = en_sm.get(hk, "")
        if txt:
            text_to_key[txt] = sk
    print(f"Reverse lookup: {len(text_to_key)} texts → shotkeys")

    # Count occurrences per file
    pattern = r'StringCatalog\.(Resolve(?:Format)?)\(\s*([^,]+?)\s*,\s*"((?:[^"\\]|\\.)*)"'

    file_counts = {}
    for root, dirs, files in os.walk(SCRIPTS_DIR):
        for fn in files:
            if not fn.endswith(".cs"):
                continue
            path = os.path.join(root, fn)
            rel = os.path.relpath(path, REPO)
            with open(path, encoding="utf-8") as f:
                content = f.read()
            
            matched = 0
            unmatched = 0
            for m in re.finditer(pattern, content):
                inner = m.group(3) if m.group(3) else ""
                if inner in text_to_key:
                    matched += 1
                else:
                    # Check if it's a hash key reference (ResolveByKey)
                    if inner.startswith("mob."):
                        continue  # already a shotkey
                    if inner.startswith("s."):
                        continue  # already hash-based
                    if inner.startswith(("prop.", "item.", "quest.", "book.", "eng.", "sys.")):
                        continue  # already a named key
                    unmatched += 1
            
            if matched:
                file_counts[rel] = (matched, unmatched)

    total_matched = sum(c[0] for c in file_counts.values())
    total_unmatched = sum(c[1] for c in file_counts.values())
    
    print(f"\nFiles to patch: {len(file_counts)}")
    print(f"Total matched Resolve calls: {total_matched}")
    print(f"Unmatched (not in our shotkey mapping): {total_unmatched}")
    for rel, (m, u) in sorted(file_counts.items()):
        if u > 0:
            print(f"  {rel}: {m} match, {u} UNMATCHED")
        else:
            print(f"  {rel}: {m} match")


if __name__ == "__main__":
    import sys
    if len(sys.argv) > 1:
        cmd = sys.argv[1]
        if cmd == "apply-llm":
            apply_llm_translations()
        elif cmd == "prune":
            prune_scripts_mobiles()
        elif cmd == "patch-cs":
            patch_cs_files()
        elif cmd == "dry-run":
            dry_run()
        elif cmd == "all":
            print("=== Step A: Apply LLM translations ===")
            apply_llm_translations()
            print()
            print("=== Step B: Prune scripts-mobiles.json ===")
            prune_scripts_mobiles()
            print()
            print("=== Step C: Dry-run C# patch ===")
            dry_run()
        else:
            print(f"Unknown command: {cmd}")
            print("Usage: build_mobile_shotkeys.py [apply-llm|prune|patch-cs|dry-run|all]")
    else:
        apply_llm_translations()
        print("\n---")
        prune_scripts_mobiles()
        print("\n---")
        dry_run()
