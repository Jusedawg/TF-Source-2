using Sandbox;
using Amper.FPS;
using System.Linq;
using Breaker;

namespace TFS2;

partial class TFPlayer
{
	[Command("giveammo"), Permission("tfs2.ammo")]
	public static void Command_GiveAmmo( int count )
	{
		if ( ConsoleSystem.Caller.Pawn is TFPlayer player )
		{
			if ( player.ActiveWeapon is TFWeaponBase weapon )
				weapon.Reserve += count;
		}
	}

	[Command( "tf_regenerate" ), Permission( "tfs2.ammo" )]
	public static void Command_Regenerate()
	{
		(ConsoleSystem.Caller.Pawn as TFPlayer)?.Regenerate();
	}

	[Command( "tf_deleteweapons" ), Permission( "tfs2.ammo" )]
	public static void Command_DeleteWeapons()
	{
		(ConsoleSystem.Caller.Pawn as TFPlayer)?.DeleteAllWeapons();
	}

	[Command( "hurtme" ), Permission( "tfs2.health" )]
	public static void Command_HurtMe( int damage = 10 )
	{
		if ( ConsoleSystem.Caller.Pawn is TFPlayer player )
		{
			var dmgInfo = ExtendedDamageInfo.Create( damage )
				.WithAttacker( player )
				.WithInflictor( player )
				.WithAllPositions( player.Position )
				.WithTag( TFDamageTags.PreventPhysicsForce );

			player.TakeDamage( dmgInfo );
		}
	}

	[Command( "burn" ), Permission( "tfs2.health" )]
	public static void Command_Burn( int time = 5 )
	{
		if ( ConsoleSystem.Caller.Pawn is TFPlayer player )
			player.Burn( player, null, time );
	}

	[Command( "bot_voicecommand" ), Permission( "tfs2.bots" )]
	public static void Command_BotCommand( string botname, int menu = -1, int concept = -1 )
	{
		var bot = Game.Clients.FirstOrDefault( x => x.IsBot && x.Name == botname );
		if ( bot == null )
		{
			Log.Error( $"Bot named \"{botname}\" was not found" );
			return;
		}

		var botPlayer = bot.Pawn as TFPlayer;
		if ( !botPlayer.IsValid() )
			return;

		if ( menu < 0 ) menu = Game.Random.Int( 0, 2 );
		if ( concept < 0 ) concept = Game.Random.Int( 0, 8 );

		botPlayer.PlayVoiceCommand( menu, concept );
	}
}
