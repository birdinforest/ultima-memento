Localization (server-side)
============================

Layout (split by source tree category)
--------------------------------------
Data/Localization/en/<category>.json
Data/Localization/zh-Hans/<category>.json

Categories match World/Source layout:
  system.json                    — Source/System
  scripts-system.json          — Source/Scripts/System
  scripts-items.json           — Source/Scripts/Items
  scripts-mobiles.json       — Source/Scripts/Mobiles
  scripts-engines-and-systems.json — Source/Scripts/Engines and Systems
  scripts-utilities.json       — Source/Scripts/Utilities

_index.json lists en files for translators (runtime loads all *.json in en/ and zh-Hans/).

Configuration
-------------
Data/System/CFG/localization.cfg

Commands
--------
[Lang]  [Lang en|zh-Hans]  [ReloadLang]

Regenerating
------------
cd to repository root (ultima-memento), then:

  python3 World/Source/Tools/build_localization_strings.py --no-translate
    Re-scan C# and rewrite split files; preserves Chinese when English unchanged.

  pip install deep-translator
  python3 World/Source/Tools/translate_zh_from_en.py
    Machine-translate each en/*.json into zh-Hans/*.json.

Coverage notes
--------------
See World/Documentation/localization-implementation-review.md — not all server
text is in JSON yet (gumps, Clilocs, formatted SendMessage, etc.).
