# 服务器稳定性与常见崩溃模式（AI / 开发者指引）

## 概述

本文档列出 **Ultima Memento**（RunUO / ServUO 系）代码库中 **常见且可避免** 的运行时严重问题：栈溢出、空引用、序列化损坏、资源耗尽等。**AI Agent 在修改 C# 核心与脚本时，应主动规避下列反模式**；人类审阅也可按表做针对性的代码评审。

**核心引擎相关实现参阅：** `World/Source/System/Item.cs`、`Mobile.cs`、`World.cs` 等。

---

## 1. 对象属性列表（OPL）与 `InvalidateProperties` 重入

| 项目 | 说明 |
|------|------|
| **现象** | `StackOverflowException`；堆栈顶多为 `Item.GetProperties` → `ObjectPropertyList` → `InvalidateProperties` → `PropertyList` 循环。 |
| **根因** | 在 **正在构建 OPL** 期间（`PropertyList` getter 或 `GetLocalizedPropertyList` → `GetProperties`），对 `ColorText1`–`ColorText5`（及任何 **setter 内会调用 `InvalidateProperties()`** 的属性）赋值。`InvalidateProperties` 会清空缓存并 **同步** 再次取 `PropertyList`，从而重入 `GetProperties`，形成无限递归。 |
| **正确做法** | 在 `GetProperties` / `AddNameProperties` 等 OPL 路径内：对后备字段赋值（如 `m_ColorText3`），**不要**走 `ColorText3 = …`；或覆盖 `AddColorText3Property` 用 `list.Add` + `ResolvePropertyText`，避免在构建期改带 `InvalidateProperties` 的属性。参见 `AGENTS.md` §3.2 对 `AddColorText3Property` 的说明。 |
| **核心缓解** | `Item` / `Mobile` 已实现构建深度计数：构建期间 `InvalidateProperties` **延迟**到构建结束后再执行（见 `Item.m_PropertyListBuildDepth`）。Agent **仍不应**依赖此机制而继续在 OPL 内滥用 setter——延迟只防崩溃，不当用法仍可能导致多余重建或时序问题。 |

---

## 2. 无限递归（非 OPL）

| 项目 | 说明 |
|------|------|
| **现象** | `StackOverflowException`。 |
| **常见来源** | 成对覆盖的 `virtual` 方法互相 `base`/子类调用路线错误；`OnDuplicated` / `OnAdded` / `OnRemoved` 间接再次触发同一入口；事件 `EventSink` 回调里再次触发同一事件且无终止条件。 |
| **正确做法** | 画调用链；保证递归有明确出口；敏感路径用显式标志位或早期返回防止重入。 |

---

## 3. 空引用（`NullReferenceException`）

| 项目 | 说明 |
|------|------|
| **现象** | 服务器日志或崩溃报告中的 NRE。 |
| **常见来源** | `Mobile` / `Item` 的 `Map` 为 `Internal` 或逻辑上已删除仍被访问；`Parent` / `NetState` / `Account` 在未校验下使用；`from`、工具、目标物品在命令或 Gump 回调中为空。 |
| **正确做法** | 与周边代码一致地做 `null` / `Deleted` / `Map == Map.Internal` 检查；不在不确定状态下遍历容器或父链。 |

---

## 4. 世界加载与序列化

| 项目 | 说明 |
|------|------|
| **现象** | 启动失败、读档异常、物品/生物状态错乱，极少数情况后续 NRE。 |
| **常见来源** | 修改 `Serialize`/`Deserialize` 版本号与字段顺序不一致；在 `Deserialize` 中假设世界已完全加载并访问全局集合；删除字段却未读掉旧数据导致流错位。 |
| **正确做法** | 严格递增 `version`；新旧分支读满字节流；加载期逻辑参考现有项对 `World.Loading` 的用法；**禁止**人工编辑 `World/Saves/`。 |

---

## 5. 定时器、队列与循环

| 项目 | 说明 |
|------|------|
| **现象** | 服务器卡死、单线程占用 100%、逻辑上“死循环”。 |
| **常见来源** | `Timer.DelayCall`/自定义轮询无终止条件；在回调里再次以零延迟注册同一工作；`while` 遍历集合时集合被修改导致异常或逻辑错误（见下节）。 |
| **正确做法** | 为定时任务设最大次数或结束条件；避免在回调中无条件自我重入；大工作量用分片延迟处理。 |

---

## 6. 在遍历中修改集合

| 项目 | 说明 |
|------|------|
| **现象** | `InvalidOperationException: Collection was modified` 或漏项/重复处理。 |
| **常见来源** | `foreach` 容器时 `Delete` 子项、向同一容器 `Add`、修改触发重排。 |
| **正确做法** | 先复制到数组/`List` 再遍历；或使用引擎惯用的延期删除模式；与现有 `Scripts` 中同类逻辑保持一致。 |

---

## 7. 除零与数值边界

| 项目 | 说明 |
|------|------|
| **现象** | `DivideByZeroException` 或 `ArithmeticException`；或隐身溢出导致逻辑异常。 |
| **常见来源** | 伤害、价格、技能检查公式中分母来自可为 0 的运行时值；`unchecked` 算术未考虑边界。 |
| **正确做法** | 除法前显式校验；与现有公式风格一致；对玩家可见的经济相关计算尤需谨慎。 |

---

## 8. 网络与封包

| 项目 | 说明 |
|------|------|
| **现象** | NRE 或客户端异常；极端情况下崩溃。 |
| **常见来源** | `NetState` 已断开仍 `Send`；向 `null` 列表发包；假定 `Mobile` 必有客户端。 |
| **正确做法** | 发送前检查 `NetState != null`、连接有效；匹配引擎内其他发送点的写法。 |

---

## 9. 未捕获异常与资源释放

| 项目 | 说明 |
|------|------|
| **现象** | 单次操作失败波及整 tick；句柄/包未释放。 |
| **常见来源** | 空 `catch` 吞掉异常；处理失败后未 `return` 导致后续用到损坏状态。 |
| **正确做法** | 遵循 `AGENTS.md`：**不要静默吞异常**；`Packet.Release` 等沿用现有 `using`/`try/finally` 模式。 |

---

## 10. 内存与性能（劣化至不可服务）

| 项目 | 说明 |
|------|------|
| **现象** | OOM、长时间 GC、世界 tick 超时。 |
| **常见来源** | 无上限缓存、每 tick 全图扫描、字符串大量拼接无 pooling。 |
| **正确做法** | 限制缓存大小；热点路径Profiler；与现有 `Core` 调度方式一致。 |

---

## AI Agent 工作检查清单（摘要）

在提交涉及 **物品 / 生物 / OPL / 序列化 / 定时器** 的改动前，自问：

1. **OPL 路径内**是否对 `ColorText*`、`Hue` 等会 `InvalidateProperties` 的属性使用了 **setter**？若必须改显示内容，是否可改为 `m_ColorText*` 或覆盖 `AddColorText*Property`？
2. 是否存在 **无出口的递归** 或 **定时器自我触发**？
3. 是否所有 **`Mobile`/`Item`/`Map`** 访问都有与周边代码相当的 **空与安全地图** 检查？
4. **`Serialize`/`Deserialize`** 是否版本一致、分支读全？
5. 是否在 **`foreach` 容器**时修改了同一集合？
6. 除法与数组索引是否在 **运行时** 下安全？
7. 改动是否 **与现有 `World/Saves/` 读档兼容**？按根目录 **`AGENTS.md` §4.5** 做最终存档兼容性审查并在回复中报告结论。

更细的工程与本地化约束仍以根目录 **`AGENTS.md`** 为准；本文件专注 **稳定性与崩溃规避**。

---

## 文档维护

- **何时更新：** 发现新书或系统性崩溃根因、或核心引擎对 `InvalidateProperties`/OPL 行为有变更时，增补表格与检查项。
- **不匹配时：** 以 `World/Source/System` 实际代码为准，并提议修订本文件与 `AGENTS.md` 索引。

**变更记录：**

- 2026-05-18：初版（OPL 重入、递归、NRE、序列化、定时器、集合修改、除零、网络、异常、内存与 Agent 检查清单）。
- 2026-07-03：§10 Agent 检查清单第 7 条 — 指向 `AGENTS.md` §4.5 现有存档兼容性最终审查。
