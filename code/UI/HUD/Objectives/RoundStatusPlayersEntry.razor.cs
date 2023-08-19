using Sandbox.UI;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace TFS2.UI;

public partial class RoundStatusPlayersEntry : Panel
{
	public IClient Client { get; set; }
	Panel Portrait;
	Label RespawnTimer;

	public override void Tick()
	{
		if ( Portrait == null ) return;

		var local = Game.LocalClient;
		if ( local == null )
			return;

		TFPlayer ply = null;

		if(Client.Pawn.IsValid())
		{
			ply = (TFPlayer)Client.Pawn;
		}

		var ourTeam = local.GetTeam();
		var theirTeam = Client.GetTeam();
		var anonymous = true;

		if ( ourTeam == theirTeam && ply.IsValid() )
		{
			var theirClass = Client.GetPlayerClass();
			if ( theirClass != null )
			{
				anonymous = false;
				Portrait.Style.Set( "background-image", $"url(/ui/hud/classportraits/{theirClass.Title}_{theirTeam.GetName()}.png)" );
			}
		}

		if ( anonymous )
			Portrait.Style.Set( "background-image", $"url(/ui/hud/classportraits/silhouette.png)" );
		else
		{
			var rules = TFGameRules.Current;
			if(!ply.IsAlive && rules.CanTeamRespawn(ply.Team) && rules.CanPlayerRespawn(ply) && !TFGameRules.Current.IsWaitingForPlayers)
			{
				var respawnTime = TFGameRules.Current.GetNextPlayerRespawnWaveTime( ply ) - Time.Now;
				RespawnTimer.Text = respawnTime.CeilToInt().ToString();
				RespawnTimer.SetClass( "visible", true );
			}
			else
			{
				RespawnTimer.SetClass( "visible", false );
			}
		}

		Portrait.SetClass( "dead", !Client.IsAlive() );
		Portrait.SetClass( "us", local == Client );
	}
}
