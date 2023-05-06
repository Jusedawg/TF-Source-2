using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TFS2;

public class Heatmap
{
	public IReadOnlyList<HeatmapData> Data => _data;
	protected List<HeatmapData> _data = new();
	public bool Draw { get; set; } = false;
	private string loadedFile;
	const float radiusScale = 4f;

	public Heatmap()
	{
		Event.Register( this );
	}

	BaseFileSystem fs => FileSystem.Data;
	/// <summary>
	/// Create heatmap from file
	/// </summary>
	/// <param name="name">file name</param>
	public Heatmap(string name) : this()
	{
		var list = fs.ReadJson<List<HeatmapData>>( name );
		_data = list;
		loadedFile = name;
	}

	[GameEvent.Tick.Server]
	public void Tick()
	{
		if ( !Draw )
			return;

		DebugOverlay.ScreenText( $"Displaying Heatmap {loadedFile}...", -2 );
		foreach ( var d in _data )
		{
			DebugOverlay.Text( $"{d.Event}: {d.Count}", d.Position, Color.Green, maxDistance: 1024f );
			DebugOverlay.Sphere( d.Position, d.Count * radiusScale, Color.Red, 0, false );
		}
	}

	/*
	public int GridSize = 16;
	public int VerticalGridSize = 32;
	protected Vector3 Snap( Vector3 input )
	{
		return input.SnapToGrid( GridSize, true, true, false ).SnapToGrid( VerticalGridSize, false, false, true );
	}
	*/
	const int GridSize = 16;
	/// <summary>
	/// Merge close data points together
	/// </summary>
	public void MergeData()
	{
		List<HeatmapData> merged = new();
		foreach(var d in _data)
		{
			d.Position.SnapToGrid( GridSize );
			var match = merged.Find( e => e.Equals( d ) );
			if ( match != null )
				match.Count++;
			else
				merged.Add( d );
		}

		_data = merged;
	}

	public void AddData( string e, Vector3 pos )
	{
		HeatmapData add = new( e, pos );

		var match = _data.Where( d => d == add ).FirstOrDefault();
		if ( match == default )
			_data.Add( add );
		else
		{
			// the count wont update unless we do this for some reason...
			_data.Remove( match ); 
			match.Count++;
			_data.Add( match );
		}
	}

	public void SaveToFile( string name )
	{
		var fs = FileSystem.Data;
		fs.WriteJson( name, _data );
	}

	#region Viewing via command
	/// <summary>
	/// The heatmap currently being displayed via command
	/// </summary>
	private static Heatmap cmdHeatmap;
	[ConCmd.Server( "tf_heatmap_load" )]
	public static void DisplayFromFile( string name )
	{
		if ( cmdHeatmap != null )
			StopDisplaying();

		cmdHeatmap = new( name );
		cmdHeatmap.MergeData();
		cmdHeatmap.Draw = true;
		Log.Info( $"Loaded heatmap from file {name} ({cmdHeatmap._data.Count})" );
	}

	[ConCmd.Server( "tf_heatmap_unload" )]
	public static void StopDisplaying()
	{
		if ( cmdHeatmap == null )
		{
			Log.Warning( "No heatmap is being displayed!" );
			return;
		}

		cmdHeatmap.Draw = false;
		cmdHeatmap = null;
		Log.Info( "Unloaded heatmap" );
	}

	#endregion
}
public class HeatmapData : IEquatable<HeatmapData>
{
	public string Event { get; set; }
	public Vector3 Position { get; set; }
	public int Count { get; set; } = 1;

	public HeatmapData()
	{
		// json needs a default constructor
	}
	public HeatmapData( string e, Vector3 position )
	{
		Event = e;
		Position = position;
	}
	public bool Equals( HeatmapData other )
	{
		return Event.Equals( other.Event ) && Position.Equals( other.Position );
	}
}
