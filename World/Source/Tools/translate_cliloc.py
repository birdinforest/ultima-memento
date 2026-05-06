"""
Batch translator for CliLoc-cht.csv.
Extracts all entries with English text, batches them for LLM translation,
and applies the translations back to the CSV.

Usage:
  1. Extract: python3 translate_cliloc.py --extract
  2. Translate: Provide batch files to LLM, save response
  3. Apply: python3 translate_cliloc.py --apply <response.json>
     Or apply all: python3 translate_cliloc.py --apply-all
"""
import csv
import json
import re
import os
import shutil
from pathlib import Path
from datetime import datetime

CSV_PATH = Path("World/Documentation/CliLoc-cht.csv")
BACKUP_PATH = Path("World/Documentation/CliLoc-cht.csv.bak")
BATCH_DIR = Path("World/Data/Localization/tools-output/cliloc-batches")
os.makedirs(BATCH_DIR, exist_ok=True)

def has_english(text: str) -> bool:
    return bool(re.search(r'[A-Za-z]', text))

def extract():
    """Extract all entries needing translation into numbered batches."""
    entries = []
    with open(CSV_PATH, 'r', encoding='utf-8') as f:
        reader = csv.reader(f)
        header = next(reader)
        for row in reader:
            if len(row) >= 2:
                number = row[0].strip()
                text = row[1].strip()
                flag = row[2].strip() if len(row) > 2 else ""
                if has_english(text):
                    entries.append({
                        "number": number,
                        "text": text,
                        "flag": flag,
                        "category": "english_only" if not re.search(r'[\u4e00-\u9fff]', text) else "mixed"
                    })

    print(f"Total entries needing translation: {len(entries)}")

    # Split into reasonable batches (sorted by number for logical grouping)
    entries.sort(key=lambda e: int(e["number"]))
    
    # Categorize by type
    html_entries = [e for e in entries if re.search(r'<BASEFONT|<BODY|<CENTER|<U>|<A HREF|<BR>', e["text"])]
    code_entries = [e for e in entries if not re.search(r'[\u4e00-\u9fff]', e["text"]) and len(e["text"]) < 30]  # short english-only code
    mixed_short = [e for e in entries if re.search(r'[\u4e00-\u9fff]', e["text"]) and len(e["text"]) < 80]
    mixed_long = [e for e in entries if re.search(r'[\u4e00-\u9fff]', e["text"]) and len(e["text"]) >= 80]
    pure_english_long = [e for e in entries if not re.search(r'[\u4e00-\u9fff]', e["text"]) and len(e["text"]) >= 30]
    
    print(f"HTML gump entries: {len(html_entries)}")
    print(f"Short code/english-only entries: {len(code_entries)}")
    print(f"Mixed short entries: {len(mixed_short)}")
    print(f"Mixed long entries: {len(mixed_long)}")
    print(f"Pure English long entries: {len(pure_english_long)}")
    
    # Write batches
    batch_size = 500
    batches = []
    all_sorted = sorted(entries, key=lambda e: int(e["number"]))
    
    for i in range(0, len(all_sorted), batch_size):
        batch = all_sorted[i:i + batch_size]
        batch_num = i // batch_size + 1
        batch_file = BATCH_DIR / f"batch-{batch_num:03d}.json"
        with open(batch_file, 'w', encoding='utf-8') as f:
            json.dump({
                "batch": batch_num,
                "count": len(batch),
                "entries": [{k: v for k, v in e.items()} for e in batch]
            }, f, ensure_ascii=False, indent=1)
        print(f"Batch {batch_num:03d}: {len(batch)} entries -> {batch_file}")
        batches.append(batch_file)
    
    # Write master index
    index = {
        "total": len(entries),
        "batches": len(batches),
        "batch_dir": str(BATCH_DIR),
        "created": datetime.now().isoformat()
    }
    index_file = BATCH_DIR / "_index.json"
    with open(index_file, 'w', encoding='utf-8') as f:
        json.dump(index, f, ensure_ascii=False, indent=2)
    print(f"\nMaster index: {index_file}")
    
    # Also write a condensed version for LLM with just keys
    for batch_file in batches:
        with open(batch_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        condensed = {}
        for e in data["entries"]:
            condensed[e["number"]] = e["text"]
        
        condensed_file = batch_file.with_suffix('.txt')
        with open(condensed_file, 'w', encoding='utf-8') as f:
            f.write(json.dumps(condensed, ensure_ascii=False, indent=1))
    
    print("\nDone. Ready for LLM translation.")

def apply_batch(batch_num, translation_map):
    """Apply translations from a specific batch back to CSV."""
    # Create backup if not exists
    if not BACKUP_PATH.exists():
        shutil.copy2(CSV_PATH, BACKUP_PATH)
        print(f"Backup created: {BACKUP_PATH}")
    
    # Read current CSV
    rows = []
    with open(CSV_PATH, 'r', encoding='utf-8') as f:
        reader = csv.reader(f)
        header = next(reader)
        rows = list(reader)
    
    # Apply translations
    applied = 0
    not_found = 0
    for i, row in enumerate(rows):
        if len(row) >= 2:
            num = row[0].strip()
            if num in translation_map:
                orig_text = row[1].strip()
                new_text = translation_map[num]
                if orig_text != new_text:
                    row[1] = new_text
                    applied += 1
    
    # Write back
    with open(CSV_PATH, 'w', encoding='utf-8', newline='') as f:
        writer = csv.writer(f)
        writer.writerow(header)
        writer.writerows(rows)
    
    print(f"Applied {applied} translations from batch {batch_num}.")
    return applied

def apply_all():
    """Apply all completed batch translations."""
    if not BACKUP_PATH.exists():
        shutil.copy2(CSV_PATH, BACKUP_PATH)
        print(f"Backup created: {BACKUP_PATH}")
    
    total = 0
    response_dir = BATCH_DIR / "responses"
    if not response_dir.exists():
        print(f"No response directory found at {response_dir}")
        return
    
    for fpath in sorted(response_dir.glob("response-*.json")):
        with open(fpath, 'r', encoding='utf-8') as f:
            data = json.load(f)
        batch_num = fpath.stem.replace("response-", "")
        applied = apply_batch(batch_num, data)
        total += applied
    
    print(f"\nTotal translations applied: {total}")

def check_progress():
    """Check translation progress."""
    index_file = BATCH_DIR / "_index.json"
    if not index_file.exists():
        print("No extraction run found. Run --extract first.")
        return
    
    with open(index_file, 'r') as f:
        index = json.load(f)
    
    total = index["total"]
    batches = index["batches"]
    
    response_dir = BATCH_DIR / "responses"
    os.makedirs(response_dir, exist_ok=True)
    completed = len(list(response_dir.glob("response-*.json")))
    
    print(f"Total entries: {total}")
    print(f"Total batches: {batches}")
    print(f"Completed batches: {completed}")
    print(f"Progress: {completed}/{batches} ({completed/batches*100:.1f}%)")
    
    # Preview first batch
    first_batch = BATCH_DIR / "batch-001.json"
    if first_batch.exists():
        with open(first_batch, 'r') as f:
            data = json.load(f)
        print(f"\nFirst batch contains {len(data['entries'])} entries")
        for e in data["entries"][:5]:
            print(f"  #{e['number']}: {e['text'][:80]}")

if __name__ == "__main__":
    import sys
    
    if "--extract" in sys.argv:
        extract()
    elif "--apply-batch" in sys.argv:
        idx = sys.argv.index("--apply-batch")
        batch_num = sys.argv[idx + 1]
        response_file = sys.argv[idx + 2] if len(sys.argv) > idx + 2 else None
        if response_file:
            with open(response_file, 'r') as f:
                data = json.load(f)
        else:
            batch_path = BATCH_DIR / f"batch-{batch_num}.json"
            response_path = BATCH_DIR / "responses" / f"response-{batch_num}.json"
            if response_path.exists():
                with open(response_path, 'r') as f:
                    data = json.load(f)
            else:
                print(f"Response file not found: {response_path}")
                sys.exit(1)
        apply_batch(batch_num, data)
    elif "--apply-all" in sys.argv:
        apply_all()
    elif "--progress" in sys.argv:
        check_progress()
    else:
        print("Usage:")
        print("  python3 translate_cliloc.py --extract           # Extract entries needing translation")
        print("  python3 translate_cliloc.py --apply-batch N     # Apply batch N translations")
        print("  python3 translate_cliloc.py --apply-all         # Apply all completed batches")
        print("  python3 translate_cliloc.py --progress          # Check progress")
