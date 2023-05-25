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
	public bool HasMetal => PlayerClass.Abilities.HasMetal;

	/// <summary>
	/// Creates a building belonging to this player at a certain position.
	/// </summary>
	/// <param name="data"></param>
	/// <param name="transform"></param>
	public void Build(BuildingData data, Transform transform)
	{
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
		ply.SwitchToWeapon( builder, true );
	}
}
