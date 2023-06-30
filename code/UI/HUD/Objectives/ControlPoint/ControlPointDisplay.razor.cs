using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace TFS2.UI;

partial class ControlPointDisplay : Panel
{
	Dictionary<ControlPoint, ControlPointDisplayEntry> PointEntries { get; set; } = new();
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
		PointEntries[point] = new ControlPointDisplayEntry
		{
			Point = point,
			Parent = this
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

	public bool ShouldDraw() => TFGameRules.Current.IsPlaying<ControlPoints>();

	public void ReorderEntries()
	{
		var order = TFGameRules.Current.GetControlPointRouteForTeam( TFTeam.Blue );
		if ( order == null ) return;

		SortChildren( ( x, y ) => {
			// If either of the panels are null, dont sort this.
			if ( x is not ControlPointDisplayEntry x1 || y is not ControlPointDisplayEntry y1 )
				return 0;

			var pointX = x1.Point;
			var pointY = y1.Point;

			// If either of the panels dont refer to a valid control point, dont sort this.
			if ( pointX == null || pointY == null ) 
				return 0;

			var indexX = order.IndexOf( pointX );
			var indexY = order.IndexOf( pointY );

			if ( indexX == indexY )
				return 0;

			return indexX > indexY ? 1 : -1;
		} );
	}
}
