using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace TFS2.UI;

partial class ControlPointDisplay : Panel
{
	Dictionary<ControlPoint, ControlPointDisplayEntry> PointEntries = new();
	List<Panel> PointRows = new();
	TimeSince TimeSinceSort { get; set; }
	IEnumerable<ControlPoint> points;
	public override void Tick()
	{
		points = ControlPoint.All;
		SetClass( "visible", ShouldDraw() );
		if ( !IsVisible )
			return;

		var ourPoints = PointEntries.Keys;

		foreach ( var item in points.Except( ourPoints ) ) AddPoint( item );
		foreach ( var item in ourPoints.Except( points ) ) RemovePoint( item );
	}

	public void AddPoint( ControlPoint point )
	{
		int row = UIConfig.Exists ? UIConfig.Current.GetControlPointRow( point ) : 0;
		while(PointRows.Count < row + 1 ) // Add enough panels to fit this control point
		{
			PointRows.Add( Add.Panel( "row" ) );
		}

		Panel rowPanel = PointRows[row];
		PointEntries[point] = new ControlPointDisplayEntry
		{
			Point = point,
			Parent = rowPanel
		};

		ReorderEntries();
	}

	public void RemovePoint( ControlPoint point )
	{
		if ( PointEntries.TryGetValue( point, out var entry ) )
		{
			entry?.Delete();
			PointEntries.Remove( point );
		}

		ReorderEntries();
	}

	public bool ShouldDraw() => ControlPoint.All.Any() && !Cart.All.Any();

	public void ReorderEntries()
	{
		var order = TFGameRules.Current.GetControlPointRouteForTeam( TFTeam.Blue );
		if ( order == null ) return;

		foreach ( var row in PointRows )
		{
			row.SortChildren<ControlPointDisplayEntry>( GetEntryOrder );
		}
	}

	private int GetEntryOrder(ControlPointDisplayEntry entry)
	{
		if ( UIConfig.Current == null ) return 0;

		return UIConfig.Current.GetControlPointIndex( entry.Point );
	}
}
