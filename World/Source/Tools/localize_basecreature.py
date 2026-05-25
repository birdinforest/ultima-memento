"""
Localize hardcoded English strings in BaseCreature.cs.
Preserves original line endings.
"""
import re

BASE_PATH = "/Users/forrrest/projects/UO-Memento/ultima-memento/World/Source/Scripts/Mobiles/Base/BaseCreature.cs"

with open(BASE_PATH, 'rb') as f:
    raw = f.read()

if b'\r\n' in raw:
    newline = '\r\n'
else:
    newline = '\n'

content = raw.decode('utf-8')
lines = content.split(newline)

modifications = 0

# Map: 0-based line index → (old_string, new_string)
CHANGES = {
    # Breath attacks
    1060: ('target.SendMessage( "You are hit by the force of the mighty roar!" )',
           'target.SendMessage( Server.Localization.StringCatalog.Resolve( target.Account, "You are hit by the force of the mighty roar!" ) )'),
    1066: ('target.SendMessage( "You are hit by a manticore thorn!" )',
           'target.SendMessage( Server.Localization.StringCatalog.Resolve( target.Account, "You are hit by a manticore thorn!" ) )'),
    1266: ('target.SendMessage( "You feel your soul draining!" )',
           'target.SendMessage( Server.Localization.StringCatalog.Resolve( target.Account, "You feel your soul draining!" ) )'),
    1285: ('target.SendMessage( "You feel your soul draining!" )',
           'target.SendMessage( Server.Localization.StringCatalog.Resolve( target.Account, "You feel your soul draining!" ) )'),

    # Subjugation
    2099: ('"* The creature has been beaten into subjugation! *"',
           'Server.Localization.StringCatalog.Resolve( this.Account, "* The creature has been beaten into subjugation! *" )'),

    # Franken sever messages
    4408: ('from.SendMessage("You sever off the giant\'s left leg.")',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You sever off the giant\'s left leg." ) )'),
    4409: ('from.SendMessage("You sever off the giant\'s right leg.")',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You sever off the giant\'s right leg." ) )'),
    4410: ('from.SendMessage("You sever off the giant\'s left arm.")',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You sever off the giant\'s left arm." ) )'),
    4411: ('from.SendMessage("You sever off the giant\'s right arm.")',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You sever off the giant\'s right arm." ) )'),
    4412: ('from.SendMessage("You sever off the giant\'s head.")',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You sever off the giant\'s head." ) )'),
    4413: ('from.SendMessage("You sever apart the giant\'s torso.")',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You sever apart the giant\'s torso." ) )'),
    4414: ('from.SendMessage("You remove the giant\'s fresh brain.")',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You remove the giant\'s fresh brain." ) )'),

    # Corpse carving
    4551: ('from.SendMessage( "You cut away some furs and they are on the corpse." )',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You cut away some furs and they are on the corpse." ) )'),
    4576: ('from.SendMessage( "You cut away some leather and they are on the corpse." )',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You cut away some leather and they are on the corpse." ) )'),
    4604: ('from.SendMessage( "You carve away some wood and they are on the corpse." )',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You carve away some wood and they are on the corpse." ) )'),
    4632: ('from.SendMessage( "You chisel away some granite and it is on the corpse." )',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You chisel away some granite and it is on the corpse." ) )'),
    4653: ('from.SendMessage( "You cut away some skins and they are on the corpse." )',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You cut away some skins and they are on the corpse." ) )'),
    4755: ('from.SendMessage( "You chip away some stones and they are on the corpse." )',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You chip away some stones and they are on the corpse." ) )'),
    4808: ('from.SendMessage( "You chip away some metal and it is on the corpse." )',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You chip away some metal and it is on the corpse." ) )'),
    4843: ('from.SendMessage( "You cut away some scales and they are on the corpse." )',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You cut away some scales and they are on the corpse." ) )'),
    4920: ('from.SendMessage( "You cut away some bones and they are on the corpse." )',
           'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You cut away some bones and they are on the corpse." ) )'),

    # Dispel: DispelDifficulty
    6517: ('"Dispel prevented (DispelDifficulty)"',
           'Server.Localization.StringCatalog.Resolve( defender.Account, "Dispel prevented (DispelDifficulty)" )'),
    # Dispel: Low skill
    6586: ('"Dispel prevented (Low skill)"',
           'Server.Localization.StringCatalog.Resolve( m.Account, "Dispel prevented (Low skill)" )'),
    # Dispel: Magery
    6597: ('"Dispel prevented (Magery)"',
           'Server.Localization.StringCatalog.Resolve( m.Account, "Dispel prevented (Magery)" )'),
    # Dispel: Mana
    6603: ('"Dispel prevented (Mana)"',
           'Server.Localization.StringCatalog.Resolve( m.Account, "Dispel prevented (Mana)" )'),
    # Dispel: Slayer
    6628: ('"Dispel chance increased (Slayer)"',
           'Server.Localization.StringCatalog.Resolve( m.Account, "Dispel chance increased (Slayer)" )'),
    # Dispel: Failed
    6639: ('"Dispel prevented (Failed)"',
           'Server.Localization.StringCatalog.Resolve( m.Account, "Dispel prevented (Failed)" )'),
    # Dispel: incoming format
    6644: ('string.Format("Dispel incoming ({0} > {1})", successChance, dispelFailureChance)',
           'Server.Localization.StringCatalog.ResolveFormat( m.Account, "Dispel incoming ({0} > {1})", successChance, dispelFailureChance )'),
    # Dispel: defensive
    6671: ('"Defensively Dispelled"',
           'Server.Localization.StringCatalog.Resolve( m.Account, "Defensively Dispelled" )'),

    # Stealing format
    6571: ('attacker.SendMessage( "You " + stole + " " + coins + " " + m_CoinType + "!" )',
           'attacker.SendMessage( Server.Localization.StringCatalog.ResolveFormat( attacker.Account, "You {0} {1} {2}!", stole, coins, m_CoinType ) )'),

    # Skill raise
    7039: ('m.SendMessage( "Make sure this skill is marked to raise. If you are near the skill cap you may need to lose some points in another skill first.")',
           'm.SendMessage( Server.Localization.StringCatalog.Resolve( m.Account, "Make sure this skill is marked to raise. If you are near the skill cap you may need to lose some points in another skill first." ) )'),

    # Peacemaking resist (L7936, L7975, L8050)
    7935: ('target.SendMessage( "You magically resist the affects of the song." )',
           'target.SendMessage( Server.Localization.StringCatalog.Resolve( target.Account, "You magically resist the affects of the song." ) )'),
    7974: ('target.SendMessage( "You magically resist the affects of the song." )',
           'target.SendMessage( Server.Localization.StringCatalog.Resolve( target.Account, "You magically resist the affects of the song." ) )'),
    8049: ('target.SendMessage( "You magically resist the affects of the song." )',
           'target.SendMessage( Server.Localization.StringCatalog.Resolve( target.Account, "You magically resist the affects of the song." ) )'),

    # Suppress
    7978: ('target.SendMessage("You hear jarring music, suppressing your abilities.")',
           'target.SendMessage( Server.Localization.StringCatalog.Resolve( target.Account, "You hear jarring music, suppressing your abilities." ) )'),
    # Hypnotic
    8072: ('target.SendMessage("The music is hypnotic, making you remove your worn items.")',
           'target.SendMessage( Server.Localization.StringCatalog.Resolve( target.Account, "The music is hypnotic, making you remove your worn items." ) )'),

    # Deathknight
    8457: ('deathknight.SendMessage( "A soul has been claimed." )',
           'deathknight.SendMessage( Server.Localization.StringCatalog.Resolve( deathknight.Account, "A soul has been claimed." ) )'),
    # Holyman
    8492: ('holyman.SendMessage( "Evil has been banished." )',
           'holyman.SendMessage( Server.Localization.StringCatalog.Resolve( holyman.Account, "Evil has been banished." ) )'),

    # Vendor cannot be harmed
    8975: ('SendMessage( "{0} the vendor cannot be harmed.", target.Name )',
           'SendMessage( Server.Localization.StringCatalog.ResolveFormat( this.Account, "{0} the vendor cannot be harmed.", target.Name ) )'),
    8977: ('SendMessage( "{0} {1} cannot be harmed.", target.Name, target.Title )',
           'SendMessage( Server.Localization.StringCatalog.ResolveFormat( this.Account, "{0} {1} cannot be harmed.", target.Name, target.Title ) )'),

    # Pet: died
    9992: ('ControlMaster.SendMessage("Your pet {0} has died!", Name)',
           'ControlMaster.SendMessage( Server.Localization.StringCatalog.ResolveFormat( ControlMaster.Account, "Your pet {0} has died!", Name ) )'),
    # Pet: friend's pet died
    9998: ('f.SendMessage("Your friend {0}\'s pet {1} has died!", ControlMaster, Name)',
           'f.SendMessage( Server.Localization.StringCatalog.ResolveFormat( f.Account, "Your friend {0}\'s pet {1} has died!", ControlMaster, Name ) )'),
    # Pet: gained exp
    10030: ('ControlMaster.SendMessage("Your pet {0} has gained {1} experience!", Name, exp)',
            'ControlMaster.SendMessage( Server.Localization.StringCatalog.ResolveFormat( ControlMaster.Account, "Your pet {0} has gained {1} experience!", Name, exp ) )'),
    # Pet: lost exp
    10060: ('ControlMaster.SendMessage("Your pet {0} has lost {1} experience!", Name, exp)',
            'ControlMaster.SendMessage( Server.Localization.StringCatalog.ResolveFormat( ControlMaster.Account, "Your pet {0} has lost {1} experience!", Name, exp ) )'),
    # Pet: gained trait
    10128: ('ControlMaster.SendMessage("Your pet has gained {0} trait{1}!", bonus, (bonus == 1 ? "" : "s"))',
            'ControlMaster.SendMessage( Server.Localization.StringCatalog.ResolveFormat( ControlMaster.Account, "Your pet has gained {0} trait{1}!", bonus, (bonus == 1 ? "" : "s") ) )'),
    # Pet: decreased level
    10154: ('ControlMaster.SendMessage("Your pet has decreased in level!")',
            'ControlMaster.SendMessage( Server.Localization.StringCatalog.Resolve( ControlMaster.Account, "Your pet has decreased in level!" ) )'),
    # Pet: trusts you
    10181: ('ControlMaster.SendMessage( "Your pet trusts you implicitly. It will be easier to control now." )',
            'ControlMaster.SendMessage( Server.Localization.StringCatalog.Resolve( ControlMaster.Account, "Your pet trusts you implicitly. It will be easier to control now." ) )'),
    # Pet: new level
    10205: ('ControlMaster.SendMessage("Your pet is now level {0}.", newLevel)',
            'ControlMaster.SendMessage( Server.Localization.StringCatalog.ResolveFormat( ControlMaster.Account, "Your pet is now level {0}.", newLevel ) )'),
}

for line_idx, (old, new_val) in sorted(CHANGES.items()):
    line = lines[line_idx]
    if old in line:
        new_line = line.replace(old, new_val, 1)
        lines[line_idx] = new_line.rstrip('\r\n')
        modifications += 1
    else:
        print(f"WARNING: L{line_idx+1} not found: {old[:70]}...")

output = newline.join(lines)

with open(BASE_PATH, 'wb') as f:
    f.write(output.encode('utf-8'))

print(f"\nBaseCreature.cs: {modifications} changes made out of {len(CHANGES)} expected")
