using Sandbox;
using System.Collections.Generic;

namespace TFS2;

/// <summary>
/// This represents taunts.
/// These are defined by creating .taunt assets in the gamemode working directory.
/// </summary>
[Library( "taunt" )]
public class TauntData : GameResource
{
	/// <summary>
	/// All registered and enabled Player Taunts are here.
	/// </summary>
	// public static List<TauntData> All = new();
	public static Dictionary<string, TauntData> All = new();

	public string DisplayName { get; set; }
	public string Icon { get; set; }
	public string AnimName { get; set; }
	public bool Disabled { get; set; }
	public string ClassUseChoice { get; set; } = "-1"; // Defaults to Undefined, doesn't set if asset itself is undefined for some reason
	public TFPlayerClass ClassUse => (TFPlayerClass)ClassUseChoice.ToInt();

	public TauntAttributes Attributes { get; set; }

	public class TauntAttributes
	{
		public string TauntTypeChoice { get; set; }
		public TauntType TauntType => (TauntType)TauntTypeChoice.ToInt();
		public bool TauntForceMove { get; set; } = false;
		public bool TauntAllowJoin { get; set; } = false;
		public bool TauntUseProp { get; set; }
		public string TauntPropModel { get; set; }
	}

	protected override void PostLoad()
	{
		base.PostLoad();

		// Caches prop model if it uses one
		if ( Attributes.TauntUseProp == true )
		{
			Precache.Add( Attributes.TauntPropModel );
			//TFGame.Log.Info( "PrecacheTauntModel " + Attributes.TauntPropModel );
		}

		// Get lowercase class name
		string tauntname = ResourceName.ToLower();

		if ( Disabled == false )
		{
			All[tauntname] = this;
			// TFGame.Log.Info( "Taunt Loaded: " + tauntname );
		}
	}

	public static bool IsValid( string name )
	{
		name = name.ToLower();

		return All.ContainsKey( name );
	}
	public static TauntData Get( string name )
	{
		name = name.ToLower();

		if ( !IsValid( name ) ) return null;
		return All[name];
	}
}

public enum TauntType
{
	/// <summary>
	/// This taunt plays once
	/// </summary>
	Once,
	/// <summary>
	/// This taunt loops
	/// </summary>
	Looping,
	/// <summary>
	/// This taunt requires a partner to fully complete
	/// </summary>
	Partner
}
