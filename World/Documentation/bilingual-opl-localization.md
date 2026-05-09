# 双语 OPL（对象属性列表）本地化方案技术文档

> **更新日期：** 2026-05-09
> **涉及系统：** `ObjectPropertyList` (OPL) 数据包 `0xD6`、`Item.cs`、`StringCatalog`

---

## 目录

1. [架构概述](#1-架构概述)
2. [OPL 数据包发送路径](#2-opl-数据包发送路径)
3. [双缓存机制](#3-双缓存机制)
4. [Cliloc 控制码速查表](#4-cliloc-控制码速查表)
5. [颜色控制](#5-颜色控制)
6. [字体与字号控制](#6-字体与字号控制)
7. [Shotkey 文本索引系统](#7-shotkey-文本索引系统)
8. [启用本地化的代码路径](#8-启用本地化的代码路径)
9. [典型案例](#9-典型案例)
10. [性能说明](#10-性能说明)

---

## 1. 架构概述

UO 客户端通过 **数据包 `0xD6` (Object Property List, OPL)** 获取物品的悬停提示信息。每个 OPL 条目是一个 `{ cliloc, 参数字符串 }` 对。客户端根据 cliloc 编号解析模板，将参数填入模板中的 `~1_val~` 占位符。

双语 OPL 方案的核心思路：

```
用户 A（中文） → 登录 → 服务器检测语言 → 发送中文 OPL 缓存
用户 B（英文） → 登录 → 服务器检测语言 → 发送英文 OPL 缓存
```

**关键组件：**

| 组件 | 位置 | 作用 |
|------|------|------|
| `BuildingPropertyListLocale` | `Item.cs:629` | `[ThreadStatic]` 线程本地存储，在 OPL 构建期间标记当前语言（`"zh"` 或 `"en"`） |
| `IsContentLocalized` | `Item.cs:1453` | 虚属性，子类重写为 `true` 以启用双语 OPL |
| `GetLocalizedPropertyList()` | `Item.cs:1405` | 按语言构建/获取 OPL 缓存 |
| `AddLocalizedProperty()` | `Item.cs:1474` | 通过 shotkey 解析并添加本地化文本到 OPL |
| `ResolvePropertyText()` | `Item.cs:1460` | 解析 shotkey 到当前语言的文本模板 |
| `m_PropertyListByLang` | `Item.cs:625` | `Dictionary<string, ObjectPropertyList>` 各语言的 OPL 缓存 |

**启动流程：**

```
Item.SendPropertiesTo(from)
  ├─ 检测 from.Account 的语言
  ├─ 调用 GetLocalizedPropertyList("zh"|"en")
  │    ├─ 检查 m_PropertyListByLang 是否存在对应语言缓存
  │    ├─ 不存在 → 设置 BuildingPropertyListLocale = "zh"|"en"
  │    ├─ 调用 GetProperties(list) → 子类中通过 BuildingPropertyListLocale 判断走向
  │    ├─ 调用 AppendChildProperties(list)
  │    └─ 缓存到 m_PropertyListByLang
  └─ 发送 OPLInfo 数据包
```

---

## 2. OPL 数据包发送路径

物品的 OPL 在两种情况下发送给客户端：

### 2.1 初次进入视野（`SendInfoTo`）

当物品进入玩家视野时，`Item.SendInfoTo()` 发送世界数据包 + OPL 数据包：

```csharp
// Item.cs ~3761
public virtual void SendInfoTo( NetState state, bool sendOplPacket )
{
    state.Send( GetWorldPacketFor( state ) );    // 世界/外观数据包

    if ( sendOplPacket )
    {
        if ( IsContentLocalized )                     // ← 双语路径
        {
            string lang = AccountLang.GetLanguageCode( state.Mobile?.Account );
            string locale = AccountLang.IsChinese( lang ) ? "zh" : "en";
            state.Send( GetLocalizedOPLPacket( locale ) );
        }
        else
        {
            state.Send( OPLPacket );                   // ← 原路径（单语言 cliloc）
        }
    }
}
```

### 2.2 鼠标悬停（`SendPropertiesTo`）

当玩家鼠标悬停在物品上时，服务器调用此方法：

```csharp
// Item.cs ~1389
public virtual void SendPropertiesTo( Mobile from )
{
    if ( IsContentLocalized )                         // ← 双语路径
    {
        string lang = AccountLang.GetLanguageCode( from?.Account );
        string locale = AccountLang.IsChinese( lang ) ? "zh" : "en";
        from.Send( GetLocalizedPropertyList( locale ) );
    }
    else
    {
        from.Send( PropertyList );                    // ← 原路径
    }
}
```

---

## 3. 双缓存机制

### 3.1 缓存对象

每个物品维护两组缓存：

```csharp
// Item.cs:623-626
private Packet m_OPLPacket;                    // 原单语言 OPL 数据包
private ObjectPropertyList m_PropertyList;     // 原单语言 OPL 对象

private Dictionary<string, ObjectPropertyList> m_PropertyListByLang;  // 各语言的 OPL 对象
private Dictionary<string, Packet> m_OPLPacketByLang;                 // 各语言的序列化数据包
```

### 3.2 构建流程

```csharp
// Item.cs ~1405
public ObjectPropertyList GetLocalizedPropertyList( string locale )
{
    // 懒初始化字典
    if ( m_PropertyListByLang == null )
        m_PropertyListByLang = new Dictionary<string, ObjectPropertyList>( 2 );

    ObjectPropertyList list;
    if ( !m_PropertyListByLang.TryGetValue( locale, out list ) )
    {
        // 设置线程本地语言标记
        BuildingPropertyListLocale = locale;
        try
        {
            list = new ObjectPropertyList( this );
            GetProperties( list );          // ← 子类在此检查 BuildingPropertyListLocale
            AppendChildProperties( list );
            list.Terminate();
            list.SetStatic();               // 标记为只读/可复用
        }
        finally
        {
            BuildingPropertyListLocale = null;  // 清理
        }
        m_PropertyListByLang[locale] = list;
    }
    return list;
}
```

### 3.3 缓存失效

当物品属性变化时（`InvalidateProperties()`），所有语言缓存均被清除：

```csharp
// Item.cs 相关位置
public void ClearProperties()
{
    Packet.Release( ref m_PropertyList );
    Packet.Release( ref m_OPLPacket );

    if ( m_PropertyListByLang != null )
    {
        foreach ( var opl in m_PropertyListByLang.Values )
            opl.Release();
        m_PropertyListByLang.Clear();
    }
    if ( m_OPLPacketByLang != null )
    {
        foreach ( var pkt in m_OPLPacketByLang.Values )
            pkt.Release();
        m_OPLPacketByLang.Clear();
    }
}
```

---

## 4. Cliloc 控制码速查表

OPL 使用 cliloc 编号决定客户端渲染方式。以下是在本方案中使用到的关键 cliloc：

| Cliloc | 客户端模板 | 效果 | 适用场景 |
|--------|-----------|------|---------|
| `1072171`–`1072175` | `<basefont color=#{0}><big>{1}</big></basefont>` | 大号加粗 + 24bit 颜色 | 物品标题、彩色属性行 |
| `1060458` | 红色 `<basefont color=#FF0000>`（固定） | 红色大号粗体 | 无需自定义颜色的红色文本 |
| `1060459` | 白色（固定） | 白色大号粗体 | 无需自定义颜色的白色文本 |
| `1042971` / `1070722` | `~1_NOTHING~`（纯文本） | 默认字体大小，无颜色 | `AddLocalizedProperty()` 输出的文本 |
| `1050039` | `~1_NUMBER~ ~2_ITEMNAME~` | 堆叠物品数量+名称 | `[Amount] [Name]` 格式 |
| `1060584` | `~1_val~ ~2_val~` | 双参数通用格式 | 使用次数（原 "5 Uses"） |
| `1060847` | `~1_val~` | 灰色小号注释 | 辅助信息 |
| `1060636` | （固定 cliloc） | "exceptional" 文本 | 品质标签（已转为 shotkey） |
| `1060401`–`1060486` | 属性命名 cliloc | 各种属性的标准名称 | 属性值（已转为 shotkey） |
| `1061168` | `~1_val~ - ~2_val~` | 伤害范围 | `weapon damage X - Y` |
| `1061182` | `~1_val~` | 装备层级 | `EquipLayerName(Layer)` |

---

## 5. 颜色控制

### 5.1 通过 cliloc 1072171 控制颜色

使用 cliloc `1072171` 时，第一个参数 `{0}` 是 6 位十六进制 RGB 颜色码，第二个参数 `{1}` 是文本：

```csharp
// 语法
list.Add( 1072171, "{0}\t{1}", "RRGGBB", text );

// 示例：紫色标题
list.Add( 1072171, "{0}\t{1}", "9F44FF", "圣辉符文" );
```

**颜色参数格式：** 6 位十六进制（`"RRGGBB"`），不包含 `#` 前缀。cliloc 模板中硬编码了 `#`。

### 5.2 预置颜色示例

| HEX 值 | 显示颜色 | 用途 |
|--------|---------|------|
| `"9F44FF"` | 紫色 | 特殊资源名称（`BaseSpecial`） |
| `"C6D11C"` | 黄绿色 | 神器提示（"May be transmuted"） |
| `"FFB400"` | 金色 | 宝箱名 |
| `"E15656"` | 红色 | 诅咒物品 |
| `"ef4848"` | 红色 | 尸体/遗骸 |
| `"0080FF"` | 蓝色 | 药罐名称、魔杖 |
| `"4ecbff"` | 浅蓝 | 宝箱名 |
| `"499EFF"` | 蓝色 | 铁砧名称 |
| `"11DADA"` | 青色 | 特殊战利品 |
| `"338fff"` | 蓝色 | 魔法颜料 |

### 5.3 ColorText 属性颜色控制

`Item` 提供了 `ColorText1`–`ColorText5` 属性配合 `ColorHue1`–`ColorHue5` 颜色属性，自动在 `AddNameProperties()` 中渲染彩色文本：

```csharp
// CurseItem.cs 多行彩色文本
ColorText1 = "CURSED!";
ColorText3 = "Give to a Wizard or Knight";
ColorText4 = "To Remove the Curse";
ColorText5 = "Or Use Curse Removing Magic";
ColorHue1 = ColorHue3 = ColorHue4 = ColorHue5 = "E15656";

// BuriedBody.cs 彩色名称
ColorText1 = "The skeleton of";
ColorHue1 = "ef4848";
ColorText2 = "Lord British";
ColorHue2 = "ef4848";
```

底层渲染代码（`Item.cs:1650-1675`）：

```csharp
if ( ColorText1 != null )
    list.Add( 1072171, "{0}\t{1}", CHue1, ColorText1 );

if ( ColorText2 != null )
    list.Add( 1072172, "{0}\t{1}", CHue2, ColorText2 );

// ColorText3-5 使用 1072173-1072175（效果相同）
```

### 5.4 在双语 OPL 中控制颜色

双语 OPL 路径中，通过直接调用 `list.Add(cliloc, format, args)` 控制颜色（不使用 `ColorText` 属性）：

```csharp
// Special.cs:72 — 双语路径中直接硬编码颜色
public override void AddNameProperty( ObjectPropertyList list )
{
    string locale = BuildingPropertyListLocale;
    if ( locale != null )
    {
        string name = GetDisplayNameLocalized( m_Resource, Name, locale );
        if ( Amount > 1 )
            list.Add( 1050039, "{0}\t{1}", Amount, name );     // 堆叠：默认大小
        else
            list.Add( 1072171, "{0}\t{1}", "9F44FF", name );   // 单个：紫色大号
        return;
    }
    base.AddNameProperty( list );  // 非双语路径走原逻辑
}
```

### 5.5 颜色说明

- UO 客户端不支持 RGBA（半透明/Alpha），仅支持 24bit RGB
- 不支持渐变色
- 在 `<basefont>` 标签中硬编码 `#`，因此参数必须为无 `#` 前缀的 6 位 hex

---

## 6. 字体与字号控制

### 6.1 可用字号

UO 客户端在 OPL 中仅支持两种字号：

| 字号 | 实现方式 | cliloc |
|------|---------|--------|
| **默认大小** | 使用纯文本 cliloc（`~1_NOTHING~`） | `1042971`、`1070722` |
| **大号加粗** | 通过 `<basefont>` 包裹 `<big>` 标签 | `1072171`–`1072175`、`1060458`、`1060459` |

**没有 `<small>`、`<font size=>`** 等标签。UO 客户端仅识别以下标签：

- `<basefont color=#{0}>` — 设置颜色
- `<big>...</big>` — 放大加粗
- `<b>...</b>` — 加粗（部分客户端支持）

### 6.2 如何选择字号

```csharp
// 大号加粗彩色 — 使用 1072171
list.Add( 1072171, "{0}\t{1}", "9F44FF", "圣辉符文" );

// 默认字号彩色 — 需要使用嵌入 HTML 标签到文本中
list.Add( 1042971, "<basefont color=#9F44FF>圣辉符文</basefont>" );

// 默认字号无颜色
AddLocalizedProperty( list, "prop.damage.increase", 15 );
```

### 6.3 字体

UO 客户端使用自己的游戏字体（通常是 `Unifont` 或系统 `Arial`），**无法通过服务器控制字体类型**。所有 OPL 文本使用相同的客户端字体。

---

## 7. Shotkey 文本索引系统

### 7.1 什么是 Shotkey

Shotkey（逻辑键）是服务器端使用的稳定文本标识符，格式为 `prop.<category>.<name>`，存储在 `en/equipment-properties.json` 和 `zh-Hans/equipment-properties.json` 中。

**英文 JSON（`en/equipment-properties.json`）：**

```json
{
  "prop.damage.increase": "damage increase {0}%",
  "prop.hit.life.leech": "hit life leech {0}%",
  "prop.weapon.damage.range": "weapon damage {0} - {1}"
}
```

**中文 JSON（`zh-Hans/equipment-properties.json`）：**

```json
{
  "prop.damage.increase": "伤害增加 {0}%",
  "prop.hit.life.leech": "击中生命偷取 {0}%",
  "prop.weapon.damage.range": "武器伤害 {0} - {1}"
}
```

### 7.2 Helper 方法

```csharp
// AddLocalizedProperty 完整实现（Item.cs:1474-1482）
public void AddLocalizedProperty( ObjectPropertyList list, string shotkey, params object[] args )
{
    string template = ResolvePropertyText( shotkey );

    if ( args != null && args.Length > 0 )
        list.Add( string.Format( template, args ) );
    else
        list.Add( template );
}

// ResolvePropertyText 完整实现（Item.cs:1460-1466）
protected string ResolvePropertyText( string shotkey )
{
    string locale = BuildingPropertyListLocale ?? "en";
    return StringCatalog.TryResolveByKey( locale, shotkey )
        ?? StringCatalog.TryResolveByKey( "en", shotkey )
        ?? shotkey;
}
```

**调用示例：**

```csharp
// 无参数
AddLocalizedProperty( list, "prop.balanced" );
// → en: "Balanced"   → zh: "平衡"

// 单参数（int 自动 .ToString()）
AddLocalizedProperty( list, "prop.damage.increase", 15 );
// → en: "damage increase 15%"   → zh: "伤害增加 15%"

// 双参数
AddLocalizedProperty( list, "prop.durability", 45, 50 );
// → en: "durability 45 / 50"   → zh: "耐久度 45 / 50"
```

### 7.3 Shotkey 命名规范

```
prop.<分类>.<属性名>     — 通用装备属性
prop.skill.<技能名>      — 武器技能需求
prop.resist.<元素>       — 元素抗性
prop.hit.<效果>          — 击中特效
```

所有已在使用的 shotkey 列表：[`en/equipment-properties.json`](../../Data/Localization/en/equipment-properties.json)

### 7.4 添加新 Shotkey

1. 在 `en/equipment-properties.json` 和 `zh-Hans/equipment-properties.json` 中添加条目
2. 在 C# 中使用 `AddLocalizedProperty(list, "prop.new.key", value)` 调用
3. 如果使用非双语路径的旧代码，保持 `BuildingPropertyListLocale` 判断回退

---

## 8. 启用本地化的代码路径

### 8.1 步骤一：设置 `IsContentLocalized`

在目标物品类的基类中：

```csharp
public override bool IsContentLocalized => true;
```

当前已启用的类（`World/Source`）：

| 类 | 文件 | 行号 |
|---|------|------|
| `BaseWeapon` | `Weapons/BaseWeapon.cs` | 27 |
| `BaseArmor` | `Armor/BaseArmor.cs` | 87 |
| `BaseClothing` | `Clothing/BaseClothing.cs` | 252 |
| `BaseTrinket` | `Trinkets/BaseTrinket.cs` | 42 |
| `BaseTool` | `Trades/BaseTool.cs` | 22 |
| `BaseHarvestTool` | `Trades/BaseHarvestTool.cs` | 96 |
| `BaseInstrument` | `Instruments/BaseInstrument.cs` | 171 |
| `BaseSpecial` | `Trades/Special.cs` | 17 |

### 8.2 步骤二：改造 `GetProperties()`

在 `GetProperties()` 中，原来使用 cliloc 编号的代码需要增加双语分支：

**模式 A — 使用 `AddLocalizedProperty()`（推荐）：**

```csharp
public override void GetProperties( ObjectPropertyList list )
{
    base.GetProperties( list );

    bool localized = BuildingPropertyListLocale != null;  // 检测是否在双语构建中

    int prop;

    if ( (prop = m_AosAttributes.WeaponDamage) != 0 )
    {
        if ( localized )
            AddLocalizedProperty( list, "prop.damage.increase", prop );
        else
            list.Add( 1060401, prop.ToString() );
    }

    if ( (prop = m_AosAttributes.NightSight) != 0 )
    {
        if ( localized )
            AddLocalizedProperty( list, "prop.night.sight" );
        else
            list.Add( 1060441 );
    }
    // ...
}
```

**模式 B — 无参数的简单属性：**

```csharp
if ( BuildingPropertyListLocale != null )
{
    AddLocalizedProperty( list, "prop.balanced" );
}
else
{
    list.Add( 1072792 ); // Balanced
}
```

### 8.3 步骤三：改造 `AddNameProperty()`

物品名称需要按语言返回不同文本。参考 `BaseSpecial.cs`：

```csharp
public override bool IsContentLocalized => true;

public override void AddNameProperty( ObjectPropertyList list )
{
    string locale = BuildingPropertyListLocale;
    if ( locale != null )
    {
        string name = GetDisplayNameLocalized( m_Resource, Name, locale );
        if ( Amount > 1 )
            list.Add( 1050039, "{0}\t{1}", Amount, name );
        else
            list.Add( 1072171, "{0}\t{1}", "9F44FF", name );
        return;
    }
    base.AddNameProperty( list );  // 非双语路径回退
}
```

---

## 9. 典型案例

### 案例 1：武器属性展示

```csharp
// BaseWeapon.GetProperties — 属性值翻译
if ( localized )
    AddLocalizedProperty( list, "prop.hit.fireball", prop );
else
    list.Add( 1060420, prop.ToString() );

// → en: "hit fireball 50%"    → zh: "击中火球 50%"
```

### 案例 2：武器技能需求

```csharp
// BaseWeapon.GetProperties — 技能名翻译
if ( localized )
{
    switch ( Skill )
    {
        case SkillName.Swords:       AddLocalizedProperty( list, "prop.skill.swordsmanship" ); break;
        case SkillName.Bludgeoning:  AddLocalizedProperty( list, "prop.skill.bludgeoning" ); break;
        case SkillName.Fencing:      AddLocalizedProperty( list, "prop.skill.fencing" ); break;
        case SkillName.Marksmanship: AddLocalizedProperty( list, "prop.skill.marksmanship" ); break;
        case SkillName.FistFighting: AddLocalizedProperty( list, "prop.skill.fist.fighting" ); break;
    }
}
// → en: "skill required: swordsmanship"    → zh: "所需技能：剑术"
// → en: "skill required: fist fighting"    → zh: "所需技能：徒手格斗"
```

### 案例 3：特殊资源名称（含颜色）

```csharp
// Special.cs — 双语名称 + 紫色
public override void AddNameProperty( ObjectPropertyList list )
{
    string locale = BuildingPropertyListLocale;
    if ( locale != null )
    {
        string name = GetDisplayNameLocalized( m_Resource, Name, locale );
        if ( Amount > 1 )
            list.Add( 1050039, "{0}\t{1}", Amount, name );
        else
            list.Add( 1072171, "{0}\t{1}", "9F44FF", name );
        return;
    }
    base.AddNameProperty( list );
}
// → 中文玩家：紫色「圣辉 符文」  → 英文玩家：紫色「Holy Rune」
```

### 案例 4：抵抗属性

```csharp
// Item.AddResistanceProperties — 全部 5 系抵抗翻译
// → en: "physical resist 10%"    → zh: "物理抵抗 10%"
// → en: "fire resist 15%"       → zh: "火焰抵抗 15%"
```

### 案例 5：制造者标记

```csharp
// → en: "crafted by Forrrest"    → zh: "制造者：Forrrest"
```

### 案例 6：ColorText 多行彩色（非双语 OPL）

```csharp
// CurseItem.cs — 4 行红色文本
ColorText1 = "CURSED!";
ColorText3 = "Give to a Wizard or Knight";
ColorText4 = "To Remove the Curse";
ColorText5 = "Or Use Curse Removing Magic";
ColorHue1 = ColorHue3 = ColorHue4 = ColorHue5 = "E15656";

// 渲染到 OPL 时等价于：
list.Add( 1072171, "E15656\tCURSED!" );
list.Add( 1072173, "E15656\tGive to a Wizard or Knight" );
list.Add( 1072174, "E15656\tTo Remove the Curse" );
list.Add( 1072175, "E15656\tOr Use Curse Removing Magic" );
```

### 案例 7：InfoText（无颜色信息文本）

`InfoText1`–`InfoText5` 使用 cliloc `1070630`–`1070634`，**不进行颜色格式化**，显示为默认大小白色：

```csharp
// BaseReagent.cs
InfoText1 = "Reagent";

// Canteen.cs
InfoText1 = "alien liquid";
InfoText2 = "use to drink";

// BaseRunicTool.cs
InfoText2 = "Magically Crafts Items";
```

---

## 10. 性能说明

1. **缓存命中率：** 每个物品每种语言只构建一次 OPL，后续相同语言的玩家共享同一缓存。缓存随 `InvalidateProperties()` 清除。

2. **内存开销：** 每个启用了 `IsContentLocalized` 的物品最多多占用 2 个 `ObjectPropertyList`（zh + en），约几 KB。只在物品属性变化时重建。

3. **CPU 开销：** 仅在以下情况下触发 OPL 构建：
   - 物品第一次展示给某语言的玩家
   - 物品属性变化（属性值改变、附魔等）
   - 玩家首次登录（缓存此时构建）
   
   游戏中不会同时有大量玩家同时触发大量物品的 OPL 构建。

4. **扩展性：** 添加第三种语言时只需：
   - 在 `equipment-properties.json` 中新增对应语言的文件
   - 在 `Item.cs` 的 `SendPropertiesTo()` / `SendInfoTo()` 中扩展语言检测逻辑
   - 缓存字典自动增长
