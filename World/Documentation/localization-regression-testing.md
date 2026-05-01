# Localization regression testing — plan & test-tier framework

## Purpose

Lock in **player-visible Chinese** behavior for dynamic localization pipelines (tavern chatter, quest composites, overhead chains, book/tome-style replacements, and similar) with **golden-case JSON** checked in CI. This document records the **chosen execution model** and a **multi-tier framework** for future server tests.

**Implementation status (2026-05):** Implemented in-repo: [`LocalizationRegressionRunner`](World/Source/System/Localization/Regression/LocalizationRegressionRunner.cs), golden cases under [`World/Data/Localization/regression/cases/`](World/Data/Localization/regression/cases/), CLI **`-localization-regression`** / **`-locreg`** on `WorldLinux.exe` (after `LocalizationBootstrap.Initialize()`), optional report [`Data/Localization/tools-output/localization-regression-report.json`](World/Data/Localization/tools-output/localization-regression-report.json). **Note:** The current hook still runs after **`World.Load()`** (full world init) because `StringCatalog` is loaded in `Main` after world load; this is acceptable for CI but slower. **Phase 2** may move regression earlier or use a slimmer host.

---

## Current execution model (`-localization-regression`)

- **Process:** Same `WorldLinux.exe` as normal shard startup through script compile, `LocalizationBootstrap.Initialize()`, then [`LocalizationRegressionRunner.Run()`](World/Source/System/Localization/Regression/LocalizationRegressionRunner.cs), then **`Environment.Exit(code)`** (no gameplay loop).
- **Trade-off:** **`World.Load()`** still runs first — startup is **not** “seconds-only” yet; exit code and JSON report remain suitable for CI.

---

## Long-term target (minimal host)

The **goal** remains a runner that initializes only `BaseDirectory` + localization (no `World.Load`, no gameplay loop). **Today’s** shipped path is the early-exit flag on `WorldLinux.exe` after full world load (see above). Golden JSON and pipeline semantics are the same either way.

**Why not compile-time only?** Golden tests need runtime catalog merge and real C# resolvers — run after `compile-world-*.sh` succeeds, not inside CSC.

---

## Layout

| Path | Role |
|---|---|
| `World/Data/Localization/regression/cases/*.json` | One case per file (flat string key/value JSON). See [`cases/README.txt`](World/Data/Localization/regression/cases/README.txt). |
| `World/Source/System/Localization/Regression/*.cs` | Runner + zh-Hans stub `IAccount`. |

Case schema: `schemaVersion`, `id`, `pipeline`, `en`, `expectedZh`; optional `tags`, `source`, `notes` (all string values).

---

## Pipelines to cover (initial targets)

Non-exhaustive list aligned with recent bug classes:

- **Dynamic common talk** — `CommonTalkDynamicZh` / tavern-style replacements (articles, “I/We found”, rare-mix, wyrm kinds, long-phrase-before-short composite tables).
- **Quest composite** — `QuestCompositeResolver` + `quest-fragment-zh-table.json` ordering (longer matches before shorter substrings).
- **Overhead / viewer chain** — `Mobile.LocalizeDynamicOverheadForViewer` (`internal`); runner uses NPC speaker + viewer with `RegressionZhHansStubAccount`.
- **Book / museum / tome patterns** — where regex or fragment tables must stay in sync with C# (e.g. `MuseumBook`-class patterns).

New regressions **add JSON** (and, if needed, one focused helper API) instead of duplicating logic in Python.

---

## CI / local invocation

1. Build: `cd World/Source && ./compile-world-mac.sh` (or `compile-world-linux.sh`).
2. Run (from repo root, where `World/WorldLinux.exe` exists):

```bash
bash World/Source/Tools/run_localization_regression.sh
# or:
cd World && mono WorldLinux.exe -localization-regression
```

Exit code **0** = all pass; **1** = failure (console + `World/Data/Localization/tools-output/localization-regression-report.json`).

**CI:** Add a step after compile using the same command. Startup includes full world load (~tens of seconds on a typical checkout).

Optional: gate with `UO_MEMENTO_RUN_LOCALIZATION_REGRESSION=1` only if you wrap the runner in a small script that checks the variable before invoking.

---

## Test-tier framework (server repo, forward-looking)

Use **explicit tiers** so new work lands in the right bucket. Higher tiers cost more to run and maintain.

| Tier | Name | Typical init | Use for |
|---|---|---|---|
| **T0** | Tooling / static | None (Python, scripts only) | Glossary check, extractor, book fragment merge, `llm_incremental_locale.py` stats — already part of localization workflow. |
| **T1** | **Localization regression (current)** | Full `WorldLinux.exe` early exit after `LocalizationBootstrap` | Golden JSON for dynamic zh pipelines; **today** includes `World.Load` (Phase 2: slimmer). |
| **T2** | Partial world / integration | World slices, test maps, or selective `World` init | Quest state, item interactions, skills that need real `Item`/`Mobile` but not full persistence. |
| **T3** | Full server / E2E | Full `WorldLinux.exe`, saves or test harness | Client-visible flows, timing, networking; run sparingly (nightly or manual). |

**Rule:** Do not assume T1 can cover T2/T3. When a feature needs world state, add a **new** harness or promote the case to T2/T3 rather than bloating the lightweight host.

**Naming:** Prefer `*-regression*.md` / `regression/` data dirs under `World/Documentation/` and `World/Data/Localization/` for localization; future non-locale suites may use `World/Documentation/testing/` (create when the first non-localization T1+ harness is added).

---

## Checklist when adding a regression case

1. Minimal repro **EN input** and **expected zh-Hans** (agree with glossary / editorial guide).
2. Add case to `Data/Localization/regression/` under the right `pipeline` / suite.
3. Run `bash World/Source/Tools/run_localization_regression.sh` (or `cd World && mono WorldLinux.exe -localization-regression`); fix code or golden if intentional behavior change.
4. If a new pipeline kind is needed, document it in this file and add dispatch in the runner.

---

## References

- `World/Data/Localization/README.txt` — points to this suite under **Localization regression**.

---

## Phase 2 (optional speed-up)

- Move `LocalizationRegressionRunner.Run()` to immediately after `StringCatalog` is ready **without** loading the full world (or split a tiny `mcs` harness). Document any new entry in this file and in `AGENTS.md` §4.4.
