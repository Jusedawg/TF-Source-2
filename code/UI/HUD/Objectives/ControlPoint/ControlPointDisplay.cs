using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace TFS2.UI;

[UseTemplate]
partial class ControlPointDisplay : Panel
{
	Dictionary<ControlPoint, ControlPointDisplayEntry> Points { get; set; } = new();

	TimeSince TimeSinceSort { get; set; }

	public override void Tick()
	{
		SetClass( "visible", ShouldDraw() );

		var allPoints = ControlPoint.All;
		var ourPoints = Points.Keys;

		foreach ( var item in allPoints.Except( ourPoints ) ) AddPoint( item );
		foreach ( var item in ourPoints.Except( allPoints ) ) RemovePoint( item );
	}

	public void AddPoint( ControlPoint point )
	{
		Points[point] = new ControlPointDisplayEntry
		{
			Point = point,
			Parent = this
		};

		ReorderEntries();
	}

	public void RemovePoint( ControlPoint point )
	{
		if ( Points.TryGetValue( point, out var entry ) )
		{
			entry?.Delete();
			Points.Remove( point );
		}

		ReorderEntries();
	}

	public bool ShouldDraw() => TFGameRules.Current.MapHasControlPoints && !TFGameRules.Current.MapHasCarts;

	public void ReorderEntries()
	{
		var order = TFGameRules.Current.GetControlPointRouteForTeam( TFTeam.Blue ).ToList();

		SortChildren( ( x, y ) => {
			var x1 = x as ControlPointDisplayEntry;
			var y1 = y as ControlPointDisplayEntry;

			// If either of the panels are null, dont sort this.
			if ( x1 == null || y1 == null ) 
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
