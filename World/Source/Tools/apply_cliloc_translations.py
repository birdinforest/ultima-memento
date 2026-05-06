"""
Combine all group translation responses and apply them to the CSV file.
"""
import csv
import json
import os
import shutil
from pathlib import Path

CSV_PATH = Path("World/Documentation/CliLoc-cht.csv")
BACKUP_PATH = Path("World/Documentation/CliLoc-cht.csv.translated_backup")
RESPONSE_DIR = Path("World/Data/Localization/tools-output/cliloc-batches/responses")

def apply_translations():
    """Combine all group responses and apply to CSV."""
    # Collect all translations
    all_translations = {}
    response_files = sorted(RESPONSE_DIR.glob("response-*.json"))
    
    if not response_files:
        print("No response files found!")
        return
    
    print(f"Found {len(response_files)} response files")
    
    for fpath in response_files:
        with open(fpath, 'r', encoding='utf-8') as f:
            data = json.load(f)
        group_name = fpath.stem.replace("response-", "")
        print(f"  {group_name}: {len(data)} translations")
        all_translations.update(data)
    
    print(f"\nTotal translations to apply: {len(all_translations)}")
    
    # Create backup
    shutil.copy2(CSV_PATH, BACKUP_PATH)
    print(f"Backup created: {BACKUP_PATH}")
    
    # Read CSV
    with open(CSV_PATH, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    # Get header
    header = lines[0]
    rows = lines[1:]
    
    # Apply translations
    applied = 0
    not_found = 0
    unchanged = 0
    
    new_lines = [header]
    for line in rows:
        # Parse CSV line (simple - no embedded commas in our fields)
        parts = line.strip().split(',', 2)
        if len(parts) >= 2:
            num = parts[0].strip()
            if num in all_translations:
                orig_text = parts[1].strip()
                new_text = all_translations[num]
                if orig_text != new_text:
                    # Rebuild line with translated text
                    if len(parts) >= 3:
                        new_line = f"{num},{new_text},{parts[2]}\n"
                    else:
                        new_line = f"{num},{new_text}\n"
                    new_lines.append(new_line)
                    applied += 1
                else:
                    new_lines.append(line)
                    unchanged += 1
            else:
                new_lines.append(line)
                not_found += 1
        else:
            new_lines.append(line)
    
    # Write back
    with open(CSV_PATH, 'w', encoding='utf-8', newline='') as f:
        f.writelines(new_lines)
    
    print(f"\nResults:")
    print(f"  Applied: {applied}")
    print(f"  Unchanged (same text): {unchanged}")
    print(f"  Not found in CSV: {not_found}")
    print(f"  Total: {applied + unchanged + not_found}")
    
    return all_translations

def verify_translations():
    """Count remaining English entries in CSV."""
    import re
    remaining = 0
    total = 0
    
    with open(CSV_PATH, 'r', encoding='utf-8') as f:
        reader = csv.reader(f)
        header = next(reader)
        for row in reader:
            if len(row) >= 2:
                total += 1
                text = row[1].strip()
                if re.search(r'[A-Za-z]', text) and not re.search(r'[\u4e00-\u9fff]', text):
                    remaining += 1
    
    print(f"\nVerification:")
    print(f"  Total entries: {total}")
    print(f"  Remaining English-only: {remaining}")
    print(f"  Progress: {total - remaining}/{total} ({(total-remaining)/total*100:.1f}%)")
    
    return remaining

if __name__ == "__main__":
    import sys
    
    if "--apply" in sys.argv:
        apply_translations()
    elif "--verify" in sys.argv:
        verify_translations()
    elif "--both" in sys.argv:
        apply_translations()
        print("\n" + "="*50)
        verify_translations()
    else:
        print("Usage:")
        print("  python3 apply_cliloc_translations.py --apply    # Apply translations")
        print("  python3 apply_cliloc_translations.py --verify   # Verify progress")
        print("  python3 apply_cliloc_translations.py --both     # Apply + verify")
