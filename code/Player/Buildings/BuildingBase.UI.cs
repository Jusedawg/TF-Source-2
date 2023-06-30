using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox;
using TFS2.UI;

namespace TFS2;

public partial class TFBuilding : ITargetID, ITargetIDSubtext, IKillfeedName
{
	protected virtual void InitializeUI( BuildingData data )
	{
		UpgradeMetalLine = new( AppliedMetal, 0, data.UpgradeCost, "UI/Hud/Buildings/ico_metal_mask.png" );
		IsUIInitialized = true;
	}
	protected bool IsUIInitialized;
	protected BuildingInfoLine UpgradeMetalLine;
	[GameEvent.Tick.Client]
	public virtual void TickUI()
	{
		if ( !IsInitialized ) return;
		if ( !IsUIInitialized )
		{
			InitializeUI( Data );
		}

		UpgradeMetalLine.Value = AppliedMetal;
		UpgradeMetalLine.Visible = Level != MaxLevel;
	}

	public virtual IEnumerable<BuildingInfoLine> GetUILines()
	{
		yield return UpgradeMetalLine;
	}

	string IKillfeedName.Name => $"{Data.Title} ({Owner.Client?.Name})";
	string ITargetID.Name => $"{Data.Title} built by {Owner.Client?.Name}";
	string ITargetID.Avatar => "";
	string ITargetIDSubtext.Subtext
	{
		get
		{
			if ( Level == MaxLevel )
			{
				return MaxLevelSubtext;
			}
			else
			{
				return NormalSubtext;
			}
		}
	}
	protected virtual string MaxLevelSubtext => $"(Level {Level})";
	protected virtual string NormalSubtext => $"(Level {Level}) Upgrade Progress: {AppliedMetal}/{Data.UpgradeCost}";
}

public class BuildingInfoLine
{
	public float MinValue { get; set; }
	public float Value { get; set; }
	public float MaxValue { get; set; }
	public string Text { get; set; }
	public string IconPath { get; set; }
	public bool ShowText { get; set; }
	public bool Visible { get; set; } = true;
	public BuildingInfoLine( float value, float minValue, float maxValue, string text, string iconPath )
	{
		Value = value;
		MinValue = minValue;
		MaxValue = maxValue;
		Text = text;
		IconPath = iconPath;
		ShowText = !string.IsNullOrEmpty( text );
	}

	public BuildingInfoLine( string text, string iconPath ) : this( 0, 0, 0, text, iconPath )
	{
	}

	public BuildingInfoLine( float value, float minValue, float maxValue, string iconPath ) : this( value, minValue, maxValue, null, iconPath )
	{

	}
}
