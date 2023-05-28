using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public partial class TFPlayer
{
	[Net] public IList<TFBuilding> Buildings { get; set; }
	[Net] public int Metal { get; set; }
	public int MaxMetal => PlayerClass.Abilities.Metal;
	public bool UsesMetal => PlayerClass.Abilities.HasMetal;

	/// <summary>
	/// Creates a building belonging to this player at a certain position.
	/// </summary>
	/// <param name="data"></param>
	/// <param name="transform"></param>
	public void Build(BuildingData data, Transform transform)
	{
		if ( Game.IsClient ) return;

		var building = data.CreateInstance();
		if ( building == null )
		{
			return;
		}

		building.SetOwner( this );
		building.StopCarrying( transform );
		Buildings.Add( building );
	}
	public void Build( string buildingName, Transform transform ) => Build( BuildingData.Get( buildingName ), transform );

	public int ConsumeMetal(int amount)
	{
		int used = (int)MathF.Min(Metal, amount);
		Metal -= used;

		return used;
	}

	public void GiveMetal(int amount)
	{
		Metal += amount;
		if ( Metal > MaxMetal )
			Metal = MaxMetal;
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
			var dmg = DamageInfo.Generic( building.Health )
								.WithTags( "manual_destroy" );
			building.TakeDamage( dmg );
		}
	}
}
