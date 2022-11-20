using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace TFS2.UI;

[UseTemplate]
partial class PayloadDisplay : Panel
{
	Panel pathContainer { get; set; }
	Dictionary<CartPath, PayloadPath> paths { get; set; } = new();
	public override void Tick()
	{
		SetClass( "visible", ShouldDraw() );

		var currentPaths = paths.Keys;
		var pathEntities = Entity.All.OfType<CartPath>();

		foreach ( var newPath in pathEntities.Except( currentPaths ) ) AddPath( newPath );
		foreach ( var oldPath in currentPaths.Except( pathEntities ) ) RemovePath( oldPath );
	}

	public bool ShouldDraw() => TFGameRules.Current.MapHasCarts;

	public void AddPath( CartPath path )
	{
		PayloadPath element = new()
		{
			Parent = this,
			Path = path
		};

		element.Parent = pathContainer;
		paths.Add( path, element );
	}

	public void RemovePath( CartPath path )
	{
		if ( paths.TryGetValue( path, out var entry ) )
		{
			entry?.Delete();
			paths.Remove( path );
		}
	}
}
