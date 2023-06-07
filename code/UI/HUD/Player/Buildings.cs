using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TFS2.UI;

[StyleSheet]
public partial class Buildings : Panel
{
	Dictionary<string, BuildingInstancePanel> BuildingPanels = new();
	List<TFBuilding> CurrentBuildings = new();
	public override void Tick()
	{
		SetClass( "visible", ShouldDraw() );

		if ( !IsVisible )
			return;

		var ply = Game.LocalPawn as TFPlayer;
		var available = ply.GetAvailableBuildings();
		if ( available == default ) return;

		var current = BuildingPanels.Keys;
		foreach ( var add in available.Except( current ) ) AddBuilding( add );
		foreach ( var remove in current.Except( available ) ) RemoveBuilding( remove );

		var buildings = ply.Buildings;
		foreach ( var building in buildings.Except( CurrentBuildings ) ) AddInstance( building );
	}

	public bool ShouldDraw()
	{
		return Game.LocalPawn is TFPlayer ply && ply.UsesMetal;
	}

	private void AddBuilding(string id)
	{
		BuildingInstancePanel panel = new( id );
		AddChild( panel );
		BuildingPanels.Add( id, panel );
	}

	private void RemoveBuilding(string id)
	{
		if ( !BuildingPanels.TryGetValue( id, out var panel ) )
			return;

		panel.Delete();
		BuildingPanels.Remove( id );
	}

	private void AddInstance(TFBuilding building)
	{
		if ( !building.IsValid() ) return;

		string id = building.Data.ResourceName;
		if ( !BuildingPanels.TryGetValue( id, out var panel ) ) return;

		CurrentBuildings.Add( building );
		panel.UpdateBuilding( building );
	}
}

public partial class BuildingInstancePanel : Panel
{
	public BuildingData Data { get; set; }
	public TFBuilding CurrentBuilding { get; set; }
	bool HasBuilding;

	Panel HealthContainer;
	List<Panel> HealthPanels = new();

	Image LevelIcon;
	Image BuildingIcon;

	Panel LineRoot;
	Label NoLineText;
	List<BuildingInfoLinePanel> LinePanels = new();
	public BuildingInstancePanel(string id)
	{
		Data = BuildingData.Get( id );
		if(Data == null)
		{
			throw new ArgumentException( "Invalid building data name!", nameof( id ) );
		}

		bool big = Data.BigPanel;

		HealthContainer = Add.Panel( "health" );
		int healthSegments = big ? 15 : 6;
		for ( int i = 0; i < healthSegments; i++ )
		{
			var healthPanel = HealthContainer.Add.Panel( "segment" );
			HealthPanels.Add( healthPanel );
		}

		var iconPanel = Add.Panel( "status" );
		BuildingIcon = iconPanel.Add.Image( Data.GetUIIcon(1), "building" );
		LevelIcon = iconPanel.Add.Image( GetLevelIcon( 1 ), "level" );

		LineRoot = Add.Panel( "lines" );
		NoLineText = Add.Label( $"{Data.UIName ?? Data.Title}\nNot Built", "no_lines" );

		SetClass( "big", big );
		HealthContainer.SetClass( "big", big );

		UpdateBuilding( null );
	}
	public override void Tick()
	{
		if(!CurrentBuilding.IsValid() && HasBuilding )
		{
			UpdateBuilding( null );
			return;
		}

		if(HasBuilding)
		{
			float healthPercent = CurrentBuilding.Health / CurrentBuilding.MaxHealth;
			int healthyElements = MathX.CeilToInt( HealthPanels.Count * healthPercent );
			for ( int i = 0; i < HealthPanels.Count; i++ )
			{
				if(i + 1 <= healthyElements)
				{
					HealthPanels[i].SetClass( "active", true );
				}
				else
				{
					HealthPanels[i].SetClass( "active", false );
				}
			}

			var buildIcon = Data.GetUIIcon( CurrentBuilding.Level );
			if ( BuildingIcon.Texture.ResourcePath != buildIcon )
				BuildingIcon.SetTexture( buildIcon );

			var lvlIcon = GetLevelIcon(CurrentBuilding.Level);
			if ( LevelIcon.Texture.ResourcePath != lvlIcon )
				LevelIcon.SetTexture( lvlIcon );
		}
	}
	public void UpdateBuilding(TFBuilding building)
	{
		CurrentBuilding = building;

		HasBuilding = CurrentBuilding != null;
		SetClass( "no_building", !HasBuilding );
		SetClass( "has_building", HasBuilding );

		HealthContainer.SetClass( "visible", HasBuilding );
		LevelIcon.SetClass( "visible", HasBuilding );
		NoLineText.SetClass( "visible", !HasBuilding );

		foreach ( var linePanel in LinePanels )
			linePanel.Delete();

		LinePanels.Clear();

		if (HasBuilding)
		{
			SetClass( "blu", building.Team == TFTeam.Blue );
			SetClass( "red", building.Team == TFTeam.Red );

			LevelIcon.SetTexture( GetLevelIcon( building.Level ) );

			var lines = building.GetUILines();
			if ( lines != null && lines.Any() )
			{
				foreach ( var line in lines )
				{
					BuildingInfoLinePanel panel = new( line );
					LineRoot.AddChild( panel );
					LinePanels.Add( panel );
				}
			}
		}
		else
		{
			BuildingIcon.SetTexture( Data.GetUIIcon( 1 ) );
		}
	}

	private static string GetLevelIcon(int level)
	{
		return $"ui/HUD/Buildings/hud_upgrade_{level}.png";
	}
}

public class BuildingInfoLinePanel : Panel
{
	private class ProgressBar : Panel
	{
		Panel Progress;
		public ProgressBar()
		{
			Progress = Add.Panel( "progress" );
		}

		public void SetProgress(float progress)
		{
			Progress.Style.Width = Length.Percent( progress * 100 );
		}
	}

	BuildingInfoLine Line;
	Panel Value;

	public BuildingInfoLinePanel(BuildingInfoLine line)
	{
		if ( line == null )
		{
			throw new ArgumentException( "Invalid info line!", nameof( line ) );
		}

		Line = line;

		Image icon = new();
		icon.AddClass( "icon" );
		icon.SetTexture( line.IconPath );
		AddChild( icon );
	}

	public override void Tick()
	{
		if(Line == null)
		{
			Delete();
			return;
		}

		SetClass( "visible", Line.Visible );
		if ( !IsVisible ) return;

		if ( Line.ShowText)
		{
			if(Value is Label lbl)
			{
				lbl?.SetText(Line.Text);
			}
			else
			{
				Value?.Delete();
				Value = Add.Label( Line.Text, "text" );
			}
		}
		else
		{
			if(Value is ProgressBar bar)
			{
				float percent = (Line.Value - Line.MinValue) / Line.MaxValue;
				bar?.SetProgress( percent );
			}
			else
			{
				Value?.Delete();
				Value = AddChild<ProgressBar>( "value" );
			}
		}
	}
}
