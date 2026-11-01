# 上游同步本地化欠账（2026-10-14）

> 上游 Jascen/main 位置 1-431 同步完成后产生。全部为显示层债务：英文回退正常，中文玩家在下列位置暂时看到英文。

## A. 法术模型重构丢失的 StringCatalog 包装（需按新模型回植）

上游把 HolyMan / Bard / DeathKnight 切换到中心化法术模型（916d1ec6 / 61bf8df2 / dd6dceeb）。冲突文件按用户政策整体取上游，我方 StringCatalog 包装丢失。回植时注意新模型结构（SpellDefinition / 集中帮助 Gump）。

| 提交 | 文件(丢失引用数) |
|---|---|
| 916d1ec6 HolyMan | HolyManSpell.cs(26) HolyManSpellBook.cs(4) HolyManSpellBookGump.cs(22) Spells/: BanishEvil(8) DampenSpirit(2) Enchant(4) HammerOfFaith(3) HeavenlyLight(2) Rebirth(8) SacredBoon(2) Seance(6) |
| 61bf8df2 Bard | Scrolls/*(16 文件×2) Spells/*(6 文件×2) SongBook.cs(20) SongBookGump.cs(62) |
| dd6dceeb DeathKnight | DeathKnightSpell.cs(21) DeathKnightSpellBook.cs(4) DeathKnightSpellBookGump.cs(53) DeathSkulls.cs(15) Spells/: HagHand(2) Hellfire(3) SoulReaper(3) SuccubusSkin(2) |
| 41d9fc57 TemptationGump | Puzzle_master 详细 INFO 页 builder（约 30 行英文）未翻译；短描述已用 temptationgump.puzzle.* 键本地化 |

参考形状：Resolve(53) ResolveFormat(22) ResolveByKey(1)，载体 SendMessage(32)/Say(4)/PrivateOverheadMessage(1)/txt+= 。中文翻译可从 git 历史（sync-checkpoint-batch-42 及更早检查点）恢复。

## B. 上游改写英文原文导致的重翻译积压

提取器 --fail-on-translated-zh-drop 报告 268 个旧哈希键失配（EN 原文被上游改写，旧中文随旧键丢弃，可从 git 历史恢复对照）。llm_incremental_locale.py stats：needs_translation=314，分布：

- scripts-engines-and-systems.json: 112
- scripts-system.json: 64
- scripts-items.json: 48
- scripts-mobiles.json: 46
- scripts-quests.json: 38
- scripts-books.json: 4; timers: 1; utilities: 1

处理：`llm_incremental_locale.py queue -o ...` → LLM 翻译（禁 Google/DeepL）→ apply → sync_localization_glossary。

## C. 本会话新增 Resolve 字面量（提取器已跑，hash 键已入库）

- BaseWeapon/BaseArmor/BaseClothing/BaseTrinket/BaseInstrument 的 Avatar 专属物品装备拒绝语（Only an Avatar may use this item. / This item belongs to another.）
- QuestTake 的 Abandon Quest? 确认 Gump 标题与正文

## D. 已顺带修复

- pending-fixes #1（SpellRegistry MAX_SPELL_ID≥700 法术不可用）：上游 MAX_SPELL_ID=1000 + 中心化法术模型重构，待玩家实测 HolyMan 法术条。

## E. 2026-11-01 清偿进度（PR #13 / merge d314b1cf）

- **A（Holy/Bard/DeathKnight）**：玩家可见 `SendMessage` / `PrivateOverheadMessage` 已回植 `StringCatalog.Resolve` / `ResolveFormat`；技能门槛句改为 `{0}` 模板。
- **B（hash 重译积压）**：`llm_incremental_locale.py stats` → `needs_translation: 0`（纯占位模板如 `{0} = {1}` 记为 identity）。
- **TemptationGump**：UI / 帮助 / Puzzle INFO 详页改走 `temptation-gump.json` shotkeys；`TemptationCommands` Avatar 解锁提示亦已 shotkey 化。
- **验证**：`sync_localization_glossary.py --check` exit 0。

