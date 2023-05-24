using Sandbox;
using System.Linq;
using System.Collections.Generic;
using System;
using Amper.FPS;

namespace TFS2;

partial class TFGameRules
{
	/// <summary>
	/// Are points allowed to be captured right now?
	/// </summary>
	public bool PointsMayBeCaptured() => AreObjectivesActive();

	/// <summary>
	/// Can this team capture this point right now?
	/// </summary>
	public bool TeamMayCapturePoint( TFTeam team, ControlPoint point )
	{
		var prevPoints = point.GetPreviousPointsForTeam( team );

		// can't cap if it's locked
		if ( point.Locked )
			return false;

		// we can't set ourselves as previous point, assume null.
		foreach (var prevPoint in prevPoints)
		{
			if ( prevPoint == point )
				continue;

			if ( prevPoint != null && prevPoint.OwnerTeam != team )
				return false;
		}

		return true;
	}

	/// <summary>
	/// Can this player capture the point right now?
	/// </summary>
	public bool PlayerMayCapturePoint( TFPlayer player, ControlPoint point )
	{
		// TODO: if invisible

		// if invulnerable
		if ( player.InCondition( TFCondition.Invulnerable ) ) 
			return false;

		// TODO: if disguised as enemy team

		return true;
	}

	/// <summary>
	/// May this player block this point right now?
	/// </summary>
	public bool PlayerMayBlockPoint( TFPlayer team, ControlPoint point ) => AreObjectivesActive();

	public virtual int GetCaptureValueForPlayer( TFPlayer player )
	{
		var val = 1;
		if ( player.PlayerClass != null )
		{
			val = player.PlayerClass.Abilities.CaptureValue;
		}

		return val;
	}

	public void ControlPointCaptured( ControlPoint point, TFTeam oldteam, TFTeam newteam, IClient[] cappers )
	{
		PlaySoundToAll( "ui.scored", SoundBroadcastChannel.Generic );

		// print to console about this.
		Log.Info( $"\"{point.PrintName}\" was captured by {cappers.Count()} player(s) from {newteam} team." );

		EventDispatcher.InvokeEvent( new ControlPointCapturedEvent
		{
			Point = point,
			NewTeam = newteam,
			Cappers = cappers
		} );

		foreach ( var ply in cappers.Select( c => c.Pawn as TFPlayer ) )
			ply.Captures += 2;
	}

	/// <summary>
	/// Returns the first point that belongs to this team by default.
	/// </summary>
	/// <param name="team"></param>
	/// <returns></returns>
	public ControlPoint GetFirstDefaultOwnedControlPoint( TFTeam team )
	{
		if ( !team.IsPlayable() )
			return null;

		var teamPoints = ControlPoint.All.Where( x => x.GetDefaultTeamOwner() == team );

		// this team doesn't own any points, return null.
		if ( teamPoints.Count() == 0 )
			return null;

		// find the points with no previous points set.
		switch ( team )
		{
			case TFTeam.Red:
				teamPoints = teamPoints.Where( x => !x.PreviousRedPoints.Any() ); break;

			case TFTeam.Blue:
				teamPoints = teamPoints.Where( x => !x.PreviousBluePoints.Any() ); break;
		}

		// If we have multiple "first" points, that means the cp layout is asymetrical. Return null.
		if ( teamPoints.Count() > 1 )
			return null;

		// now we should have just a single control point.
		// return the first one.
		return teamPoints.FirstOrDefault();
	}

	/// <summary>
	/// Returns all first owned points by the team that doesn't have any previous points set.
	/// </summary>
	/// <param name="team"></param>
	/// <returns></returns>
	public IEnumerable<ControlPoint> GetFirstOwnedControlPoints( TFTeam team )
	{
		if ( !team.IsPlayable() )
			return null;

		var teamPoints = ControlPoint.All.Where( x => x.OwnerTeam == team );

		// this team doesn't own any points, return null.
		if ( teamPoints.Count() == 0 )
			return null;

		// find the points with no previous points set.
		switch ( team )
		{
			case TFTeam.Red:
				teamPoints = teamPoints.Where( x => !x.PreviousRedPoints.Any() ); break;

			case TFTeam.Blue:
				teamPoints = teamPoints.Where( x => !x.PreviousBluePoints.Any() ); break;
		}

		// now we should have just a single control point.
		// return the first one.
		return teamPoints;
	}

	/// <summary>
	/// Returns the first owned point by the team that doesn't have any previous points set.
	/// </summary>
	/// <param name="team"></param>
	/// <returns></returns>
	public ControlPoint GetFirstOwnedControlPoint( TFTeam team ) => GetFirstOwnedControlPoints( team )?.FirstOrDefault();

	public List<ControlPoint> GetFarthestOwnedControlPoints( TFTeam team )
	{
		// get the first point that we own.
		var point = GetFirstOwnedControlPoint( team );

		// If team doesnt own a single point, that means that they cant possibly have a "farthest" control point.
		if ( point == null )
			return null;

		return GetLastPointsFor(point, team);
	}

	private List<ControlPoint> GetLastPointsFor(ControlPoint point, TFTeam team, Func<ControlPoint, bool> condition = default)
	{
		var nextPoints = point.GetNextPointsForTeam( team );
		if( point.OwnerTeam != team) return null;
		if ( condition != default && !condition.Invoke( point ) ) return null;

		if ( nextPoints == default || !nextPoints.Any() )
		{
			return new List<ControlPoint> { point };
		}

		List<ControlPoint> lastPoints = new();
		foreach (var nextPoint in nextPoints)
		{
			var nextLastPoints = GetLastPointsFor( nextPoint, team, condition );
			if( nextLastPoints != null)
				lastPoints.AddRange( nextLastPoints );
		}
		if ( !lastPoints.Any() )
		{
			return new List<ControlPoint> { point };
		}

		return lastPoints;
	}

	/// <summary>
	/// Returns the farthest owned control point with associated respawn rooms.
	/// </summary>
	/// <param name="team"></param>
	/// <returns></returns>
	public List<ControlPoint> GetFarthestOwnedControlPointsWithRespawnRoom( TFTeam team )
	{
		var teamSpawnControlPoints = All.OfType<RespawnRoom>().Where( x => x.TeamOption.Is( team ) ).Select(room => room.ControlPoint);
		// get the first point that we own.
		var point = GetFirstOwnedControlPoint( team );
		
		// If team doesnt own a single point, that means that they cant possibly have a "farthest" control point.
		if ( point == null || teamSpawnControlPoints == null || !teamSpawnControlPoints.Any() )
			return null;

		var farthest = GetLastPointsFor( point, team, ( cp ) => teamSpawnControlPoints.Contains( cp ) );

		if ( farthest == null || !farthest.Any() ) return null;

		return farthest.ToList();
	}

	public List<ControlPoint> GetControlPointRouteForTeam( TFTeam team )
	{
		var point = GetFirstDefaultOwnedControlPoint( team );
		if ( point == null )
			return null;

		return GetAllNextPointsFor( point, team );
	}

	private List<ControlPoint> GetAllNextPointsFor(ControlPoint point, TFTeam team)
	{
		var nextPoints = point.GetNextPointsForTeam( team );

		List<ControlPoint> lastPoints = new()
		{
			point
		};
		if ( !nextPoints.Any() )
			return lastPoints;

		foreach ( var nextPoint in nextPoints )
		{
			lastPoints.AddRange( GetAllNextPointsFor( nextPoint, team ) );
		}

		return lastPoints;
	}

	[ConCmd.Server( "sv_spewcontrolpoints" )]
	public static void Command_SpewControlPoints()
	{
		foreach ( TFTeam team in Enum.GetValues( typeof( TFTeam ) ) )
		{
			if ( !team.IsPlayable() )
				continue;

			Log.Info( $"{team}" );

			var first = Current.GetFirstOwnedControlPoint( team );
			var farthest = Current.GetFarthestOwnedControlPoints( team );
			Log.Info( $" - First Owned: {first}" );
			Log.Info( $" - Farthest Owned: {string.Join(' ', farthest)}" );
		}
	}
}
