using Sandbox;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Amper.FPS;

/// <summary>
/// Asset used to override game specific foosteps, needs to be one per game.
/// </summary>
[GameResource( "Footstep Override", "footstep", "", Icon = "🦶", IconBgColor = "#7e8ff4", IconFgColor = "#0e0e0e" )]
public class FootstepData : GameResource
{
	public static List<FootstepData> All { get; set; } = new();

	protected override void PostLoad()
	{
		base.PostLoad();
		All.Add( this );
	}

	[ResourceType( "surface" )]
	public string SurfacePath { get; set; }
	public FootstepSounds Sounds { get; set; }

	public static bool GetSoundsForSurface( Surface surface, out FootstepSounds sounds )
	{
		sounds = default;

		if ( surface == null )
			return false;

		var result = All.FirstOrDefault( x => x.SurfacePath == surface.ResourcePath );
		if ( result == null )
		{
			sounds = FootstepSounds.FromSurface( surface );
			return true;
		}

		sounds = result.Sounds;
		return true;
	}

	public struct FootstepSounds
	{
		[FGDType( "sound" )]
		public string FootLeft { get; set; }
		[FGDType( "sound" )]
		public string FootRight { get; set; }
		[FGDType( "sound" )]
		public string FootLaunch { get; set; }
		[FGDType( "sound" )]
		public string FootLand { get; set; }

		public static FootstepSounds FromSurface( Surface surface )
		{
			return new FootstepSounds
			{
				FootLand = surface.Sounds.FootLand,
				FootLaunch = surface.Sounds.FootLaunch,
				FootLeft = surface.Sounds.FootLeft,
				FootRight = surface.Sounds.FootRight,
			};
		}
	}
}
