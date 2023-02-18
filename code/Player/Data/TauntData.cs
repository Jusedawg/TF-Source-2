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

	/// <summary>
	/// Sequence name of taunt, used to get duration of Once taunts
	/// </summary>
	public string SequenceName { get; set; }

	/// <summary>
	/// Is this taunt disabled? If so, it will not generate in any taunt lists
	/// </summary>
	public bool Disabled { get; set; }

	/// <summary>
	/// Which class can use this taunt? Used to generate the taunt list
	/// </summary>
	[Category( "Attributes" )]
	public TFPlayerClass Class { get; set; } = TFPlayerClass.Undefined;

	[Category( "Attributes" )]
	public TauntType TauntType { get; set; }

	/// <summary>
	/// Allows players to join into the taunt by double-tapping the taunt button while looking at a player performing the taunt
	/// </summary>
	[Category( "Attributes" )]
	[Title( "Group Taunt" )]
	public bool TauntAllowJoin { get; set; }

	/// <summary>
	/// If assigned, taunt will spawn this prop
	/// </summary>
	[Category( "Attributes" )]
	[ResourceType( "vmdl" )]
	[Title( "Prop Model" )]
	public string TauntPropModel { get; set; }

	/// <summary>
	/// If the taunt enables movement, this limits how fast the player can move
	/// </summary>
	[Category( "Movement" )]
	[Title( "Maximum Movmement Speed" )]
	public float TauntMovespeed { get; set; }

	/// <summary>
	/// Forces the player to move forward
	/// </summary>
	[Category( "Movement" )]
	[Title( "Force Movement" )]
	public bool TauntForceMove { get; set; }

	/// <summary>
	/// Music that will play during the taunt
	/// </summary>
	[Category( "Audio" )]
	[Title( "Music" )]
	[ResourceType( "sound" )]
	public string TauntMusic { get; set; }



	protected override void PostLoad()
	{
		base.PostLoad();

		if ( !string.IsNullOrEmpty( TauntPropModel ) )
		{
			Precache.Add( TauntPropModel );
		}
		
		// Get lowercase class name
		//string tauntname = ResourceName.ToLower();

		Log.Info( ResourceName + " " + Disabled );

		if ( !Disabled )
		{
			AllActive.Add( this );
		}

		All.Add( this );
	}


	public static TauntData Get( string taunt_name )
	{
		TauntData taunt = null;

		if ( String.IsNullOrEmpty( taunt_name) )
		{
			Log.Warning("GET TAUNTDATA FAILED: STRING NULL OR EMPTY");
			return null;
		}

		taunt_name = taunt_name.ToLower();
		
		foreach (var taunt_data in AllActive)
		{
			if (taunt_data.ResourceName == taunt_name)
			{
				taunt = taunt_data;
				break;
			}
		}

		return taunt;
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
