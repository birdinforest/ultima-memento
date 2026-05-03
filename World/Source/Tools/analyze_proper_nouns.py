#!/usr/bin/env python3
"""
Analyze zh-Hans JSON files: extract proper nouns, cross-reference glossary,
and identify what needs English-parenthesis annotation.
Usage: python3 World/Source/Tools/analyze_proper_nouns.py
"""

import json, re, os, sys
from collections import defaultdict

LOCALE_DIR = os.path.join(os.path.dirname(__file__), '../../Data/Localization')
if not os.path.isdir(LOCALE_DIR):
    sys.stderr.write(f"Cannot find {LOCALE_DIR}\n")
    sys.exit(1)

GLOSSARY_PATH = os.path.join(LOCALE_DIR, 'glossary-approved-zh.json')

def load_glossary():
    with open(GLOSSARY_PATH, 'r', encoding='utf-8') as f:
        raw = json.load(f)
    # Entries are nested under "terms" key; top-level has metadata keys
    data = raw.get("terms", raw)
    # Build forward map: Chinese (canonical + alternatives) -> English headword
    zh_to_en = {}
    en_to_zh = {}
    for key, val in data.items():
        if key.startswith('_'):
            continue
        if isinstance(val, dict) and 'canonical' in val:
            canonical = val['canonical']
            zh_to_en[canonical] = key
            en_to_zh[key] = canonical
            for alt in val.get('alternatives', []):
                if alt not in zh_to_en:  # don't overwrite canonical
                    zh_to_en[alt] = key
    return zh_to_en, en_to_zh, data

def extract_bracketed(text):
    """Extract all 【...】 bracketed terms from Chinese text."""
    return re.findall(r'【([^】]+)】', text)

def analyze_commontalk(zh_to_en, en_to_zh, glossary_data):
    """Analyze commontalk-fragment-zh.json (keys = English, values = Chinese)."""
    path = os.path.join(LOCALE_DIR, 'commontalk-fragment-zh.json')
    with open(path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    results = {'bracketed': [], 'embedded': [], 'missing_from_glossary': set()}
    
    for en_key, zh_val in data.items():
        # Check for 【】bracketed terms
        bracketed = extract_bracketed(zh_val)
        for term in bracketed:
            if term in zh_to_en:
                results['bracketed'].append({
                    'en': en_key,
                    'zh': term,
                    'en_proper': zh_to_en[term],
                    'full_zh': zh_val
                })
            else:
                results['missing_from_glossary'].add(term)
                results['bracketed'].append({
                    'en': en_key,
                    'zh': term,
                    'en_proper': '?',
                    'full_zh': zh_val
                })
        
        # Check for embedded proper nouns (not in brackets)
        for zh_name in sorted(zh_to_en.keys(), key=len, reverse=True):
            if zh_name in zh_val and f'【{zh_name}】' not in zh_val:
                # Check it's not part of a longer 【】 bracket
                context = zh_val
                idx = context.find(zh_name)
                # Simple heuristic: not already in 【】
                if idx >= 0:
                    pre_chars = context[max(0,idx-1):idx]
                    post_chars = context[idx+len(zh_name):idx+len(zh_name)+1]
                    if pre_chars != '【' and post_chars != '】':
                        results['embedded'].append({
                            'en': en_key,
                            'zh': zh_name,
                            'en_proper': zh_to_en[zh_name],
                            'context': zh_val,
                            'position': idx
                        })
    
    return results

def analyze_paired(en_file, zh_file, zh_to_en, en_to_zh, glossary_data):
    """Analyze paired en/zh-Hans files (same hash-key structure)."""
    en_path = os.path.join(LOCALE_DIR, 'en', en_file)
    zh_path = os.path.join(LOCALE_DIR, 'zh-Hans', zh_file)
    
    if not os.path.exists(en_path) or not os.path.exists(zh_path):
        return None
    
    with open(en_path, 'r', encoding='utf-8') as f:
        en_data = json.load(f)
    with open(zh_path, 'r', encoding='utf-8') as f:
        zh_data = json.load(f)
    
    results = {
        'file': zh_file,
        'bracketed': [],
        'missing_from_glossary': set(),
        'missing_in_en': []
    }
    
    for key, zh_val in zh_data.items():
        if key not in en_data:
            results['missing_in_en'].append(key)
            continue
        en_val = en_data[key]
        
        bracketed = extract_bracketed(zh_val)
        for term in bracketed:
            if term in zh_to_en:
                results['bracketed'].append({
                    'key': key,
                    'zh': term,
                    'en_proper': zh_to_en[term],
                    'en_val': en_val[:120] + ('...' if len(en_val) > 120 else ''),
                    'zh_val': zh_val[:120] + ('...' if len(zh_val) > 120 else '')
                })
            else:
                results['missing_from_glossary'].add(term)
                results['bracketed'].append({
                    'key': key,
                    'zh': term,
                    'en_proper': '?',
                    'en_val': en_val[:120] + ('...' if len(en_val) > 120 else ''),
                    'zh_val': zh_val[:120] + ('...' if len(zh_val) > 120 else '')
                })
    
    results['missing_from_glossary'] = sorted(results['missing_from_glossary'])
    return results

def main():
    zh_to_en, en_to_zh, glossary_data = load_glossary()
    print(f"Loaded {len(zh_to_en)} Chinese→English mappings from glossary")
    print(f"Glossary headwords: {len(en_to_zh)}")
    
    # === 1. Analyze commontalk ===
    print("\n" + "=" * 70)
    print("1. COMMONTALK-FRAGMENT-ZH.JSON ANALYSIS")
    print("=" * 70)
    ct_results = analyze_commontalk(zh_to_en, en_to_zh, glossary_data)
    
    if ct_results['bracketed']:
        print(f"\n【】Bracketed terms found: {len(ct_results['bracketed'])}")
        for r in ct_results['bracketed'][:20]:
            print(f"  [{r['zh']}] → EN: {r['en_proper']} (from key: {r['en'][:80]}...)")
    
    if ct_results['embedded']:
        print(f"\nEmbedded (non-bracketed) proper nouns found: {len(ct_results['embedded'])}")
        for r in ct_results['embedded'][:20]:
            print(f"  '{r['zh']}' → '{r['en_proper']}' in: {r['context'][:80]}...")
    
    if ct_results['missing_from_glossary']:
        print(f"\n⚠ Missing from glossary ({len(ct_results['missing_from_glossary'])}):")
        for t in sorted(ct_results['missing_from_glossary']):
            print(f"  {t}")
    
    # === 2. Analyze paired files (scripts-books etc.) ===
    pairs = [
        ('scripts-books.json', 'scripts-books.json'),
        ('scripts-engines-and-systems.json', 'scripts-engines-and-systems.json'),
        ('scripts-quests.json', 'scripts-quests.json'),
        ('scripts-items.json', 'scripts-items.json'),
        ('scripts-mobiles.json', 'scripts-mobiles.json'),
    ]
    
    for en_file, zh_file in pairs:
        print(f"\n{'=' * 70}")
        print(f"2. ANALYZING: {zh_file}")
        print("=" * 70)
        results = analyze_paired(en_file, zh_file, zh_to_en, en_to_zh, glossary_data)
        if results is None:
            print("  (file not found, skipping)")
            continue
        
        if results['bracketed']:
            print(f"  【】Bracketed terms: {len(results['bracketed'])}")
            for r in results['bracketed'][:30]:
                en_status = "" if r['en_proper'] == '?' else f"✓ EN={r['en_proper']}"
                print(f"    【{r['zh']}】{en_status}")
                print(f"      EN val: {r['en_val'][:100]}")
                print(f"      ZH val: {r['zh_val'][:100]}")
        
        if results['missing_from_glossary']:
            print(f"\n  ⚠ Missing from glossary:")
            for t in results['missing_from_glossary']:
                print(f"    {t}")
        
        if results['missing_in_en']:
            print(f"\n  ⚠ Keys in zh but not in en: {len(results['missing_in_en'])}")
    
    print("\n" + "=" * 70)
    print("DONE")
    print("=" * 70)

if __name__ == '__main__':
    main()
