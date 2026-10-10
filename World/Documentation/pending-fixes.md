# Pending Fixes（待修复缺陷记录）

> 已确认、暂未修复的缺陷记录。每条包含：现象、复现、根因（file:line 以记录时为准）、修复方向、验证点、存档兼容注意。
> 修复完成后请把对应条目移入下方「已修复」区并附 commit。

---

## 记录于 2026-10-10（下次一起修复）

### 1. 新法术条：HolyMan（牧师）技能全部无法施放，提示「未找到该法术」

**现象**：新版法术条（Spell Bars 工具条 Gump）上，HolyMan（Priest 学派）的全部 14 个技能点击后只收到「未找到该法术」提示，无法施放。旧版 HolyMan 法术书内施放正常。

**根因**：`SpellRegistry` 的类型数组硬编码为 `new Type[700]`，而 HolyMan 法术注册 ID 为 770–783（≥700），注册与查找全部静默失效：

- `World/Source/Scripts/Engines and Systems/Magic/Base/SpellRegistry.cs:15` — `m_Types = new Type[700]`（上限 700）。
- `SpellRegistry.Register`（同文件 :70-71）— `spellID >= m_Types.Length` 时直接 return，**静默丢弃注册**。
- `SpellRegistry.NewSpell(int, Mobile, Item)`（同文件 :113-135）— 同样越界返回 null。
- 法术条施放入口 `World/Source/Scripts/Engines and Systems/Spell Bars/Core/SpellBarToolbarGump.cs:73-79` — `SpellRegistry.NewSpell(spellId, from, null)` 返回 null 即 `from.SendMessage("That spell was not found.")`（客户端本地化显示为「未找到该法术」）。
- HolyMan 注册：`World/Source/Scripts/Engines and Systems/Magic/Base/Initializer.cs:358-371`（770–783）。
- 旧法术书之所以正常：`World/Source/Scripts/Engines and Systems/Magic/Holy Man/HolyManSpellBookGump.cs:368+` 直接 `new BanishEvilSpell(from, null).Cast()` 构造实例，完全绕过 `SpellRegistry`。

**受影响范围（不止 HolyMan）**：所有 ID ≥ 700 的注册都被吞掉：

| 学派 | ID 段 | 注册处 | 是否已上法术条 |
|---|---|---|---|
| Magical | 700–706 | Initializer.cs:333-339 | 未上条，但注册同样失效 |
| DeathKnight | 750–763 | Initializer.cs:342-355 | **是**（`Schools/DeathKnight.cs:43-44`，基地址 750）——症状应与 HolyMan 相同 |
| HolyMan | 770–783 | Initializer.cs:358-371 | **是**（`Schools/Priest.cs:41-42`，基地址 770）——本次报告的问题 |

Research/远古法术为 600–663（Initializer.cs:267-330），低于 700，不受影响——这解释了为什么远古法术条正常。

**修复方向**：把 `m_Types` 扩容（如 `new Type[800]` 或 1024，并留注释说明最大注册 ID）。`SpellRegistry` 为纯运行时结构、无序列化，扩容不影响存档。顺手项：`SpellBarToolbarGump.cs:79` 的 `"That spell was not found."` 是硬编码英文，应按 §3.2 目录化（shotkey 或 `StringCatalog.Resolve` 字面量）。

**验证点**：编译通过；HolyMan、DeathKnight 法术条各实际施放一个法术；确认 Research 条不受影响；确认 `SpellRegistry.NewSpell` 对 770–783 返回非 null。

**存档兼容**：N/A（无序列化改动，纯运行时）。

---

### 2. 化生飞升重生：研究包已找回/共鸣完成，重新登录仍弹「待共鸣」重生报告

**现象**：化生飞升（Avatar）角色死亡重生后，远古魔法研究包通过记忆回响共鸣任务已找回（共振完成、背包已激活），但每次重新登录仍弹出「Avatar 重生报告」Gump，列出「研究包：沉睡于银行（待共鸣）」等过期信息；关闭时还会再次收到「你的研究包已在银行中等待共鸣」提示。

**触发链路**：

- 重生迁移：`World/Source/Scripts/Engines and Systems/Avatar/AvatarCoreItemMigration.cs:139`（`MigrateItems` 末尾 `ctx.HasPendingRebirthReport = …`）。
- 登录入口：`World/Source/Scripts/Mobiles/Base/PlayerMobile.cs:909-918`（`OnLogin`：`HasPendingRebirthReport` 为 true 则 2 秒后 `TryShowRebirthReport`）。
- 展示判定：`AvatarCoreItemMigration.cs:178-191`（`TryShowRebirthReport`）。
- 报告 Gump：`World/Source/Scripts/Engines and Systems/Avatar/Gumps/AvatarRebirthReportGump.cs`。

**三个独立缺陷点**（共同导致「共鸣完成后仍反复弹」）：

1. **Gump 右键关闭不清旗标**：`AvatarRebirthReportGump.OnResponse`（:55-58）在 `info.ButtonID != 1` 时直接 return——玩家用右键/ESC 关闭 Gump（ButtonID 0）不会执行 `HasPendingRebirthReport = false`，旗标永久为 true。
2. **`CompleteResonance` 不清任何待共鸣状态**：`AvatarCoreItemMigration.cs:142-176` 完成共振（恢复背包、激活背包与法术书）后，不清 `ctx.HasPendingRebirthReport`、不清 `ctx.CurrentResonanceLocation` / `MemoryEchoSearchBaseSerial`、不清 `Snapshot*` 研究快照字段（`HasResearchSnapshot` 因此永远为 true）。调用方：`ResearchResonanceConfirmGump.cs:42`（Search 路径）、`LostArtsRegistrarGump.cs:99`（Registrar 付费路径）。
3. **`TryShowRebirthReport` 判定不看背包是否仍沉睡**：`AvatarCoreItemMigration.cs:187` 的守卫是 `GetResearchBag() == null && !HasResearchSnapshot && !HasMigratableCoreItems` 才跳过——背包已找回（非 null）或快照字段非空（永远非空，见上）都会继续弹，从不检查 `bag.IsDormant`。

另：Gump 关闭按钮路径（OnResponse :60-68）还会因 `CurrentResonanceLocation` 未清空而再次发送 `research.resonance.msg.bank_reminder`「待共鸣」提示——与用户报告的"重新登录还会弹出提示待共鸣"吻合。

**修复方向**（供下次实施参考）：

- `CompleteResonance` 成功后：`ctx.HasPendingRebirthReport = false`；按需清理 `CurrentResonanceLocation` / `MemoryEchoSearchBaseSerial`（若 UI 别处还依赖回响位置展示，至少不再用它发 bank_reminder）；评估是否清 `Snapshot*` 字段（保留可能有利于后续死亡快照回退，但需与 `HasResearchSnapshot` 判定解耦或加"已完成共鸣"标记）。
- `AvatarRebirthReportGump.OnResponse`：任意 ButtonID（含 0）都清 `HasPendingRebirthReport`。
- `TryShowRebirthReport`：增加 `bag.IsDormant` 检查——背包已激活/不存在沉睡背包且无待处理快照时不弹。
- 顺带留意：`PlayerContextResearch.SuppressRebirthReport`（:35，序列化于 :119/:162）全库只读不写，是死开关——修复时可决定启用或删除。

**存档兼容注意**：`PlayerContextResearch` 字段均为已序列化字段，运行时清值不需要版本升级；若修复引入新字段（如"共鸣已完成"标记）须走版本递增 + 旧档读取分支（§4.5）。

---

## 已修复

（空）
