#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Merge vendor_npc_speech_zh_table translations into zh-Hans/vendor_npc_speech.json."""

import json
import os
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
EN_PATH = os.path.join(ROOT, "World", "Data", "Localization", "en", "vendor_npc_speech.json")
ZH_PATH = os.path.join(ROOT, "World", "Data", "Localization", "zh-Hans", "vendor_npc_speech.json")

sys.path.insert(0, os.path.dirname(__file__))
from vendor_npc_speech_zh_table import build_zh_map  # noqa: E402


def main() -> None:
	with open(EN_PATH, encoding="utf-8") as f:
		en = json.load(f)
	if not os.path.isfile(ZH_PATH):
		with open(ZH_PATH, "w", encoding="utf-8") as f:
			json.dump(en, f, ensure_ascii=False, indent=2)
			f.write("\n")
	with open(ZH_PATH, encoding="utf-8") as f:
		zh = json.load(f)

	over = build_zh_map(EN_PATH)
	for k, v in over.items():
		zh[k] = v

	# Ensure every EN key exists in zh (fallback English until translated)
	for k, v in en.items():
		if k not in zh or zh[k] is None or len( str( zh[k] ) ) == 0:
			zh[k] = v

	with open(ZH_PATH, "w", encoding="utf-8") as f:
		json.dump(zh, f, ensure_ascii=False, indent=2)
		f.write("\n")

	print("Updated", ZH_PATH, "keys:", len(zh))


if __name__ == "__main__":
	main()
