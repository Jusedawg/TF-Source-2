using Sandbox;
using Editor;
using Amper.FPS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TFS2;

[Library( "tf_logic_tdm" )]
[Title( "Team Deathmatch" )]
[Category( "Gamemode" )]
[Icon( "groups" )]
[EditorSprite( "materials/editor/tf_logic_tdm.vmat" )]
[HammerEntity]

public partial class TeamDeathmatch : GamemodeEntity
{
	/// <summary>
	/// How many frags every team has collected.
	/// </summary>
	[Net] public IDictionary<TFTeam, int> Frags { get; set; }

	/// <summary>
	/// This is the amount of time since either of the teams reached beep time.
	/// </summary>
	[Net] public TimeSince TimeSinceReachFragLimit { get; set; }

	/// <summary>
	/// This property will store the first team to reach frag limit.
	/// </summary>
	[Net] public TFTeam FirstScorer { get; set; }
	[Net] public int FragLimit { get; set; }

	public TeamDeathmatch()
	{
		EventDispatcher.Subscribe<PlayerDeathEvent>( PlayerKilled, this );
	}

	public override bool HasWon( out TFTeam team, out TFWinReason reason )
	{
		team = FirstScorer;
		reason = TFWinReason.FragLimit;

		if ( HasReachedFragLimit() && GetTimeUntilRoundEnd() == 0 )
		{
			return true;
		}

		return false;
	}

	public override void Reset()
	{
		// reset frags
		Frags.Clear();
		FirstScorer = TFTeam.Unassigned;

		// calculate the frag limit, based on the player count.
		float count = MathF.Ceiling( All.OfType<TFPlayer>().Count() / 2 ) * 2;
		var lerp = count.Remap( 4, 24 ).Clamp( 0, 1 );

		float min = tf_tdm_frag_limit_min;
		float max = tf_tdm_frag_limit_max;

		var limit = min.LerpTo( max, lerp ).FloorToInt();
		limit = Math.Max( limit, 1 );
		FragLimit = limit;
	}
	public virtual int GetTeamFragCount( TFTeam team )
	{
		Frags.TryGetValue( team, out var count );
		return count;
	}

	public virtual float GetTimeUntilRoundEnd()
	{
		if ( !HasReachedFragLimit() )
			return tf_tdm_finale_beep_time;

		float time = tf_tdm_finale_beep_time - TimeSinceReachFragLimit;
		time = MathF.Max( 0, time );

		return time;
	}

	/// <summary>
	/// Are we currently in beep time?
	/// </summary>
	/// <returns></returns>
	public virtual bool HasReachedFragLimit()
	{
		if ( !TFGameRules.Current.AreObjectivesActive() )
			return false;

		foreach ( TFTeam team in Enum.GetValues( typeof( TFTeam ) ) )
		{
			if ( !team.IsPlayable() ) continue;
			if ( HasTeamReachedFragLimit( team ) ) return true;
		}

		return false;
	}

	public virtual bool HasTeamReachedFragLimit( TFTeam team )
	{
		return GetTeamFragCount( team ) >= FragLimit;
	}

	public virtual void SetTeamFragCount( TFTeam team, int count )
	{
		count = Math.Min( count, FragLimit );
		Frags[team] = count;
	}

	/// <summary>
	/// Add points to the team's frag score.
	/// </summary>
	public virtual void AddTeamFragCount( TFTeam team, int count )
	{
		// Remember 
		bool wasNotInBeepTime = !HasReachedFragLimit();
		SetTeamFragCount( team, GetTeamFragCount( team ) + count );

		if ( wasNotInBeepTime && HasReachedFragLimit() )
		{
			FirstScorer = team;
			OnReachedFragLimit();
		}
	}

	public virtual void OnReachedFragLimit()
	{
		TimeSinceReachFragLimit = 0;
	}

	/// <summary>
	/// This is fired when a player dies.
	/// </summary>
	/// <param name="args"></param>
	public virtual void PlayerKilled( PlayerDeathEvent args )
	{
		if ( !Game.IsServer )
			return;

		if ( !TFGameRules.Current.AreObjectivesActive() )
			return;

		var attacker = args.Attacker;
		var victim = args.Victim;

		if ( attacker == null )
			return;

		// Check if attacker and victim are not the same
		// we don't count suicides as frags.
		if ( attacker == victim )
			return;

		// Check if both attacker and victim are on different teams.
		if ( attacker.GetTeam() == victim.GetTeam() )
			return;

		// Get the attacker's team.
		var team = attacker.GetTeam();

		// And give them one score.
		AddTeamFragCount( team, 1 );
	}

	[ConVar.Replicated] public static int tf_tdm_frag_limit_min { get; set; } = 15;
	[ConVar.Replicated] public static int tf_tdm_frag_limit_max { get; set; } = 75;
	[ConVar.Replicated] public static int tf_tdm_finale_beep_time { get; set; } = 5;
}
