"""
Localize from.Say() in Behavior.cs - convert hardcoded English to CitizenLocalization.SayLocalized.
Preserves original line endings.
"""
import re

BEHAVIOR_PATH = "/Users/forrrest/projects/UO-Memento/ultima-memento/World/Source/Scripts/Mobiles/Base/Behavior.cs"

with open(BEHAVIOR_PATH, 'rb') as f:
    raw = f.read()

# Detect line ending
if b'\r\n' in raw:
    newline = '\r\n'
else:
    newline = '\n'

content = raw.decode('utf-8')
lines = content.split(newline)

FICTIONAL_PATTERNS = [
    'Esaeu lizz gia xes zes soth',
    'Dnadona qae zaaq esaeun doom',
    'I lizz raeq chq esaeu xaed za',
    'Dnadona qae gia,',
    'Rae...sael yor yiz xa?',
    'Zae zes hima ends sabbia!',
    'Gri Gril Gestroy Groo!',
    'Groo Gran Grever Gregreat Gre!',
    'Grour Grones Gril Gray Grin Gry Grwamp!',
    'Groo Grar Gro Gratch Gror Gre!',
    'Groo Grite Grood!',
    'Grarrgh...',
    '*blinks a',
]

DEATH_GRUNTS = {
    'No!', 'Argh!', 'Ahhh...', 'Nooo...', 'I...uh...uhhhhh...',
    'Arrrggghhh...',
}

def has_fictional_pattern(text):
    for fp in FICTIONAL_PATTERNS:
        if fp in text:
            return True
    return False

def get_say_inner(line_text):
    """Extract the content inside from.Say(...)"""
    idx = line_text.find('from.Say(')
    if idx == -1:
        return None, None, None, None
    start = idx + len('from.Say(')
    paren_depth = 1
    i = start
    while i < len(line_text) and paren_depth > 0:
        if line_text[i] == '(':
            paren_depth += 1
        elif line_text[i] == ')':
            paren_depth -= 1
        i += 1
    if paren_depth != 0:
        return None, None, None, None
    end = i - 1
    inner = line_text[start:end]
    before = line_text[:idx]
    after_paren = line_text[end+1:]
    return before, inner, after_paren, line_text

in_say_attack = False
in_say_death = False
modifications = 0
skipped = []

for i, line in enumerate(lines):
    stripped = line.rstrip()
    
    if 'public static void SaySomethingWhenAttacking' in stripped:
        in_say_attack = True
    if 'public static void SaySomethingOnDeath' in stripped:
        in_say_attack = False
        in_say_death = True
    
    is_in_say = in_say_attack or in_say_death
    if not is_in_say or 'from.Say(' not in stripped:
        continue
    
    if 'NameList.RandomName("magic words")' in stripped:
        skipped.append((i+1, stripped, 'magic words'))
        continue
    if has_fictional_pattern(stripped):
        skipped.append((i+1, stripped, 'fictional'))
        continue
    
    before, inner, after_paren, _ = get_say_inner(stripped)
    if inner is None:
        skipped.append((i+1, stripped, 'parse error'))
        continue
    
    after_paren = after_paren.lstrip()
    if not after_paren.startswith(';'):
        after_paren = ';' + after_paren
    
    # Check death grunt
    if '+' not in inner:
        m = re.match(r'^"((?:[^"\\]|\\.)*)"$', inner)
        if m:
            literal = m.group(1)
            if literal in DEATH_GRUNTS:
                skipped.append((i+1, stripped, 'death grunt'))
                continue
    
    if '+' not in inner:
        m = re.match(r'^"((?:[^"\\]|\\.)*)"$', inner)
        if m:
            literal = m.group(1)
            new_line = f'{before}CitizenLocalization.SayLocalized(from, "{literal}"){after_paren}'
            lines[i] = new_line.rstrip('\r\n')
            modifications += 1
        else:
            skipped.append((i+1, stripped, 'simple parse fail'))
        continue
    
    # Has + concatenation
    parts = []
    current = ''
    in_string = False
    paren_depth = 0
    for ch in inner:
        if ch == '"' and (not current or current[-1] != '\\'):
            in_string = not in_string
            current += ch
        elif ch == '(':
            paren_depth += 1
            current += ch
        elif ch == ')':
            paren_depth -= 1
            current += ch
        elif ch == '+' and not in_string and paren_depth == 0:
            parts.append(current.strip())
            current = ''
        else:
            current += ch
    if current.strip():
        parts.append(current.strip())
    
    if len(parts) <= 1:
        skipped.append((i+1, stripped, 'concat fail'))
        continue
    
    all_literals = all(p.startswith('"') and p.endswith('"') for p in parts)
    if all_literals:
        combined = ''.join(p[1:-1] for p in parts)
        new_line = f'{before}CitizenLocalization.SayLocalized(from, "{combined}"){after_paren}'
        lines[i] = new_line.rstrip('\r\n')
        modifications += 1
        continue
    
    format_parts = []
    args = []
    for part in parts:
        if part.startswith('"') and part.endswith('"'):
            format_parts.append(part[1:-1])
        else:
            placeholder_idx = len(args)
            format_parts.append('{' + str(placeholder_idx) + '}')
            args.append(part)
    
    fmt = ''.join(format_parts)
    args_str = ', '.join(args)
    new_line = f'{before}CitizenLocalization.SayLocalizedFormat(from, "{fmt}", {args_str}){after_paren}'
    lines[i] = new_line.rstrip('\r\n')
    modifications += 1

output = newline.join(lines)

with open(BEHAVIOR_PATH, 'wb') as f:
    f.write(output.encode('utf-8'))

print(f"Behavior.cs: {modifications} from.Say() lines converted to CitizenLocalization")
print(f"Skipped {len(skipped)} lines:")
for ln, text, reason in skipped:
    print(f"  L{ln} ({reason}): {text[:80]}")
