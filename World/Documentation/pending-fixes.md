# Pending Fixes（待修复缺陷记录）

> 已确认、暂未修复的缺陷记录。每条包含：现象、复现、根因（file:line 以记录时为准）、修复方向、验证点、存档兼容注意。
> 修复完成后请把对应条目移入下方「已修复」区并附 commit。

---

## 记录于 2026-10-10（下次一起修复）

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

### 1. 新法术条：HolyMan（牧师）技能全部无法施放，提示「未找到该法术」✅（2026-10-14 玩家实测通过）

**修复**：随 Jascen 上游同步解决，无需本地补丁——

- `SpellRegistry.MAX_SPELL_ID` 提升为 **1000**（原 `new Type[700]`，770–783 注册与查找全部静默失效）。
- 上游 `f6e78322` 增加 spell ID 越界硬抛异常保护。
- 上游 916d1ec6 / 61bf8df2 / dd6dceeb 将 HolyMan / Bard / DeathKnight 切换到中心化法术模型（SpellDefinition），法术书/卷轴类型检测一并重做。

**验证**：2026-10-14 玩家以 GM 号受洗绑定 HolyManSpellbook、`[AllSpells` 灌满 14 法术（770–783）后，从**法术条**逐个施放全部成功，无「未找到该法术」；`[set karma 5000` + `[SetSkill healing/spiritualism 100` 满足受洗门槛（Karma≥2500 且两技能 >0）。受影响同段的 DeathKnight（750–763）与 Magical（700–706）注册随同恢复。

**遗留**：法术条施放失败提示 "That spell was not found." 仍为硬编码英文（本地化欠账，见 `sync-localization-debt-2026-10.md`）。

**存档兼容**：N/A（纯运行时结构，无序列化改动）。

（空）
