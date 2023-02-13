using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TFS2;

/// <summary>
/// This represents the Team Fortress player class. TFPlayer.PlayerClass is set to this when player chooses the class they want to play.
/// These are defined by creating .taunt assets in the gamemode working directory.
/// </summary>
[GameResource( "TF:S2 Taunt Data", "taunt", "Team Fortress: Source 2 taunt definition", Icon = "accessibility_new", IconBgColor = "#ffc84f", IconFgColor = "#0e0e0e" )]
public class TauntData : GameResource
{
	/// <summary>
	/// All registered and enabled Player Taunts are here.
	/// </summary>
	/// 
	// public static List<TauntData> All = new(); Old Method
	public static List<TauntData> All { get; set; } = new();
	public static List<TauntData> AllActive { get; set; } = new();

	/// <summary>
	/// Display Name 
	/// </summary>
	public string DisplayName { get; set; }

	/// <summary>
	/// Icon for taunt in taunt menu
	/// </summary>
	//[Category( "Images" )]
	[ResourceType( "png" )]
	public string Icon { get; set; }
	public string StringName { get; set; }
	public bool Disabled { get; set; }

	/*
	public string ClassUseChoice { get; set; } = "-1"; // Defaults to Undefined, doesn't set if asset itself is undefined for some reason
	public TFPlayerClass ClassUse => (TFPlayerClass)ClassUseChoice.ToInt();
	*/
	/// <summary>
	/// Which class can use this taunt? Used to generate the taunt list dynamically
	/// </summary>
	[Category( "Attributes" )]
	public TFPlayerClass Class { get; set; } = TFPlayerClass.Undefined;


	//[Category("Attributes")]
	//public string TauntTypeChoice { get; set; }
	//public TauntType TauntType => (TauntType)TauntTypeChoice.ToInt();
	[Category( "Attributes" )]
	public TauntType TauntType { get; set; }

	[Category( "Attributes" )]
	public bool TauntForceMove { get; set; } = false;

	[Category( "Attributes" )]
	public bool TauntAllowJoin { get; set; } = false;

	[Category( "Attributes" )]
	public bool TauntUseProp { get; set; }

	[Category( "Attributes" )]
	public string TauntPropModel { get; set; }
	

	protected override void PostLoad()
	{
		base.PostLoad();

		if (TauntUseProp == true )
		{
			Precache.Add( TauntPropModel );
		}

		// Get lowercase class name
		//string tauntname = ResourceName.ToLower();

		Log.Info( StringName + " " + Disabled );

		if ( !Disabled )
		{
			AllActive.Add( this );
		}

		All.Add( this );
	}


	public static TauntData Get( string taunt_name )
	{
		if ( String.IsNullOrEmpty( taunt_name) )
		{
			Log.Info("GET TAUNTDATA FAILED: STRING NULL OR EMPTY");
			return null;
		}

		taunt_name = taunt_name.ToLower();
		TauntData tauntCurr = null;
		foreach (var taunt_data in AllActive)
		{
			if (taunt_data.StringName == taunt_name)
			{
				tauntCurr = taunt_data;
				break;
			}
		}

		return tauntCurr;
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
