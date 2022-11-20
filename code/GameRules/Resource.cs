using Sandbox;
using Amper.FPS;

namespace TFS2;

partial class TFGameRules
{
	public override void UpdateClientData( Client client, SDKPlayer player )
	{
		base.UpdateClientData( client, player );

		if ( player is TFPlayer pawn )
			UpdateClientData( client, pawn );
	}

	public void UpdateClientData( Client client, TFPlayer player )
	{
		var playerClass = "";
		if ( player.PlayerClass != null )
			playerClass = player.PlayerClass.ResourcePath;

		client.SetValue( "s_playerclass", playerClass );

		#region Stats
		client.SetInt( "i_points", player.GetPoints() );
		client.SetInt( "i_kills", player.Kills );
		client.SetInt( "i_deaths", player.Deaths );
		client.SetInt( "i_assists", player.Assists );
		client.SetInt( "i_destructions", player.Destructions );
		client.SetInt( "i_captures", player.Captures );
		client.SetInt( "i_defenses", player.Defenses );
		client.SetInt( "i_dominations", player.Dominations );
		client.SetInt( "i_revenges", player.Revenges );
		client.SetInt( "i_invulns", player.Invulns );
		client.SetInt( "i_headshots", player.Headshots );
		client.SetInt( "i_teleports", player.Teleports );
		client.SetInt( "i_healing", player.Healing );
		client.SetInt( "i_backstabs", player.Backstabs );
		client.SetInt( "i_bonus", player.Bonus );
		client.SetInt( "i_support", player.Support );
		client.SetInt( "i_damage", player.DamageScore );
		#endregion
	}
}

public static class ClientExtensions
{
	public static float GetHealth( this Client client ) => client.GetValue<float>( "f_health" );
	public static bool IsAlive( this Client client ) => client.GetValue<bool>( "b_alive" );
	public static TFTeam GetTeam( this Client client ) => (TFTeam)client.GetValue<int>( "n_teamnumber" );
	public static PlayerClass GetPlayerClass( this Client client ) => ResourceLibrary.Get<PlayerClass>( client.GetValue<string>( "s_playerclass" ) );

	#region Stats
	public static int GetPoints( this Client client ) => client.GetInt( "i_points" );
	public static int GetKills( this Client client ) => client.GetInt( "i_kills" );
	public static int GetDeaths( this Client client ) => client.GetInt( "i_deaths" );
	public static int GetAssists( this Client client ) => client.GetInt( "i_assists" );
	public static int GetDestructions( this Client client ) => client.GetInt( "i_destructions" );
	public static int GetCaptures( this Client client ) => client.GetInt( "i_captures" );
	public static int GetDefenses( this Client client ) => client.GetInt( "i_defenses" );
	public static int GetDominations( this Client client ) => client.GetInt( "i_dominations" );
	public static int GetRevenges( this Client client ) => client.GetInt( "i_revenges" );
	public static int GetInvulns( this Client client ) => client.GetInt( "i_invulns" );
	public static int GetHeadshots( this Client client ) => client.GetInt( "i_headshots" );
	public static int GetTeleports( this Client client ) => client.GetInt( "i_teleports" );
	public static int GetHealing( this Client client ) => client.GetInt( "i_healing" );
	public static int GetBackstabs( this Client client ) => client.GetInt( "i_backstabs" );
	public static int GetBonus( this Client client ) => client.GetInt( "i_bonus" );
	public static int GetSupport( this Client client ) => client.GetInt( "i_support" );
	public static int GetDamage( this Client client ) => client.GetInt( "i_damage" );
	#endregion
}
