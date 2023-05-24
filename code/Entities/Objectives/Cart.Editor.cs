using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;
public partial class Cart
{
	private static int previewIndex = 0;
	private static float previewFraction = 0;
	public static void DrawGizmos( EditorContext ctx )
	{
		if ( !ctx.IsSelected ) return;

		// This is a serialized version of the Entity currently being drawn
		var target = ctx.Target;
		var localTransform = Gizmo.Transform.ToLocal( target.Transform );

		string model = target.GetProperty( "model" ).As.String;
		string pathName = target.GetProperty( "Path" ).As.String;
		var pathObject = ctx.FindTarget( pathName );

		string nodesJson = pathObject?.GetProperty( "pathNodesJSON" )?.GetValue<string>();
		Log.Info( $"nodesJson: {nodesJson}/{pathName}" );

		Gizmo.Draw.Color = Color.Cyan;
		Gizmo.Draw.Model( model, localTransform );
	}
}
