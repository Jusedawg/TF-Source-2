using Sandbox;
using System.Linq;
using TFS2.UI;
using Amper.FPS;

namespace TFS2;

partial class TFGameRules
{
	public static void PlaySoundToTeam( TFTeam team, string sound, SoundBroadcastChannel channel, bool stopprevious = true )
	{
		PlaySoundToTeam( (int)team, sound, channel, stopprevious );
	}

	//
	// HUD Alerts
	//

	public static void SendHUDAlertToTeam( TFTeam team, string message, string icon, float time = 5, TFTeam colorteam = TFTeam.Unassigned )
	{
		var clients = All.OfType<TFPlayer>()
			.Where( x => x.Team == team )
			.Select( x => x.Client )
			.Where( x => x != null );

		Alert.Show( To.Multiple( clients ), message, icon, time, colorteam );
	}

	public static void SendHUDAlertToAll( string message, string icon, float time = 5, TFTeam team = TFTeam.Unassigned )
	{
		Alert.Show( To.Everyone, message, icon, time, team );
	}

	public static void SendHUDAlertToPlayer( TFPlayer player, string message, string icon, float time = 5, TFTeam team = TFTeam.Unassigned )
	{
		Alert.Show( To.Single( player.Client ), message, icon, time, team );
	}
}
