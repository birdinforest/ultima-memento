# Simplified Chinese (zh-Hans) localization — translation guide

This guide applies to narrative text in `World/Data/Localization/zh-Hans/` (books, quests, flavor
lines, etc.). It was derived from concrete revision feedback on a book-style passage: comparing an
over-literary first draft with a reviewer’s corrections and their translator notes (译注).

---

## 1. Revision summary (what changed and why)

| # | English | Weak first draft | Preferred direction | Reason |
|---|---------|------------------|---------------------|--------|
| 1 | *its now lit hallways* | “灯火通明的**廊道**” | “灯火通明的**地城**” (+ note if needed) | Literal “hallways” misses the story beat: the narrator lives in a **dungeon** they renovated; take **context** over literal room type. |
| 2 | *to be sure* (after “quick to search”) | Added “以确认是否还有遗漏” | Keep the main clause only; explain *to be sure* in a **note** | Do **not** add information the English does not state. *To be sure* is often an emphatic tag (“indeed / no doubt”), not “in order to verify.” |
| 3 | *a small dungeon* | “一座**小城般的地穴**” | “一座**小型地城**” | Avoid poetic substitution; **dungeon** maps to 地下城 / 地城 in this shard. |
| 4 | *we searched … for anything of value* | “为一探价值而搜了…” | “在…中**搜索一切有价值的东西**” | Prefer **faithful, plain** wording over ornate paraphrase. |
| 5 | *Devil Guard* | Invented Chinese name | **【Devil Guard】** | Place names must not be invented without an approved glossary entry. |
| 6 | *Moon* | “月辉” (Moonglow guess) | **【Moon】** | **Moon** is a shard term in corpus (`lore-glossary.md`); do not equate it to Moonglow without approval. |

---

## 2. Core rules

### 2.1 Proper nouns: keep English, mark with 【】

- Cities, regions, NPCs, factions, unique items, and other **likely proper names** stay in **English**
  inside **【】** until approved Chinese exists in `World/Data/Localization/glossary-approved-zh.json`.

**Good**

```text
我们回到【Devil Guard】，就此分道。我独自前往【Moon】。
```

**Bad**

```text
我们回到魔鬼卫。我独自前往月辉。
```

**Why**

- Approved spellings belong in `glossary-approved-zh.json`; translators should not mint canonical names.
- Wrong guesses (e.g. Moon vs Moonglow) are costly to fix across many strings.

If `glossary-approved-zh.json` defines a term, use that Chinese in production JSON (no 【】 unless you
still need disambiguation in a draft).

### 2.2 Context over literal wording

When a literal gloss conflicts with **who / where / what** the story is about, prefer the reading
that fits the **scene**. Document non-obvious choices in a **translator note** during review; do
not stuff long explanations into the player-facing string unless the product owner wants footnotes
in-game.

### 2.3 No silent expansion in the body

Do not add motives, outcomes, or clarifications that are **not** in the source. If the English uses
a short tag phrase (*to be sure*, *you know*, etc.), either:

- omit it if it carries no propositional content in Chinese, or  
- reflect it with natural emphasis **without** inventing a new sub-plot, and optionally explain in a
  **review note** (译注) for the editor.

### 2.4 Preserve narrator voice; avoid “literary upgrade”

UO book text is often first-person, direct, confident. Do not “elevate” it with unrelated imagery
(城、穴、廊道) that changes what happened. Match **tone** to the English, not to a generic wuxia novel
unless the English already reads that way.

### 2.5 Game vocabulary (default choices)

Use stable shard terms unless glossary says otherwise:

| English (typical) | zh-Hans default |
|-------------------|-----------------|
| dungeon | 地下城 / 地城 (pick one per passage and stay consistent) |
| wizard | 法师 |
| creatures (hostile) | 魔物 / 生物 (context) |
| camp | 扎营 |
| gold / gems / jewels | 黄金 / 宝石 / 珠玉 |

### 2.6 Runic spell words and English-only incantations

Some catalog strings are **magic formulas**, not narrative prose: Ultima / UO-style **runic**
circle phrases (e.g. *In Vas Mani*) or other fixed **incantation tokens** (e.g. *Xtee Mee Glau*).

For these, **leave zh-Hans identical to English** — same spelling and spacing as `en/`. Do **not**
translate into Chinese or approximate with phonetic spellings (误译示例：「在瓦斯玛尼」for *In Vas Mani*).

**Distinction:** Narrative dialogue that *mentions* a spell may still be translated; only the **pure**
formula strings assigned as localized values must stay English. Follow `AGENTS.md` §3.3.1 and add any new
fixed formulas to `llm_incremental_locale.py` `_IDENTITY_HASH_EN_VALUES` when needed.

---

## 3. Translator notes (译注) — when reviewing

Use notes for:

| Situation | Example note |
|-----------|----------------|
| Proper noun kept in English | 可能的专名保留原文并用【】标记。 |
| Context beats literal gloss | 根据上下文，叙述者实际住在改造后的地下城中。 |
| English idiom / tag phrase | 原文 “to be sure” 为语气插入语，表示强调而非“为了确认”。 |

**Pipeline:** Drafts may carry 译注 for editors. **Shipped JSON** under `zh-Hans/` should normally
contain **no** 译注; conclusions should become glossary entries or updates to this guide.

---

## 4. Workflow

1. Read the English; highlight anything that might be a **proper noun** or a **runic / incantation**
   token (§2.6 — if applicable, copy English verbatim for zh-Hans).  
2. Check `glossary-approved-zh.json`.  
   - If listed: use the approved Chinese.  
   - If not: **【English】** in the draft.  
3. Translate the rest: context-first, no silent additions, stable game vocabulary.  
4. Add 译注 only where a choice is non-obvious.  
5. After sign-off: strip notes from JSON; add new approved pairs to `glossary-approved-zh.json`.
6. Run `World/Source/Tools/sync_localization_glossary.py` so shipped locale files are normalized back to the latest approved canonical terms and context-specific overrides.

**Related tooling and data**

- Heuristic term lists: run `World/Source/Tools/build_lore_glossary.py` (see
  `World/Documentation/lore-glossary.md` and `Data/Localization/lore-glossary.json`).  
- QA report: `World/Source/Tools/review_translations_glossary.py` →
  `World/Documentation/translation-glossary-review.md`.
- Glossary sync: `World/Source/Tools/sync_localization_glossary.py` →
  locale-specific normalization using `World/Data/Localization/zh-Hans-glossary-sync-rules.json`.

---

## 5. Related files

| Path | Role |
|------|------|
| `World/Data/Localization/zh-Hans/*.json` | Shipped Simplified Chinese strings |
| `World/Data/Localization/glossary-approved-zh.json` | Curated EN → approved zh (fill over time) |
| `World/Data/Localization/zh-Hans-glossary-sync-rules.json` | zh-Hans-specific follow-up replacements and exact overrides |
| `World/Data/Localization/README.txt` | Layout, tools, links to docs |
| `World/Documentation/zh-localization-glossary-sync-workflow.md` | Repeatable glossary normalization workflow |
| `World/Documentation/lore-glossary.md` | Auto-generated proper-noun hints |
| `World/Documentation/translation-glossary-review.md` | Glossary-based QA output |
