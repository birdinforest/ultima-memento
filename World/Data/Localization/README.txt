Localization (server-side)
============================

Files
-----
- strings.en.json   — English text keyed by hash of the exact English string (matches C# literals).
- strings.zh-Hans.json — Simplified Chinese (machine-translated; review for lore/quests).

Configuration
-------------
Data/System/CFG/localization.cfg
  DefaultLanguage=en|zh-Hans   (new accounts)
  FallbackLanguage=en

Commands (in game)
------------------
[Lang]              — show current language
[Lang en]         — English
[Lang zh-Hans]    — Chinese (aliases: zh, zh-cn, cn, chinese)
[ReloadLang]      — reload JSON from disk (no access level required)

Regenerating strings
--------------------
From repository root:
  python3 World/Source/Tools/build_localization_strings.py --no-translate
    Refreshes strings.en.json from C# and merges new keys into zh-Hans (keeps existing Chinese).

  python3 World/Source/Tools/translate_zh_from_en.py
    Re-fills zh-Hans from en using Google Translate (requires: pip install deep-translator).

Notes
-----
- SendMessage / SendAsciiMessage / Say(single string) are resolved at runtime for the viewer's account language.
- SendMessage(format, args) is NOT localized (format is not a full English sentence in the table).
- Public overhead speech is translated per receiving client.
