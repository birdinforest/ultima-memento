#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Sync zh-Hans/vendor_npc_speech.json with en/vendor_npc_speech.json without clobbering translations.

The server resolves vendor speech via StringCatalog + hash keys in these JSON files only.
Chinese strings are edited in Data/Localization/zh-Hans/vendor_npc_speech.json (or regenerated
in-repo by rebuild_vendor_npc_speech_zh.py). This script does NOT read vendor_npc_speech_zh_table.py.

When new English keys appear (e.g. after gen_vendor_npc_speech_en.py), missing zh entries fall back
to English until someone translates the JSON.
"""

import json
import os

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
EN_PATH = os.path.join(ROOT, "World", "Data", "Localization", "en", "vendor_npc_speech.json")
ZH_PATH = os.path.join(ROOT, "World", "Data", "Localization", "zh-Hans", "vendor_npc_speech.json")


def main() -> None:
	with open(EN_PATH, encoding="utf-8") as f:
		en = json.load(f)

	prev: dict = {}
	if os.path.isfile(ZH_PATH):
		with open(ZH_PATH, encoding="utf-8") as f:
			prev = json.load(f)

	out: dict = {}
	for k, en_val in en.items():
		z = prev.get(k)
		if z is not None and isinstance(z, str) and len(z.strip()) > 0:
			out[k] = z
		else:
			out[k] = en_val

	with open(ZH_PATH, "w", encoding="utf-8") as f:
		json.dump(out, f, ensure_ascii=False, indent=2)
		f.write("\n")

	print("Updated", ZH_PATH, "keys:", len(out), "(preserved existing zh; new keys use EN until translated)")


if __name__ == "__main__":
	main()
