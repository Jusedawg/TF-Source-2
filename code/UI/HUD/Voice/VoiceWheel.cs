using Sandbox;
using Sandbox.UI;
using System;

namespace TFS2;

public partial class VoiceWheel : Panel
{
	public const int MaxSlices = 8; // 45 degrees each.
}

public partial class VoiceWheelBackground : Panel
{
	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		var center = new Vector2( (state.Width / 2) - state.X, (state.Height / 2) - state.Y );

		/*
		var draw = Render.Draw2D;

		draw.Ring( center, 200, 199, 64 );
		draw.Ring( center, 100, 99, 64 );

		draw.Texture = Texture.Load( "https://cdn.discordapp.com/emojis/868602055680458753.png" );
		draw.Ring( center, 199, 100, 64 );
		*/

		//draw.Texture = Texture.White;
		var color = Color.White;
		var material = Material.UI.Basic;

		for ( var degree = 0f; degree <= 360; degree += 10 )
		{
			var rads = degree.DegreeToRadian();
			var radsL = (degree + .5f).DegreeToRadian();
			var radsR = (degree - .5f).DegreeToRadian();

			Vector2 dir = new Vector2( MathF.Sin( 0f - rads ), MathF.Cos( 0f - rads ) );
			Vector2 dirL = new Vector2( MathF.Sin( 0f - radsL ), MathF.Cos( 0f - radsL ) );
			Vector2 dirR = new Vector2( MathF.Sin( 0f - radsR ), MathF.Cos( 0f - radsR ) );
			var p1 = dir * 100 + center;

			var p2 = dirL * 200 + center;
			var p3 = dir * 200 + center;
			var p4 = dirR * 200 + center;

			Vertex[] verts = new Vertex[4] { 
				new( p1, new( 0, 0 ), color ), 
				new( p2, new( 0, 0 ), color ), 
				new( p3, new( 0, 0 ), color ), 
				new( p4, new( 0, 0 ), color ) 
			};

			Graphics.Draw( verts, verts.Length, material );

			/*
			draw.MeshStart();
			draw.AddVertex( in p1, draw.Color );
			draw.AddVertex( in p2, draw.Color );
			draw.AddVertex( in p3, draw.Color );
			draw.AddVertex( in p4, draw.Color );
			draw.MeshEnd();
			*/
		}
	}
}
