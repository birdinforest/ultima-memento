## Craft attribute bundles (`CraftAttInfo`) applied to armour & weapons

`CraftAttributeInfo` stores independent **weapon** versus **armor** augmentation integers. Most gameplay surfaces copy the **durability**, **luck**, **lower requirement** trio into **both** branches via factory:

```65:83:World/Source/Scripts/System/Misc/ResourceInfo.cs
		public static CraftAttributeInfo CraftAttInfo( int armorphy, int armorfir, int armorcld, int armorpsn, int armoregy, object spacer, int weapcold, int weapfire, int weapengy, int weappois, object spacer2, int durable, int lowreq, int luck )
		{
			CraftAttributeInfo var = new CraftAttributeInfo();

			var.ArmorPhysicalResist = 		armorphy;
			var.ArmorColdResist = 			armorcld;
			var.ArmorFireResist = 			armorfir;
			var.ArmorEnergyResist = 		armoregy;
			var.ArmorPoisonResist = 		armorpsn;
			var.ArmorDurability = 			durable;
			var.ArmorLowerRequirements = 	lowreq;
			var.ArmorLuck = 				luck;
			var.WeaponColdDamage = 			weapcold;
			var.WeaponFireDamage = 			weapfire;
			var.WeaponEnergyDamage = 		weapengy;
			var.WeaponPoisonDamage = 		weappois;
			var.WeaponDurability = 			durable;
			var.WeaponLowerRequirements = 	lowreq;
			var.WeaponLuck = 				luck;

			return var;
		}
```

Parameter mnemonic (**positional**):

| Index | Formal arg | Mapped fields |
|---|---|---|
| 1–5 | `armorphy, armorfir, armorcld, armorpsn, armoregy` | Armour **phys/fire/cold/poison/energy resist** deltas |
| 6–7 | `spacer`, `spacer2` | Ignored placeholders (`null` in tables) |
| 8–11 | `weapcold,weapfire,weapengy,weappois` | Weapon **cold/fire/energy/poison** elemental damage percentages |
| 12–14 | `durable, lowreq, luck` | **Both** armour & weapon buckets receive identical ints |

Unused in factory pathway today (remain default **0**) but exist on struct: **`WeaponChaosDamage`, `WeaponDirectDamage`, `WeaponGoldIncrease`, `ArmorGoldIncrease`** — future balancing hooks.

High-signal illustrative rows (**metals excerpt** commentary already inline in **`ResourceInfo`** near static ctor):

| Material | Armour spread (phys fire cold poison energy) | Weapon elem (cold fire energy poison) | Dur / LReq / Luck |
|---|---|---|---|
| Valorite | 4 each (energy 3) | 20/10/20/10 | 40 / 0 / 0 |
| Dull Copper | baseline small resists | zeros | **75 / 35 / 0** (cheap lower-req smith band) |

**Spectral elemental specs** (`FireSpec` etc.) smash resist templates: **`FireSpec`** sets **armor fire 17**, others **0**, while assigning **weapon fire 100%** — asymmetric glass cannon designs.

Interpretation caveat: **percentage fields** accumulate with other crafting bonuses + Runic outcomes — QA should snapshot before/after on test shard when editing static tables.

Linkage to **`CraftResourceInfo`**:

| Field interplay | Behaviour summary |
|---|---|
| `Dmg` / `Arm` | Numeric weapon/armour power scaffolding separate from elemental resist ints |
| `WepArmor` | When **1**, server applies **half** of armour resist package to melee weapons (**per ctor comment**) |
| `AttributeInfo.Blank` | Iron / baseline wood/leather/board — no resist or elemental skew |
