#!/usr/bin/env python3
"""
Batch-process C# files in the Magic/ directory to wrap SendMessage/Say
string literals through StringCatalog.Resolve.

Usage: python3 batch_wrap_localization.py

NOTE: This was a one-time transformation tool. Most files have already
been processed. Review git diff before re-running.
"""

import re
import os
import sys

MAGIC_BASE = '/Users/forrrest/projects/UO-Memento/ultima-memento/World/Source/Scripts/Engines and Systems/Magic'

FILES = [
    # Bard/Scrolls
    "Bard/Scrolls/ArmysPaeon.cs",
    "Bard/Scrolls/EnchantingEtude.cs",
    "Bard/Scrolls/EnergyCarol.cs",
    "Bard/Scrolls/EnergyThrenody.cs",
    "Bard/Scrolls/FireCarol.cs",
    "Bard/Scrolls/FireThrenody.cs",
    "Bard/Scrolls/FoeRequiem.cs",
    "Bard/Scrolls/IceCarol.cs",
    "Bard/Scrolls/IceThrenody.cs",
    "Bard/Scrolls/KnightsMinne.cs",
    "Bard/Scrolls/MagesBallad.cs",
    "Bard/Scrolls/MagicFinale.cs",
    "Bard/Scrolls/PoisonCarol.cs",
    "Bard/Scrolls/PoisonThrenody.cs",
    "Bard/Scrolls/SheepfoeMambo.cs",
    "Bard/Scrolls/SinewyEtude.cs",
    # Bard/Spells
    "Bard/Spells/ArmysPaeonSong.cs",
    "Bard/Spells/EnchantingEtudeSong.cs",
    "Bard/Spells/EnergyCarolSong.cs",
    "Bard/Spells/EnergyThrenodySong.cs",
    "Bard/Spells/FireCarolSong.cs",
    "Bard/Spells/FireThrenodySong.cs",
    "Bard/Spells/FoeRequiemSong.cs",
    "Bard/Spells/IceCarolSong.cs",
    "Bard/Spells/IceThrenodySong.cs",
    "Bard/Spells/KnightsMinneSong.cs",
    "Bard/Spells/MagesBalladSong.cs",
    "Bard/Spells/PoisonCarolSong.cs",
    "Bard/Spells/PoisonThrenodySong.cs",
    "Bard/Spells/SheepfoeMamboSong.cs",
    "Bard/Spells/SinewyEtudeSong.cs",
    "Bard/SongSpells.cs",
    # Base
    "Base/SpellItemInfo.cs",
    # Death Knight
    "Death Knight/DeathKnightSpell.cs",
    "Death Knight/DeathKnightSpellBook.cs",
    "Death Knight/DeathSkulls.cs",
    "Death Knight/SoulLantern.cs",
    "Death Knight/Spells/DevilPactSpell.cs",
    "Death Knight/Spells/HagHandSpell.cs",
    "Death Knight/Spells/HellfireSpell.cs",
    "Death Knight/Spells/ShieldOfHateSpell.cs",
    "Death Knight/Spells/SoulReaperSpell.cs",
    "Death Knight/Spells/SuccubusSkinSpell.cs",
    # Druidism
    "Druidism/Effects/DruidicRuneSpell.cs",
    "Druidism/Effects/FireflySpell.cs",
    "Druidism/Effects/MushroomGatewaySpell.cs",
    "Druidism/Effects/NaturesPassageSpell.cs",
    "Druidism/Effects/RestorativeSoilSpell.cs",
    "Druidism/Effects/StoneCircleSpell.cs",
    "Druidism/Effects/TreefellowSpell.cs",
    "Druidism/HerbalistPotions.cs",
    # Elementalism
    "Elementalism/ElementalShrine.cs",
    "Elementalism/ElementalSpell.cs",
    "Elementalism/Sphere 1/Elemental_Sanctuary.cs",
    "Elementalism/Sphere 4/Elemental_Void.cs",
    "Elementalism/Sphere 5/Elemental_Echo.cs",
    "Elementalism/Sphere 5/Elemental_Fiend.cs",
    "Elementalism/Sphere 6/Elemental_Rune.cs",
    "Elementalism/Sphere 7/Elemental_Gate.cs",
    "Elementalism/Sphere 8/Elemental_Soul.cs",
    "Elementalism/Sphere 8/Elemental_Spirit.cs",
    "Elementalism/ElementalEffect.cs",
    "Elementalism/ElementalCommands.cs",
    # Holy Man
    "Holy Man/HolyManSpell.cs",
    "Holy Man/HolySymbol.cs",
    "Holy Man/HolySymbols.cs",
    "Holy Man/MalletStake.cs",
    "Holy Man/Spells/DampenSpiritSpell.cs",
    "Holy Man/Spells/EnchantSpell.cs",
    "Holy Man/Spells/HammerOfFaithSpell.cs",
    "Holy Man/Spells/HeavenlyLightSpell.cs",
    "Holy Man/Spells/NourishSpell.cs",
    "Holy Man/Spells/RebirthSpell.cs",
    "Holy Man/Spells/SacredBoonSpell.cs",
    "Holy Man/Spells/SeanceSpell.cs",
    "Holy Man/Spells/TrialByFireSpell.cs",
    "Holy Man/Spells/TouchOfLifeSpell.cs",
    "Holy Man/Spells/SanctifySpell.cs",
    # Jedi
    "Jedi/JediDatacrons.cs",
    "Jedi/JediSpell.cs",
    "Jedi/JediSpellbook.cs",
    "Jedi/KaranCrystal.cs",
    "Jedi/Spells/Celerity.cs",
    "Jedi/Spells/Deflection.cs",
    "Jedi/Spells/ForceGrip.cs",
    "Jedi/Spells/MindsEye.cs",
    "Jedi/Spells/Mirage.cs",
    "Jedi/Spells/Replicate.cs",
    "Jedi/Spells/ThrowSabre.cs",
    "Jedi/Spells/SoothingTouch.cs",
    "Jedi/Spells/PsychicAura.cs",
    "Jedi/Spells/StasisField.cs",
    # Jester
    "Jester/JesterCommandList.cs",
    "Jester/JesterSpell.cs",
    "Jester/Spells/Insult.cs",
    "Jester/Spells/Clowns.cs",
    "Jester/Spells/CanOfSnakes.cs",
    "Jester/Spells/Hilarity.cs",
    "Jester/Spells/JumpAround.cs",
    "Jester/Spells/PoppingBalloon.cs",
    "Jester/Spells/RabbitInAHat.cs",
    "Jester/Spells/SurpriseGift.cs",
    # Knight
    "Knight/BookOfChivalry.cs",
    "Knight/PaladinSpell.cs",
    "Knight/RemoveCurse.cs",
    "Knight/SacredJourney.cs",
    "Knight/EnemyOfOne.cs",
    # Magery
    "Magery/Spellbook.cs",
    "Magery/Spells/Magery 1st/CreateFood.cs",
    "Magery/Spells/Magery 1st/NightSight.cs",
    "Magery/Spells/Magery 2nd/MagicTrap.cs",
    "Magery/Spells/Magery 2nd/RemoveTrap.cs",
    "Magery/Spells/Magery 3rd/MagicLock.cs",
    "Magery/Spells/Magery 3rd/Telekinesis.cs",
    "Magery/Spells/Magery 3rd/Unlock.cs",
    "Magery/Spells/Magery 4th/Recall.cs",
    "Magery/Spells/Magery 5th/BladeSpirits.cs",
    "Magery/Spells/Magery 5th/Incognito.cs",
    "Magery/Spells/Magery 5th/MagicReflect.cs",
    "Magery/Spells/Magery 6th/Mark.cs",
    "Magery/Spells/Magery 6th/Reveal.cs",
    "Magery/Spells/Magery 7th/GateTravel.cs",
    "Magery/Spells/Magery 8th/EnergyVortex.cs",
    "Magery/Spells/Magery 8th/Resurrection.cs",
    # Misc
    "Misc/AttackSpells.cs",
    "Misc/SummonDragonSpell.cs",
    "Misc/SummonSnakesSpell.cs",
    "Misc/TravelSpell.cs",
    # Mystic
    "Mystic/MysticMonkRobe.cs",
    "Mystic/MysticPack.cs",
    "Mystic/MysticSpell.cs",
    "Mystic/Scrolls/CreateRobeScroll.cs",
    "Mystic/Scrolls/GentleTouchScroll.cs",
    "Mystic/Spells/AstralProjection.cs",
    "Mystic/Spells/AstralTravel.cs",
    "Mystic/Spells/Leap.cs",
    "Mystic/Spells/PsychicWall.cs",
    "Mystic/Spells/PurityOfBody.cs",
    "Mystic/Spells/QuiveringPalm.cs",
    "Mystic/Spells/WindRunner.cs",
    # Necromancy
    "Necromancy/AnimateDeadSpell.cs",
    "Necromancy/BloodOathSpell.cs",
    "Necromancy/CorpseSkinSpell.cs",
    "Necromancy/CurseWeaponSpell.cs",
    "Necromancy/EvilOmenSpell.cs",
    "Necromancy/Exorcism.cs",
    "Necromancy/HorrificBeast.cs",
    "Necromancy/LichFormSpell.cs",
    "Necromancy/MindRot.cs",
    "Necromancy/NecromancerSpellbook.cs",
    "Necromancy/PainSpike.cs",
    "Necromancy/PoisonStrike.cs",
    "Necromancy/StrangleSpell.cs",
    "Necromancy/VampiricEmbrace.cs",
    "Necromancy/VengefulSpiritSpell.cs",
    "Necromancy/WitherSpell.cs",
    "Necromancy/WraithForm.cs",
    # Ninjitsu
    "Ninjitsu/Backstab.cs",
    "Ninjitsu/BookOfNinjitsu.cs",
    "Ninjitsu/DeathStrike.cs",
    "Ninjitsu/FocusAttack.cs",
    "Ninjitsu/SurpriseAttack.cs",
    # Research
    "Research/AncientSpellBook.cs",
    "Research/ResearchFunctions.cs",
    "Research/ResearchSpell.cs",
    "Research/Spells/Conjuration/ResearchAerialServant.cs",
    "Research/Spells/Conjuration/ResearchCreateGold.cs",
    "Research/Spells/Conjuration/ResearchDeathVortex.cs",
    "Research/Spells/Conjuration/ResearchExtinguish.cs",
    "Research/Spells/Conjuration/ResearchSwarm.cs",
    "Research/Spells/Death/ResearchMaskofDeath.cs",
    "Research/Spells/Death/ResearchOpenGround.cs",
    "Research/Spells/Death/ResearchRockFlesh.cs",
    "Research/Spells/Death/ResearchWithstandDeath.cs",
    "Research/Spells/Death/ResearchDeathSpeak.cs",
    "Research/Spells/Enchanting/ResearchCauseFear.cs",
    "Research/Spells/Enchanting/ResearchCharm.cs",
    "Research/Spells/Enchanting/ResearchEnchant.cs",
    "Research/Spells/Enchanting/ResearchMassSleep.cs",
    "Research/Spells/Enchanting/ResearchSleep.cs",
    "Research/Spells/Enchanting/ResearchSleepField.cs",
    "Research/Spells/Sorcery/ResearchConflagration.cs",
    "Research/Spells/Sorcery/ResearchCreateFire.cs",
    "Research/Spells/Sorcery/ResearchEndureCold.cs",
    "Research/Spells/Sorcery/ResearchEndureHeat.cs",
    "Research/Spells/Sorcery/ResearchExplosion.cs",
    "Research/Spells/Sorcery/ResearchFlameBolt.cs",
    "Research/Spells/Sorcery/ResearchIgnite.cs",
    "Research/Spells/Sorcery/ResearchRingofFire.cs",
    "Research/Spells/Thaumaturgy/ResearchBanishDaemon.cs",
    "Research/Spells/Thaumaturgy/ResearchCallDestruction.cs",
    "Research/Spells/Thaumaturgy/ResearchConfusionBlast.cs",
    "Research/Spells/Thaumaturgy/ResearchDevastation.cs",
    "Research/Spells/Thaumaturgy/ResearchEtherealTravel.cs",
    "Research/Spells/Thaumaturgy/ResearchMeteorShower.cs",
    "Research/Spells/Theurgy/ResearchDivination.cs",
    "Research/Spells/Theurgy/ResearchRestoration.cs",
    "Research/Spells/Theurgy/ResearchSeeTruth.cs",
    "Research/Spells/Theurgy/ResearchWizardEye.cs",
    "Research/Spells/Wizardry/ResearchAvalanche.cs",
    "Research/Spells/Wizardry/ResearchFrostField.cs",
    "Research/Spells/Wizardry/ResearchGasCloud.cs",
    "Research/Spells/Wizardry/ResearchHailStorm.cs",
    "Research/Spells/Wizardry/ResearchMassDeath.cs",
    "Research/Spells/Wizardry/ResearchSnowBall.cs",
    # Shinobi
    "Shinobi/ShinobiCommandList.cs",
    "Shinobi/ShinobiSpell.cs",
    "Shinobi/Spells/CheetahPaws.cs",
    "Shinobi/Spells/Deception.cs",
    "Shinobi/Spells/EagleEye.cs",
    "Shinobi/Spells/Espionage.cs",
    "Shinobi/Spells/FerretFlee.cs",
    "Shinobi/Spells/MonkeyLeap.cs",
    "Shinobi/Spells/MysticShuriken.cs",
    # Syth
    "Syth/HellShard.cs",
    "Syth/SythDatacrons.cs",
    "Syth/SythSpell.cs",
    "Syth/SythSpellbook.cs",
    "Syth/Spells/Absorption.cs",
    "Syth/Spells/Clone.cs",
    "Syth/Spells/DeathGrip.cs",
    "Syth/Spells/DrainLife.cs",
    "Syth/Spells/Projection.cs",
    "Syth/Spells/Psychokinesis.cs",
    "Syth/Spells/Speed.cs",
    "Syth/Spells/ThrowSword.cs",
    # Witch
    "Witch/Effects/BloodPact.cs",
    "Witch/Effects/GraveyardGateway.cs",
    "Witch/Effects/HellsBrand.cs",
    "Witch/Effects/HellsGate.cs",
    "Witch/Effects/NecroUnlock.cs",
    "Witch/Effects/Phantasm.cs",
    "Witch/Effects/UndeadEyes.cs",
    "Witch/Effects/VampireGift.cs",
    "Witch/Effects/WallOfSpikes.cs",
    "Witch/UndeadSpell.cs",
    "Witch/WitchBrews.cs",
]

KNOWN_INCANTATIONS = {
    "Xtee Mee Glau",
    "Anh Mi Sah Ko",
}


def is_incantation(text):
    """Check if a string looks like a spell incantation (not user-facing copy)."""
    if text in KNOWN_INCANTATIONS:
        return True
    # All words start with uppercase and there are 2+ words
    words = text.split()
    if len(words) >= 2 and all(w[0].isupper() for w in words):
        return True
    return False


def should_skip_string(text):
    """Return True if this string should NOT be wrapped."""
    # Emotes: start with *
    if text.startswith('*'):
        return True
    # Empty strings
    if not text.strip():
        return True
    # Incantations
    if is_incantation(text):
        return True
    return False


def process_file(filepath):
    """Process a single .cs file, wrapping SendMessage/Say literals."""
    full_path = os.path.join(MAGIC_BASE, filepath)

    if not os.path.exists(full_path):
        print(f"  SKIP (not found): {filepath}")
        return 0

    with open(full_path, 'r', encoding='utf-8', errors='replace') as f:
        content = f.read()

    original = content
    changes = 0

    # Regex: var_name.SendMessage("literal") or var_name.Say("literal")
    # Captures:
    #   group(1): variable expression (e.g. "from", "Caster", "m_Caster", "m_From", "m.TargetMobile")
    #   group(2): method name (SendMessage or Say)
    #   group(3): string literal content (without quotes)
    pattern = r'(\w+(?:\.\w+)*)\.(SendMessage|Say)\(\s*"((?:[^"\\]|\\.)*)"\s*\)'

    def wrap_match(m):
        nonlocal changes
        var_name = m.group(1)
        method = m.group(2)
        string_content = m.group(3)

        if should_skip_string(string_content):
            return m.group(0)

        changes += 1
        return f'{var_name}.{method}( StringCatalog.Resolve( {var_name}.Account, "{string_content}" ) )'

    content = re.sub(pattern, wrap_match, content)

    if changes == 0:
        return 0

    # Add using Server.Localization if not present
    if 'using Server.Localization;' not in content:
        lines = content.split('\n')
        last_using_idx = -1
        for i, line in enumerate(lines):
            trimmed = line.strip()
            if trimmed.startswith('using ') and trimmed.endswith(';') and '/*' not in trimmed:
                last_using_idx = i
        if last_using_idx >= 0:
            lines.insert(last_using_idx + 1, 'using Server.Localization;')
            content = '\n'.join(lines)

    with open(full_path, 'w', encoding='utf-8') as f:
        f.write(content)

    return changes


def main():
    total_files = 0
    total_changes = 0
    per_file = []

    print("=" * 70)
    print("BATCH LOCALIZATION WRAPPER")
    print("Wrapping SendMessage/Say string literals with StringCatalog.Resolve")
    print("=" * 70)

    for filepath in FILES:
        print(f"\n  Processing: {filepath}")
        changes = process_file(filepath)
        if changes > 0:
            total_files += 1
            total_changes += changes
            per_file.append((filepath, changes))
            print(f"    → {changes} change(s)")
        else:
            print(f"    → No changes needed")

    print("\n" + "=" * 70)
    print(f"SUMMARY: {total_files} files modified, {total_changes} total changes")
    print("=" * 70)
    print("\nFiles modified:")
    for fpath, count in per_file:
        print(f"  {fpath}: {count}")
    print()

    return 0


if __name__ == '__main__':
    sys.exit(main())
