## Reference tables — leather, wood, fabric, blocks, skins, skeletal

Canonical arrays (**`World/Source/Scripts/System/Misc/ResourceInfo.cs`**):

| Array | Approx lines |
|---|---|
| `m_LeatherInfo` | L435–459 |
| `m_WoodInfo` | L461–485 |
| `m_FabricInfo` | L487–500 |
| `m_BlockInfo` | L502–518 |
| `m_SkinInfo` | L520–531 |
| `m_SkeletalInfo` | L533–560 |

Columns match **`03`** (`D/A`=Damage/Armor ctor fields; **WA**=`WepArmor`; **Cliloc³**=`CraftText`,`MaterialText`,`LowCaseText`).

### Naming note (`Spined` leather)

HUD **Name `"Deep Sea"`** maps to **`CraftResource.SpinedLeather`** (**`RegularLeather` tier analog** visually “spined/leather hides” naming in code). Designers should cite **either** folklore name (**Deep Sea**) **or** internal enum (**SpinedLeather**) consistently in specs.

---

### Leather + sci‑fi leathers (`m_LeatherInfo`)

Classic pipeline rows pair **`xxxLeather` + matching `xxxHides`** (except sci‑fi **`*Leather`** without separate hide typedef in ctor — see **`Adesote`…`Thermoweave`** lines).

| CR | Name | D/A | Gold | Skill | WA | Registered types |
|---|---|---:|---:|---:|:---:|---|
| RegularLeather | Normal | 0/0 | 1.00 | 0 | 0 | `Leather`,`Hides` |
| HornedLeather | Lizard | 1/1 | 1.25 | 55 | 0 | `HornedLeather`,`HornedHides` |
| BarbedLeather | Serpent | 1/2 | 1.50 | 60 | 0 | … |
| NecroticLeather | Necrotic | 2/3 | 1.50 | 65 | 0 | … |
| VolcanicLeather | Volcanic | 2/4 | 2.00 | 70 | 0 | … |
| FrozenLeather | Frozen | 2/5 | 1.75 | 75 | 0 | … |
| SpinedLeather | **Deep Sea** | 3/6 | 2.00 | 80 | 0 | `SpinedLeather`,`SpinedHides` |
| GoliathLeather | Goliath | 3/7 | 2.00 | 85 | 0 | … |
| DraconicLeather | Draconic | 3/8 | 2.25 | 90 | 0 | … |
| HellishLeather | Hellish | 4/9 | 2.25 | 100 | 0 | … |
| DinosaurLeather | Dinosaur | 4/10 | 3.00 | 105 | **1** | … |
| AlienLeather | Alien | 5/11 | 4.00 | 125 | **1** | … |
| Adesote…Thermoweave | (Sci‑fi names) | 5/12 | 3.75 | 110 | **1** | each `typeof( FooLeather )` only |

Sci‑fi block uses shared numeric template **150 uses / weight 24 / bonus 36 / xtra 12** besides hue + Cliloc.

---

### Wood (`m_WoodInfo`)

Base row registers plain **`Board` + `Log`**. Each exotic registers **`FooBoard`,`FooLog`** pairs until **Cosian onward** omit logs in ctor (**board-only smithing**) — inspect **`L478–485`** individually when scripting drops.

Structural tiers excerpt:

| CR | Name | D/A | Gold | Skill | WA | Highlights |
|---|---|---:|---:|---:|:---:|---|
| RegularWood | Normal | 0/0 | 1.00 | 0 | 0 | Vanilla board/log |
| AshTree→WalnutTree | *(wood names)* | escalating | … | … | mostly 0 | Board+log pairs |
| PetrifiedTree | Petrified | 3/12 | 3.25 | 115 | 0 | |
| DriftwoodTree | Driftwood | 3/13 | 2.40 | 115 | 0 | lower gold vs neighbours |
| ElvenTree | Elven | 4/14 | 3.40 | 125 | **1** | |
| BorlTree…VeshokTree | Star Wars‑flavour | 4/15 | 3.50 | 115 | **1** | Board-only registrations |

Harvest alignment: ordinary **`Lumberjacking`** emits **`AshLog→WalnutLog`** tiers; **`ElvenLog`** plus volatile banks appear in **`05`**.

---

### Fabric (`m_FabricInfo`)

Each row **`typeof( MatchingFabricClass )`** only.

| CR | Name | Skill | WA | Notes |
|---|---|---|:---:|---|
| Fabric | Normal | 0 | 0 | Baseline |
| FurryFabric | Furry | 45 | 0 | |
| … | … through Fiendish | up to **105** | Fiendish **WA=1** | |

The ctor stat ladder mirrors leather progression (consult lines **489–499**).

---

### Polished gemstone blocks (`m_BlockInfo`)

All standard blocks (**Amethyst→Topaz**) share **D/A 3/12**, **Gold 3.40**, **Skill 85**, **Uses 140**, tertiary stats **20/32/12**, **`WA=1`**, **`typeof( FooBlocks ), typeof( FooStone )`**.

**CaddelliteBlock** spikes to **D/A 4/16**, **Gold 4.00**, **Skill 115**, uses **200**, weight **28**, bonus **42**, **Xtra 15**.

---

### Skins (`m_SkinInfo`)

Rows **Demon→Unicorn**: **D/A 3/8**, skill **65**, uses **100**, weight **20**, bonus **30**, **Xtra 10**, **`WA=1`**.

**Icy–Dead** upgrade to **D/A 3/10**, skill **75**, uses **24/40/12**.

Craft names: **Dead** ⇒ `DeadSkin` resource but label **Dead** (**`CraftResourceInfo` Name** differs from **`Seaweed`** row).

---

### Skeletal bones (`m_SkeletalInfo`)

| Slice | Rows | Skill spread | Structural notes |
|---|---|---|---|
| Brittle baseline | **`BrittleSkeletal`** | 0 | Blank attributes |
| Humanoid/monster ladder | **`Drow`…`Draco`** | **55→105** | escalating D/A uses |
| **Xeno** | peak fantasy | **110** | D/A **5/18**, gold **3.00**, uses weight **180/28/38/16** |
| Alien humanoids block | **`Andorian`–`Zabrak`** | **100.1** skill gate | Uniform uses **140/26/37** etc. (**`WA=1`**) |

`Draco` row uses asymmetric hue pair **`0x437,0x698`** — verify art pipeline if swapping IDs.
