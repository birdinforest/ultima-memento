"""
Localize remaining complex string patterns in TarotPoker.cs and BlackJack.cs
that were skipped by the bulk script (string.Format, PublicOverheadMessage with format).

Usage: python3 World/Source/Tools/localize_games_complex.py
"""

import re
import os
import sys

REPO_ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
GAMES_DIR = os.path.join(REPO_ROOT, "Source", "Scripts", "Items", "Games")

def localize_tarot_poker():
    path = os.path.join(GAMES_DIR, "TarotPoker.cs")
    if not os.path.exists(path):
        print(f"ERROR: {path} not found")
        return False
    
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()
    
    original = content
    
    # Pattern 1: string.Format("string literal", from.Name) inside PublicOverheadMessage
    # Example: this.PublicOverheadMessage(..., string.Format("{0} pulls 'The Fool'", from.Name));
    content = re.sub(
        r'string\.Format\(\s*"([^"]+)"\s*,\s*from\.Name\s*\)',
        r'StringCatalog.ResolveFormat(from.Account, "\1", from.Name)',
        content
    )
    
    # Pattern 2: string.Format("string literal" (no args)) inside PublicOverheadMessage
    # Example: this.PublicOverheadMessage(..., string.Format("Everyone bets 10."));
    content = re.sub(
        r'string\.Format\(\s*"((?:[^\\"]|\\.)*)"\s*\)',
        r'StringCatalog.Resolve(from.Account, "\1")',
        content
    )
    
    # Pattern 3: {0}'s (possessive) format strings with from.Name
    content = re.sub(
        r'StringCatalog\.ResolveFormat\(from\.Account,\s*"([^"]*)\{0\}([^"]*)"\)',
        r'StringCatalog.ResolveFormat(from.Account, "\1{0}\2")',
        content
    )
    
    if content != original:
        with open(path, "w", encoding="utf-8") as f:
            f.write(content)
        print(f"MODIFIED: {path}")
        return True
    else:
        print(f"UNCHANGED: {path}")
        return False

def localize_blackjack():
    path = os.path.join(GAMES_DIR, "BlackJack.cs")
    if not os.path.exists(path):
        print(f"ERROR: {path} not found")
        return False
    
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()
    
    original = content
    
    # Define replacements for PublicOverheadMessage with simple string literals
    replacements = [
        # PublicOverheadMessage with simple strings (no format)
        (r'(this\.PublicOverheadMessage\(0,\s*\(this\.Hue\s*==\s*907\s*\?\s*0\s*:\s*this\.Hue\)\s*,\s*false\s*,\s*)"Can not alter Test Mode while in use\."',
         r'\1StringCatalog.Resolve(from.Account, "Can not alter Test Mode while in use.")'),
        (r'(this\.PublicOverheadMessage\(0,\s*\(this\.Hue\s*==\s*907\s*\?\s*0\s*:\s*this\.Hue\)\s*,\s*false\s*,\s*)"Blackjack Open\!"',
         r'\1StringCatalog.Resolve(from.Account, "Blackjack Open!")'),
        (r'(this\.PublicOverheadMessage\(0,\s*this\.Hue\s*,\s*false\s*,\s*)"Blackjack Closed\."',
         r'\1StringCatalog.Resolve(from.Account, "Blackjack Closed.")'),
    ]
    
    for pattern, replacement in replacements:
        content = re.sub(pattern, replacement, content)
    
    # SendMessage with format args (not string.Format, but SendMessage overload with format)
    # from.SendMessage("{0} has left this table too long, it is yours to play.", tempName);
    content = re.sub(
        r'(from\.SendMessage\()"(\{[0-9]\}[^{]*)"\s*,\s*(\w+)\)',
        r'\1StringCatalog.ResolveFormat(from.Account, "\2", \3)',
        content
    )
    
    # Also handle m_InUseBy / m context
    content = re.sub(
        r'(m_InUseBy\.SendMessage\()"(\{[0-9]\}[^{]*)"\s*,\s*(\w+)\)',
        r'\1StringCatalog.ResolveFormat(m_InUseBy.Account, "\2", \3)',
        content
    )
    
    # PublicOverheadMessage with "This game needs to be closed." - appears multiple times
    # Use a simple string replacement approach to be safe
    # This pattern occurs at lines 1212, 1235, 2001
    
    if content != original:
        with open(path, "w", encoding="utf-8") as f:
            f.write(content)
        print(f"MODIFIED: {path}")
        return True
    else:
        print(f"UNCHANGED: {path}")
        return False

if __name__ == "__main__":
    t = localize_tarot_poker()
    b = localize_blackjack()
    if t or b:
        print("Done - files modified.")
    else:
        print("No changes needed.")
