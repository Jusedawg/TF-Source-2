using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;
[GameResource( "TF:S2 Building Data", "tfbuild", "Team Fortress: Source 2 building definitions", Icon = "build_circle", IconBgColor = "#ff6861", IconFgColor = "#0e0e0e" )]
public class BuildingData : GameResource
{
	/// <summary>
	/// All registered Buildings.
	/// </summary>
	public static IReadOnlyList<BuildingData> All => _all;
	private static List<BuildingData> _all = new();
	public static BuildingData Get(string name) => _all.FirstOrDefault(building => building.ResourceName== name);

	/// <summary>
	/// Title of the weapon that will be displayed to the client.
	/// </summary>
	public string Title { get; set; }
	/// <summary>
	/// Engine entity classname.
	/// </summary>
	public string EngineClass { get; set; }
	public int BuildCost { get; set; } = 100;
	public float BuildTime { get; set; } = 10f;
	public int UpgradeCost { get; set; } = 200;
	
	public List<BuildingLevelData> Levels { get; set; } = new();
	[HideInEditor]
	public int LevelCount => Levels.Count;
	[Category( "BBox" )]
	public Vector3 Mins { get; set; } = new( -20, -20, 0 );
	[Category( "BBox" )]
	public Vector3 Maxs { get; set; } = new( 20, 20, 55 );
	[HideInEditor]
	public BBox BBox => new( Mins, Maxs );

	[Category("Blueprint")]
	[ResourceType( "vmdl" )]
	public string BlueprintModel { get; set; }
	/// <summary>
	/// Icon used for things like the PDA
	/// </summary>
	[Category( "Blueprint" )]
	[ResourceType( "png" )]
	public string BlueprintIcon { get; set; }
	/// <summary>
	/// Creates an instance of this weapon.
	/// </summary>
	/// <returns></returns>
	public TFBuilding CreateInstance()
	{
		if ( string.IsNullOrEmpty( EngineClass ) )
			return null;

		if(Levels.Count == 0)
		{
			Log.Warning( $"Tried to create instance of data with no levels!" );
			return null;
		}

		var building = TypeLibrary.Create<TFBuilding>( EngineClass, false );
		if(building == null)
		{
			Log.Error( $"Tried to create building with invalid engine class {EngineClass}!" );
			return null;
		}
		building.Initialize( this );

		return building;
	}
	protected override void PostLoad()
	{
		if ( Levels.Count == 0 )
		{
			Log.Error( $"Tried to load building {this} with no levels!" );
			return;
		}

		Precache.Add( BlueprintModel );
		Precache.Add( BlueprintIcon );

		foreach ( var level in Levels )
		{
			Precache.Add( level.Model );
			Precache.Add( level.DeployModel );
		}

		// Add this asset to the registry.
		_all.Add( this );
	}
}

public struct BuildingLevelData
{
	public float MaxHealth { get; set; }
	/// <summary>
	/// Model used while the building is active at this level
	/// </summary>
	[ResourceType( "vmdl" )]
	public string Model { get; set; }
	/// <summary>
	/// Model used while this level is being constructed / upgraded to
	/// </summary>
	[ResourceType( "vmdl" )]
	public string DeployModel { get; set; }
}
