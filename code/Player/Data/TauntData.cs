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
	public static List<TauntData> AllTaunts { get; set; } = new();
	public static List<TauntData> EnabledTaunts { get; set; } = new();
	public static List<TauntData> StockTaunts { get; set; } = new();
	public static List<TauntData> CustomTaunts { get; set; } = new();

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
	/// Is this taunt running on the Legacy Pre-AnimGraph Tag system?
	/// </summary>
	public bool Legacy { get; set; }

	/// <summary>
	/// Is this taunt disabled? If so, it will not generate in any taunt lists
	/// </summary>
	public bool Disabled { get; set; }

	/// <summary>
	/// If disabled, this taunt will ignore being cancelled if the player leave the ground
	/// </summary>
	[Category( "Attributes" )]
	[Title( "Require Ground" )]
	public bool RequireGround { get; set; } = true;

	/// <summary>
	/// Which class can use this taunt? Used to generate the taunt list
	/// </summary>
	[Category( "Attributes" )]
	[Obsolete]
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
	/// Is this a custom taunt? This changes behavior of taunts to accomodate additional, player-installed taunts
	/// </summary>
	[Category( "Attributes" )]
	[Title( "Custom Taunt" )]
	public bool IsCustomTaunt { get; set; }

	/// <summary>
	/// If assigned, allows that class to use this taunt. The playermodel bonemerges to the selected model. Undefined does nothing. If "Custom Taunt" is enabled, uses TAM system, otherwise ignores model entry
	/// </summary>
	[Category( "Models" )]
	[Title( "Animation Models" )]
	public List<TauntModelEntry> AnimationModelEntries { get; set; }

	/// <summary>
	/// If assigned, this taunt will spawn the specified prop for it's duration. Use Undefined for a shared prop between all classes (prioritizes class-specific entries).
	/// </summary>
	[Category( "Models" )]
	[Title( "Prop Models" )]
	public List<TauntModelEntry> PropModelEntries { get; set; }

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

		if ( PropModelEntries != null )
		{
			foreach ( var propModel in PropModelEntries )
			{
				Precache.Add( propModel.modelPath );
			}
		}

		// Get lowercase class name
		//string tauntname = ResourceName.ToLower();

		//Log.Info( ResourceName + " " + Disabled );

		if ( !Disabled )
		{
			if ( IsCustomTaunt )
			{
				CustomTaunts.Add( this );
			}
			else
			{
				StockTaunts.Add( this );
			}
			EnabledTaunts.Add( this );
		}
		AllTaunts.Add( this );
	}

	/// <summary>
	/// Returns a TauntData via a string
	/// </summary>
	/// <param name="taunt_name"></param>
	/// <returns></returns>
	public static TauntData Get( string taunt_name )
	{
		if ( String.IsNullOrEmpty( taunt_name ) )
		{
			Log.Warning( "GET TAUNTDATA FAILED: STRING NULL OR EMPTY" );
			return null;
		}

		taunt_name = taunt_name.ToLower();
		
		foreach (var taunt_data in EnabledTaunts)
		{
			if (taunt_data.ResourceName == taunt_name)
			{
				return taunt_data;
			}
		}

		//We have a string, but it didn't match any existing taunts
		Log.Warning( "GET TAUNTDATA FAILED: STRING DOES NOT MATCH ANY EXISTING FILES" );
		return null;
	}
	public string GetAnimationModel( TFPlayerClass playerClass )
	{
		if ( AnimationModelEntries == null ) return "";
		foreach ( var entry in this.AnimationModelEntries )
		{
			if ( entry.playerClass == playerClass ) return entry.modelPath;
		}
		return "";
	}
	public string GetPropModel( TFPlayerClass playerClass )
	{
		if ( PropModelEntries == null ) return "";
		foreach ( var entry in this.PropModelEntries )
		{
			if ( entry.playerClass == playerClass ) return entry.modelPath;
		}
		return "";
	}
	public struct TauntModelEntry
	{
		public TFPlayerClass playerClass { get; set; }
		[ResourceType( "vmdl" )] public string modelPath { get; set; }
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
