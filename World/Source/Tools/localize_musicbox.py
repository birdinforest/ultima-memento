"""
Fix m.SendMessage("SongName") in MusicBox.cs that were skipped due to Mplay += 1 on same line.
Usage: python3 World/Source/Tools/localize_musicbox.py
"""

import re
import os

REPO_ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
PATH = os.path.join(REPO_ROOT, "Source", "Scripts", "Items", "Misc", "MusicBox.cs")

with open(PATH, "r", encoding="utf-8", errors="replace") as f:
    content = f.read()

original = content

# Pattern: m.SendMessage("songname"); Mplay = Mplay + 1;
# Also catches single-line else if pattern
SENDMSG_SONG = re.compile(
    r'(m\.SendMessage\s*\(\s*)"((?:[^\\"]|\\.)*)"(\s*\);\s*Mplay)'
)

def _replace(m):
    prefix = m.group(1)
    text = m.group(2)
    suffix = m.group(3)
    return f'{prefix}StringCatalog.Resolve(m.Account, "{text}"){suffix}'

content = SENDMSG_SONG.sub(_replace, content)

if content != original:
    with open(PATH, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"MODIFIED: {PATH}")
else:
    print(f"UNCHANGED: {PATH}")
