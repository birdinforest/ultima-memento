Localization regression cases (zh-Hans)
========================================

Each file is one flat JSON object. All property values MUST be JSON strings (including
schemaVersion) so Server.Localization.SimpleJsonObject can parse them.

Required fields:
  schemaVersion  "1"
  id             Stable id for reports (also used in tools-output JSON).
  pipeline       One of:
                   citizen_broadcast  — TryApplyForBroadcast → composite → NpcSpeechTokenZh
                   overhead_chain     — StringCatalog.TryResolve(zh viewer) → speaker.LocalizeDynamicOverheadForViewer
                   composite_only     — QuestCompositeResolver.ResolveCompositeToZhHans(en)
                   string_catalog_only — StringCatalog.TryResolve("zh-Hans", en) ?? en
  en             Input English (exact game line).
  expectedZh     Golden zh-Hans (entire pipeline output for that pipeline).

Optional: tags, source, notes (all strings).

Run from directory containing WorldLinux.exe (usually World/):

  mono WorldLinux.exe -localization-regression
  mono WorldLinux.exe -locreg

Or use World/Source/Tools/run_localization_regression.sh from repo root.

Exit code 0 = all pass; 1 = one or more mismatches (see console + Data/Localization/tools-output/localization-regression-report.json).
