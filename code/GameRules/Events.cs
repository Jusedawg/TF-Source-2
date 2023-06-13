using Amper.FPS;
using Sandbox;
using System.Linq;
using TFS2.UI;

namespace TFS2;

partial class TFGameRules
{
	public override void PlayerHurt( SDKPlayer player, ExtendedDamageInfo info )
	{
		base.PlayerHurt( player, info );

		EventDispatcher.InvokeEvent( new PlayerHurtEvent()
		{
			Victim = player,
			Attacker = info.Attacker,
			Assister = null,
			Inflictor = info.Weapon,
			Tags = info.Tags.ToArray(),
			Position = info.ReportPosition,
			Damage = info.Damage
		} );
	}

	public override void PlayerRespawn( SDKPlayer player )
	{
		base.PlayerRespawn( player );

		if ( player is not TFPlayer tfPlayer )
			return;

		EventDispatcher.InvokeEvent( new PlayerSpawnEvent()
		{
			Client = player,
			Team = tfPlayer.Team,
			Class = tfPlayer.PlayerClass
		} );
	}

	public override void PlayerDeath( SDKPlayer player, ExtendedDamageInfo info )
	{
		//
		// Dispatch event about this.
		//

		EventDispatcher.InvokeEvent( new PlayerDeathEvent()
		{
			Victim = player,
			Attacker = info.Attacker,
			Assister = null,
			Weapon = info.Weapon,
			Inflictor = info.Inflictor,
			Tags = info.Tags?.ToArray(),
			Position = info.ReportPosition,
			Damage = info.Damage,
		} );

		if ( ShouldAnnounceFirstBlood() && !FirstBloodAnnounced )
		{
			FirstBloodAnnounced = true;
			PlaySoundToAll( "announcer.first_blood", SoundBroadcastChannel.Announcer );
		}
	}

	public override void PlayerChangeTeam( SDKPlayer player, int team )
	{
		Game.AssertServer();

		base.PlayerChangeTeam( player, team );
		TFChatBox.AddInformation( To.Everyone, $"{player.Client.Name} joined team {((TFTeam)team).GetTitle()}" );

		EventDispatcher.InvokeEvent( new PlayerChangeTeamEvent
		{
			Client = player.Client,
			Team = (TFTeam)team
		} );
	}
}
