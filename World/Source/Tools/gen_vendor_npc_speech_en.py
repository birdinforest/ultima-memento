#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Regenerate Data/Localization/en/vendor_npc_speech.json from Talk.cs (hash-keyed templates).

Only covers conversations still built from plain "..." + sYourName + sMyName concatenation.
Branches using StringCatalog.Resolve / ResolveFormat (Variety, Architect, Assassin, Pets, etc.)
are emitted by build_localization_strings.py into scripts-*.json instead.

Run from repo root:
  python3 World/Source/Tools/gen_vendor_npc_speech_en.py

Then run (adds new keys to zh JSON with EN fallback; does not overwrite existing zh):
  python3 World/Source/Tools/apply_vendor_npc_speech_zh.py

Optional bulk zh rewrite (in-repo dict): rebuild_vendor_npc_speech_zh.py
"""

import hashlib
import json
import os
import re
from typing import Optional

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
TALK = os.path.join(ROOT, "World", "Source", "Scripts", "System", "Misc", "Talk.cs")
OUT = os.path.join(ROOT, "World", "Data", "Localization", "en", "vendor_npc_speech.json")


def for_english(s: str) -> str:
	h = hashlib.sha256(s.encode("utf-8")).digest()
	return "s." + "".join(f"{b:02x}" for b in h[:8])


def concat_to_template(expr: str) -> Optional[str]:
	expr = expr.strip().rstrip(";").strip()
	if any(x in expr for x in ["JesterSpeech"]):
		return None
	segs = re.split(r"\s*\+\s*", expr)
	out = []
	for seg in segs:
		seg = seg.strip()
		if seg == "sYourName":
			out.append("{0}")
		elif seg == "sMyName":
			out.append("{1}")
		elif seg.startswith('"') and seg.endswith('"'):
			out.append(seg[1:-1].replace('\\"', '"'))
		elif seg in ("lowreg", "mercrate"):
			return None
		elif seg.startswith("MuseumBook") or seg.startswith("MySettings"):
			return None
		else:
			return None
	return "".join(out)


def main() -> None:
	full = open(TALK, encoding="utf-8").read()
	en: dict[str, str] = {}

	ranger_m = re.search(r'if \( sConversation == "Ranger"\)\{ sText = ([^;]+);', full)
	if ranger_m:
		t = concat_to_template(ranger_m.group(1))
		if t:
			en[for_english(t)] = t

	blocks = list(re.finditer(r'else if \( sConversation == "([^"]+)"\)\s*\{', full))
	for i, m in enumerate(blocks):
		start = m.end()
		end = blocks[i + 1].start() if i + 1 < len(blocks) else len(full)
		body = full[start:end]
		am = re.search(r"sText\s*=\s*([^;]+);", body, re.DOTALL)
		if not am:
			continue
		first = am.group(1).strip()
		if "\n" in first:
			first = first.split("\n")[0].strip()
		t = concat_to_template(first)
		if not t:
			continue
		name = m.group(1)
		# Skip partial Architect opener (full speech depends on server flags).
		if name == "Architect":
			continue
		en[for_english(t)] = t

	os.makedirs(os.path.dirname(OUT), exist_ok=True)
	with open(OUT, "w", encoding="utf-8") as f:
		json.dump(en, f, ensure_ascii=False, indent=2)
		f.write("\n")

	print("Wrote", OUT, "keys:", len(en))


if __name__ == "__main__":
	main()
