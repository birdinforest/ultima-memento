## Reference tables — metal, scales, spectral / elemental special

Canonical array lines in **`ResourceInfo.cs`**: **`m_MetalInfo` L362–395**, **`m_ScaleInfo` L397–415**, **`m_SpecialInfo` L417–433**.

Legend for columns below:

| Col | Meaning |
|---|---|
| **CR** | `CraftResource` enum |
| **Name** | Designer label + Cliloc-backed craft strings (**not** identical to hue-specific creature colour names) |
| **D/A** | **`Dmg` / `Arm`** fields from ctor (weapon / armour deltas in crafting pipeline — see **`06`**) |
| **Gold** | Vendor-ish price multiplier scaffold |
| **Skill** | Craft / smelt gate |
| **Uses…Xtra** | `Uses`, `Weight`, `Bonus`, `Xtra` positional ctor fields (instruments, poles, barding tuning — contextual) |
| **WA** | `WepArmor` — **non-zero ⇒ weapons receive armour-bonus halves** (`m_WepArmor` comment in source) |
| **Cliloc³** | `CraftText`,`MaterialText`,`LowCaseText` message numbers |
| **Types** | `typeof(...)` list registered via `CraftResources.RegisterType` |

Implementation comment from ctor (**`Hue`/`Clr`/etc.**):

```333:349:World/Source/Scripts/System/Misc/ResourceInfo.cs
			m_Hue = hue;			// Hue for items
			m_Clr = clr;			// Hue for creatures
			m_Dmg = dmg;			// Damage Mod
			m_Arm = ar;				// Armor Mod
			m_Gold = gold;			// Gold Mod
			m_Skill = skill;		// Skill Required
			m_Uses = uses;			// Instrument & Fishing Pole Uses
			m_Weight = weight;		// Ten Foot Pole Weight
			m_Bonus = bonus;		// Ten Foot Pole & Fishing Pole Effectiveness
			m_Xtra = xtra;			// Horse Barding Bonus & Spyglass bonus
			m_WepArmor = weparm;	// Indicates if a Weapon will get Half of the Armor Bonuses
```

`CraftAttributeInfo` for **`Gold`** metal row is **`Golden`** (static instance name).

### `m_MetalInfo` — metals & alloys

| CR | Name | D/A | Gold | Skill | Uses,Wgt,Bon,Xtr | WA | Cliloc³ | Types |
|---|---|---:|---:|---:|---|:---:|---|---|
| Iron | Iron | 0/0 | 1.00 | 0 | 0,0,0,0 | 0 | 1044022,1044036,1053109 | `IronIngot`,`IronOre`,`Granite` |
| DullCopper | Dull Copper | 1/1 | 1.25 | 65 | 10,2,3,1 | 0 | 1044023,1074916,1053108 | `DullCopperIngot`,`DullCopperOre`,`DullCopperGranite` |
| ShadowIron | Shadow Iron | 1/2 | 1.50 | 70 | 20,4,6,2 | 0 | 1044024,1074917,1053107 | `ShadowIron*` |
| Copper | Copper | 2/3 | 1.75 | 75 | 30,6,9,3 | 0 | 1044025,1074918,1053106 | `Copper*` |
| Bronze | Bronze | 2/4 | 2.00 | 80 | 40,8,12,4 | 0 | 1044026,1074919,1053105 | `Bronze*` |
| Gold | Gold | 2/5 | 2.25 | 85 | 50,10,15,5 | 0 | 1044027,1074920,1053104 | `GoldIngot`,`GoldOre`,`GoldGranite` |
| Agapite | Agapite | 3/6 | 2.50 | 90 | 60,12,18,6 | 0 | 1044028,1074921,1053103 | `Agapite*` |
| Verite | Verite | 3/7 | 2.75 | 95 | 70,14,21,7 | 0 | 1044029,1074922,1053102 | `Verite*` |
| Valorite | Valorite | 3/8 | 3.00 | 99 | 80,16,24,8 | 0 | 1044030,1074923,1053101 | `Valorite*` |
| Nepturite | Nepturite | 3/9 | 3.10 | 99 | 90,18,27,9 | 0 | 1036173,1036174,1036175 | `Nepturite*` |
| Obsidian | Obsidian | 3/9 | 3.10 | 105 | 100,20,30,10 | 0 | 1036162,1036164,1036165 | `Obsidian*` |
| Steel | Steel | 4/10 | 3.25 | 105 | 110,22,33,11 | 0 | 1036144,1036145,1036146 | `SteelIngot` |
| Brass | Brass | 4/11 | 3.50 | 105 | 120,24,36,12 | 0 | 1036152,1036153,1036154 | `BrassIngot` |
| Mithril | Mithril | 5/12 | 3.75 | 115 | 130,26,39,13 | 0 | 1036137,1036138,1036139 | `MithrilIngot`,`MithrilOre`,`MithrilGranite` |
| Xormite | Xormite | 5/12 | 3.75 | 115 | 140,27,41,14 | **1** | 1034437,1034438,1034439 | `Xormite*` |
| Dwarven | Dwarven | 6/14 | 4.50 | 125 | 160,28,42,15 | **1** | 1036181,1036182,1036183 | `Dwarven*` |
| Agrinium | Agrinium | 5/13 | 4.25 | 117 | 150,27,41,14 | **1** | 1063982,1063983,1063981 | `AgriniumIngot` |
| Beskar … Xonolite | (same tier block) | 5/13 | 4.25 | 117 | 150,27,41,14 | **1** | custom 1063986…1064043 triplets | matching `*Ingot` only |

Rows **Beskar** through **Xonolite** mirror the **Agrinium** stat template (identical ctor numbers except hues + Cliloc + `Type`). See contiguous source lines **`L381–395`**.

---

### `m_ScaleInfo` — dragonish / exotic scales

All early tiers share **`D/A = 2/5`**, **`Gold 2.40`**, **`Skill 45`** except Cadalyte line ( **`4/9`**, **`3.40`**, **`115`**) and the four “sci‑fi reptile” lines (**`3/7`**, **`3.00`**, **`110`**).

| CR | Name | D/A | Gold | Skill | WA | Types | Notes |
|---|---|---:|---:|---:|:---:|---|---|
| RedScales | Crimson | 2/5 | 2.40 | 45 | 1 | `RedScales` | — |
| YellowScales | Golden | 2/5 | 2.40 | 45 | 1 | `YellowScales` | |
| BlackScales | Dark | … | … | … | … | … | |
| … | … | … | … | … | … | Through `PlatinumScales` | |
| CadalyteScales | Cadalyte | 4/9 | 3.40 | 115 | 1 | `CadalyteScales` | |
| **CadalyteScales*** | **Gorn** | 3/7 | 3.00 | 110 | 1 | `GornScales` | **Enum dup** — persisted as **`CadalyteScales`** in ctor (**`L412`**) |
| **CadalyteScales*** | **Trandoshan** | same | … | … | … | `TrandoshanScales` | |
| **CadalyteScales*** | **Silurian** | … | … | … | … | `SilurianScales` | |
| **CadalyteScales*** | **Krayt** | … | … | … | … | `KraytScales` | |

See design warning in **`01-domain-model-code-index.md`**.

---

### `m_SpecialInfo` — spectral slabs / elemental concentrates / Exodus / Turtle

| CR | Display Name | D/A | Gold | Skill | Uses,Wgt,Bon,Xtr | WA | Cliloc³ | Types |
|---|---|---:|---:|---:|---|:---:|---|---|
| SpectralSpec | Spectral | 3/10 | 3.00 | 110 | 130,26,42,13 | 1 | 1064088,1064102,1063811 | `SpectralSpec` |
| DreadSpec | Dread | … | … | … | … | 1 | 1064089… | … |
| GhoulishSpec | Ghoulish | … | … | … | … | 1 | 1064090… | … |
| WyrmSpec | Wyrm | … | … | … | … | 1 | 1064091… | … |
| HolySpec | Holy | … | … | … | … | 1 | 1064092… | … |
| BloodlessSpec | Bloodless | … | … | … | … | 1 | 1064093… | … |
| GildedSpec | Gilded | … | … | … | … | 1 | 1064094… | … |
| DemilichSpec | Demilich | … | … | … | … | 1 | 1064095… | … |
| WintrySpec | Wintry | … | … | … | … | 1 | 1064096… | … |
| FireSpec | Fire | **3/6** | **1.60** | **80** | **30,6,9,3** | 0 | 1064097,1064111,1064077 | `FireSpec` |
| ColdSpec | Cold | … | … | … | … | 0 | 1064098… | `ColdSpec` |
| PoisSpec | Venom | … | … | … | … | 0 | 1064099… | `PoisSpec` |
| EngySpec | Energy | … | … | … | … | 0 | 1064100… | `EngySpec` |
| ExodusSpec | Exodus | **4/16** | **4.20** | **120** | 150,27,41,14 | **1** | 1064101,1064115,1018194 | `ExodusSpec` |
| TurtleSpec | Turtle Shell | 3/10 | 3.00 | 110 | 130,26,42,13 | **0** | 1064116,1064117,1064119 | `TurtleSpec` |

**Attribute packages** (**`CraftAttributeInfo.FireSpec`** etc.) are extreme compared to armour spectral rows — see **`06`**.
