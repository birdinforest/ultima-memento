using System; 
using Server; 
using Server.Items; 
using Server.Network; 
using Server.Misc; 
using Server.Gumps;
using Server.Localization;

namespace Server.Items
{
	public class BeginnerBook : Item
	{
		[Constructable]
		public BeginnerBook() : base( 0x0FF1 )
		{
			Name = "The Journey Begins";
		}

		public override void OnDoubleClick( Mobile from )
		{
			// Temporarily Disable
			return;
			if ( from.InRange( GetWorldLocation(), 1 ) )
			{
				from.CloseGump( typeof( BeginnerBookGump ) );
				from.SendGump( new BeginnerBookGump( from, 1 ) );
			}
		}

		public void TitleBook()
		{
			if ( ColorText1 == null && X > 0 )
			{
				ColorText1 = "The Journey Begins";
				ColorText2 = "How to start a new";
				ColorText3 = "life in this world";
				ColorHue1 = "FF9900";
				ColorHue2 = "B57B24";
				ColorHue3 = "B57B24";
			}
		}

        public override void OnAfterSpawn()
        {
			TitleBook();
			base.OnAfterSpawn();
		}

		public override void OnAdded( object parent )
		{
			TitleBook();
			base.OnAdded( parent );
		}

		public override void OnLocationChange( Point3D oldLocation )
		{
			TitleBook();
			base.OnLocationChange( oldLocation );
		}

		public BeginnerBook( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}

namespace Server.Gumps 
{ 
	public class BeginnerBookGump : Gump 
	{
		private static string ResolveText( Mobile from, string text )
		{
			string lang = AccountLang.GetLanguageCode( from.Account );
			return StringCatalog.TryResolve( lang, text ) ?? text;
		}

		private static bool UseLocalizedTextMode( Mobile from )
		{
			return AccountLang.GetLanguageCode( from.Account ) != "en";
		}

		private static string GetLocalizedBody( Mobile from )
		{
			return ResolveText( from, "BASICS OF THE GAME<BR><BR>Playing in the " ) + MySettings.S_ServerName + ResolveText( from, " is pretty simple as the interface is quite intuitive. Although the game is over 20 years old, some explanation is in order. After you login and are in the game world, you will see a book open with abilities. Right click on this book to make it close as you will not need it for this game. Almost any window can be closed with a right click. Your character is always in the center of the playing screen. To travel, simply move the mouse over the game world display... then right click and hold. The mouse cursor will always point away from your character, who will move in the indicated direction (for example, if you wish to walk up the screen, hold the cursor above your character). You will continue to head in that direction until you come to an obstruction or release the mouse button. The further away the cursor is from your character, the faster the character will move. Double right clicking will cause your character to move to the exact point where the cursor was... unless disabled in the options.<br><br>PAPERDOLL: Your character paperdoll will be open when you start. If it is not, pressing Alt P will open it for you. Below I will explain what this does. The left side shows boxes for the slot showing what is on your head, then the slot showing what is on your ears, then the slot showing what is on your neck, then the slot showing what is on your finger, lastly the slot showing what is on your wrist. The bottom will show your name and your title. Sometimes it is custom, while mostly it is your best practiced skill along with any fame and/or karma you gained. The right side has various buttons. Pressing the HELP button brings up a simple help menu. The only thing that you should ever use here is when your character is physically stuck in the game world and needs to be teleported out. It will teleport you into a safe area somewhere in the land. The OPTIONS button will bring up your options for the game (discussed later). The LOG OUT button Logs you out of the game. Make sure you are in a safe place. The STATS button will bring up some vital stats about your character (discussed later). The SKILLS button will bring up all of the skill available in the game. Here you manage your skill progression (discussed later). The GUILD button enables you to start your own guild. It will cost money to get started, but you can invite other players and share homes and chat with each other. The PEACE button toggles whether you are ready to fight... or not. Lastly, the STATUS button will bring up the status of your character (discussed later). The center shows your character. Here you can drag and drop clothing, armor, and other equipment worn by your character. Double click the left scroll to see how old your account is. Double click the right scroll to organize a party of other players. This is important if you plan to share the rewards of dungeon delving. Double click the backpack to open your backpack (discussed later).<br><br>MENU BAR: This menu bar can allow you to get access to certain items quicker. It can be disabled in the options. The small triangle will minimize the menu bar. The MAP button will open a mini map of your surrounding area. Pressing it a second time will make the map a bit larger (Alt R does this as well). The PAPERDOLL button will open your paperdoll. The INVENTORY button will open your backpack (discussed later). The JOURNAL button will open your journal, which shows the most recent things you saw or heard. The CHAT button does not work as the chat option is disabled. Type the command [c instead. The HELP button will bring up the help menu that was already discussed. The ? button brings up a very outdated information screen. It is best not to use it.<br><br>BACKPACK: When you double click the backpack on the paperdoll, it will open (Alt I will do that as well). You can only carry a certain amount of weight based off of your strength. If you strength is extremely high, then you are at the mercy at how much the backpack can actually hold. The image on the right shows how you can actually have containers within the backpack to help organize things better. You can drag and drop items between the containers. Sometimes your containers will close when you travel between different worlds. If you close a container, that has other container from within it open, those containers will also close. OPTIONS Pressing the options button will open this window (as will pressing Alt O). You can change many things in the options section. You can control the volume of music and sounds. You can change the fonts and colors of such fonts. You can setup macros to create shortcut keys for commonly used series of commands. You can also filter obscenities. This is also where you can set your pathfinding, war mode, targeting system, and menu bar options. You can choose to offset your interface windows (like containers) when opening. Pay attention to the macro options, as you can learn about some of the pre built shortcut keys... along with learning how to steer ships when you can afford to buy one.<br><br>STATS: There are many aspects of your character, and the stats button will display this for you. You can see what comprises of your abilities and if you have any bonuses to regeneration of statistics like mana, stamina, or hit points. You can see the values of your karma and fame (which can also appear as a title on your paperdoll). Your hunger and thirst will be shown so you can determine if you need to eat or drink (of course, the game will tell you when you are starving or dying of thirst without this statistic). You can tell how fast you can cast spells or apply bandages. If you murdered anybody innocent, you can see that value here. If you use tithing points (people doing knightship), you can see that value here to know if you need to make a donation of gold to a shrine sooner or later.<br><br>SKILLS: Pressing the skills button will open this (as will pressing Alt K). Here you can see the many skills available in the game for your character to become proficient at. You will have a maximum of 1,000 skill points to use. The skill with the blue dots to the left are skills that are activated to use most of the time (meaning, sometimes they are working in the background as well). For these skills, you can click and drag them off of this scroll and it will make a button on your screen that you can click on to activate the skill in the future. To the right of each value is an up arrow that you can change to a down arrow or a lock. You can lock a skill at a certain value so it does not raise or lower any more than that. You can change it to a down arrow as well. This will tell the game that this skill will decrease if another skill raises (and you have used up all 1,000 points). You can see the example on the right. The lower right number of 193. 1 indicates that this character has used that many skill points so far. Some magic items add to your skills and will be reflected here. If you just want to see the skill values (without the addition that magic items provide), click on the 'show real' option on the bottom right. The 'show caps' option will show you the maximum value you can have a skill at. Each skill is allowed to go up to 100 each (without going over 1,000 total). You can find scrolls of power that will allow a skill to go above 100, and this option will show you that. Skills are organized by category and you can even click the 'new group' button to make a group of your own. Then you can drag and drop skills in this 'group' so you can select a particular set of skills you may want to keep your eye on for that character.<br><br>STATUS: The status window shows your character's strength, dexterity, intelligence, hit points, stamina, mana, luck, carried weight, followers, damage, and carried gold. You can also see the maximum value you are allowed for strength, dexterity, and intelligence (always 250). Double clicking this window will switch it from a detailed view to a smaller bar view. You can set your strength, dexterity, and intelligence to raise and lower similar to skills described above... with the arrows on the left of each value. On the very right is your values of defense against physical, fire, cold, poison, and energy. All creatures have these values and some attacks deal damage in all or some of these categories. You will one day want all of these values high (maximum of 70% in each). The rest of the game features can be learned while in the game. As an example, you can learn some more commands from the message of day. You can also visit a sage and buy a scroll that will detail what all of the skills do and how to use them. Many commands you can type in the bottom left of the world view window by typing a '[' symbol (without quotes)... along with the command. For example, '[c' will bring up the chat window. '[status' will bring up the stats window. '[motd' will bring up the message of the day. On the message of the day window, press the ? on the upper right to learn more commands. It will be up to you and explore. Now lets get into some of the common things that you will probably do in the game.<br><br>CHAT: This is a means to communicate within the game world when you are not on the same screen as another player. Type '[c' to begin using the chat system. This also lets you send a message to another player that they can read later on. Keep in mind that this is character specific and not account specific. If you send a message to a character, but the player logs in with a different character, they will not see the message until they log in with 'that character'. This chat feature has many options Internet chat systems have. You can see who is online. You can establish channels. You can even set some privacy levels to ignore others or not be seen at all.<br><br>CITIZENS: Many citizens have a context menu when you single left click them. This brings up a list of services they provide for you. Some may be grayed out as being something they cannot provide 'you' in particular (if they train tailoring for example, and you are already a master at it, it will be grayed out because they cannot teach you any more about it). Many citizens have a 'hire' option. Make sure to explore what you can hire them to do for you. It may come in handy later. Be careful when single left clicking them as you may still be in war mode and accidentally attack them. If they live through your initial attack, run away or risk becoming a murderer. Murders take about 8 hours of real time (while in game) to go away. It will take 40 hours of real time (while in game) to go away if you continue to murder while you are a murderer. " );
		}

		public BeginnerBookGump( Mobile from, int page ) : base( 50, 50 ) 
		{
			if ( UseLocalizedTextMode( from ) )
			{
				from.PlaySound( 0x55 );

				Closable = true;
				Disposable = false;
				Dragable = true;
				Resizable = false;

				AddPage( 0 );
				AddImage( 0, 0, 9546, Server.Misc.PlayerSettings.GetGumpHue( from ) );
				AddHtml( 14, 14, 400, 20, @"<BODY><BASEFONT Color=#ddbc4b>" + ResolveText( from, "Basics" ) + "</BASEFONT></BODY>", false, false );
				AddHtml( 17, 49, 875, 726, @"<BODY><BASEFONT Color=#ddbc4b>" + GetLocalizedBody( from ) + "</BASEFONT></BODY>", false, true );
				AddButton( 867, 10, 4017, 4017, 0, GumpButtonType.Reply, 0 );
				return;
			}

			if ( page < 1 || page > 49 )
				page = 1;

			from.PlaySound( 0x55 );

			Closable=true;
			Disposable=false;
			Dragable=true;
			Resizable=false;

			AddPage(0);
			AddImage(0, 0, 7010, 1993);
			AddImage(0, 0, 7011, 2989);
			AddImage(0, 0, 7025, 2268);

			int paper = 23014+page;
				paper = paper+page;

			AddImage(68, 22, paper);
			AddImage(489, 25, paper+1);

			AddButton(118, 15, 4014, 4014, Page( page, -1 ), GumpButtonType.Reply, 0);

			AddButton(901, 18, 4005, 4005, Page( page, 1 ), GumpButtonType.Reply, 0);

			AddButton(124, 635, 4011, 4011, 2, GumpButtonType.Reply, 0); // TOC

			if ( page == 2 )
			{
				int g = 58;
				int h = 32;

				AddButton(113, g+=h, 2117, 2117, 1, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 2, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 3, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 4, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 5, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 5, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 7, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 8, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 11, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 11, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 12, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 13, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 14, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 15, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 15, GumpButtonType.Reply, 0);
				AddButton(113, g+=h, 2117, 2117, 16, GumpButtonType.Reply, 0);

				g = 38;
				AddButton(534, g+=h, 2117, 2117, 18, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 21, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 22, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 24, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 25, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 27, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 29, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 30, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 33, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 35, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 38, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 38, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 39, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 40, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 47, GumpButtonType.Reply, 0);
				AddButton(534, g+=h, 2117, 2117, 49, GumpButtonType.Reply, 0);
			}
		}

		public int Page( int page, int mod )
		{
			page = page + mod;

			if ( page < 1 )
				page = 49;
			else if ( page > 49 )
				page = 1;

			return page;
		}
       
		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile; 

			if ( info.ButtonID > 0 )
				from.SendGump( new BeginnerBookGump( from, info.ButtonID ) );

			from.PlaySound( 0x55 );
		}
	} 
}
