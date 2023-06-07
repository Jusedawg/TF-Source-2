using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public partial class TFPlayer
{
	[Net, SkipHotload] public IList<TFBuilding> Buildings { get; set; }
	[Net] public int Metal { get; set; }
	public int MaxMetal => PlayerClass?.Abilities.Metal != null ? PlayerClass.Abilities.Metal : 0;
	public bool UsesMetal => PlayerClass?.Abilities.HasMetal == true;

	/// <summary>
	/// Creates a building belonging to this player at a certain position.
	/// </summary>
	/// <param name="data"></param>
	/// <param name="transform"></param>
	public void Build(BuildingData data, Transform transform)
	{
		if ( Game.IsClient ) return;
		if ( !CanBuild( data ) ) return;

		var building = data.CreateInstance();
		if ( building == null )
		{
			return;
		}

		building.SetOwner( this );
		building.StopCarrying( transform );
		Buildings.Add( building );
		ConsumeMetal(data.BuildCost);
	}
	public void Build( string buildingName, Transform transform ) => Build( BuildingData.Get( buildingName ), transform );
	public void SimulateBuildingPickup()
	{
		if(Game.IsServer)
		{
			if ( Input.Pressed( "Attack2" ) )
			{
				TryPickupBuilding();
			}
		}
	}
	const float PICKUP_DISTANCE = 96f;
	public void TryPickupBuilding()
	{
		if ( Game.IsClient ) return;
		if ( HoveredEntity is not TFBuilding building ) return;
		if(building.Owner != this || !building.InPickupDistance()) return;

		var builder = Weapons.OfType<Builder>().FirstOrDefault();
		if ( builder == null )
		{
			Log.Warning( "Cant build without builder weapon!" );
			return;
		}

		if ( !building.CanCarry(this) ) return;

		builder.CarryBuilding( building );
		ForceSwitchWeapon( builder );
	}
	public bool CanBuild(BuildingData data)
	{
		return Buildings.Count( building => building.Data == data ) < data.MaxCount;
	}
	public bool CanBuild( string name ) => CanBuild( BuildingData.Get( name ) );

	public string[] GetAvailableBuildings()
	{
		var wrench = Weapons.OfType<Wrench>().FirstOrDefault();
		if ( wrench != null )
			return wrench.AvailableBuildings;

		return default;
	}
	public int ConsumeMetal(int amount)
	{
		int used = (int)MathF.Min(Metal, amount);
		Metal -= used;

		return used;
	}

	public int GetUsableMetal(int targetAmount)
	{
		return (int)MathF.Min( Metal, targetAmount );
	}

	public int GiveMetal(int amount)
	{
		int given = (int)MathF.Min(MaxMetal - Metal,amount);
		Metal += amount;
		if ( Metal > MaxMetal )
			Metal = MaxMetal;

		return given;
	}

	public void DestroyBuildings()
	{
		if(Buildings.Count > 0)
		{
			for ( int i = 0; i < Buildings.Count; i++ )
			{
				Buildings[i].ManualDestroy();
			}

			Buildings.Clear();
		}
	}

	[ConCmd.Server("tf_build")]
	public static void StartBuilding(string buildingName)
	{
		if ( ConsoleSystem.Caller.Pawn is not TFPlayer ply ) return;

		var builder = ply.Weapons.OfType<Builder>().FirstOrDefault();
		if ( builder == null )
		{
			Log.Warning( "Cant build without builder weapon!" );
			return;
		}

		var data = BuildingData.Get( buildingName );
		if(data == null)
		{
			Log.Warning( $"Building with name {buildingName} does not exist!" );
			return;
		}

		if(!ply.CanBuild(data))
		{
			if(TFBuilding.tf_debug_buildings)
				Log.Warning( $"Cant start building if the player cant build a {buildingName}!" );
			return;
		}

		if(ply.Metal < data.BuildCost)
		{
			if(TFBuilding.tf_debug_buildings)
				Log.Warning( $"Cant start building if the player doesnt have enough metal" );
			return;
		}

		builder.PlacementData = data;
		ply.ForceSwitchWeapon( builder);
	}

	[ConCmd.Server( "tf_destroy" )]
	public static void DestroyBuilding(string buildingName)
	{
		if ( ConsoleSystem.Caller.Pawn is not TFPlayer ply ) return;

		var data = BuildingData.Get( buildingName );
		if ( data == null )
		{
			Log.Warning( $"Building with name {buildingName} does not exist!" );
			return;
		}

		var building = ply.Buildings?.FirstOrDefault( building => building.Data == data );
		if(building != null)
		{
			building.ManualDestroy();
		}
	}
}
