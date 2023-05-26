using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Sandbox;
using Editor;
using System.Text.Json.Serialization;

namespace TFS2
{
	[Library("tf_cart_path"), Path("tf_cart_path_node")]
	[Title("Cart Path")]
	[Category( "Objectives" )]
	[HammerEntity]
	public class CartPath : BasePathEntity<CartPathNode>
	{
		/// <summary>
		/// The mode of a section of the track
		/// </summary>
		public class Section
		{
			public CartPath Path { get; init; }
			/// <summary>
			/// The distance of this section as fraction (0..1)
			/// </summary>
			public float Distance { get; init; }
			public PathNodeMode Mode { get; init; }

			public override string ToString()
			{
				return $"{Distance}-{Mode}";
			}
		}
		public class ControlPointInfo
		{
			public CartPath Path { get; init; }
			/// <summary>
			/// The distance of this cp from the start in HU
			/// </summary>
			public float Distance { get; init; }
			public ControlPoint Point { get; init; }
			public override string ToString()
			{
				return $"{Distance}-{Point}";
			}
		}
		public List<Section> GetSections()
		{
			var sections = new List<Section>();

			CartPathNode start = PathNodes[0];
			PathNodeMode currentMode = PathNodes[0].Mode;
			foreach (var node in PathNodes)
			{
				var mode = node.Mode;
				if(currentMode != mode)
				{
					sections.Add( new Section
					{
						Path = this,
						Distance = GetFraction(GetCurveLength(start, node, PATH_DETAIL ) ),
						Mode = currentMode
					} );

					start = node;
					currentMode = node.Mode;
				}
			}

			CartPathNode lastNode = PathNodes.Last();
			if (start != lastNode)
			{
				// Add last section
				sections.Add( new Section
				{
					Path = this,
					Distance = GetFraction( GetCurveLength( start, lastNode, PATH_DETAIL ) ),
					Mode = currentMode
				} );
			}

			return sections;
		}
		public List<ControlPointInfo> GetControlPoints()
		{
			var cpNodes = PathNodes.Where( node => node.GetControlPoint() != null );
			return cpNodes.Select( node => new ControlPointInfo()
			{
				Path = this,
				Distance = GetNodeDistance( node ),
				Point = node.GetControlPoint()
			} ).ToList();
		}
		/// <summary>
		/// How much of the path a certain distance takes up.
		/// </summary>
		/// <param name="distance">Distance in Hammer Units</param>
		/// <returns>Distance in a fraction between 0 to 1 (float)</returns>
		public float GetFraction( float distance )
		{
			return MathX.Clamp( distance / GetFullLength(), 0f, 1f );
		}
		public float GetFraction( CartPathNode node )
		{
			return GetFraction( GetNodeDistance( node ) );
		}
		internal const int PATH_DETAIL = 10;
		public float GetFullLength(int detail = PATH_DETAIL)
		{
			float length = 0;
			for ( int i = 0; i < PathNodes.Count - 1; i++ )
			{
				length += GetCurveLength( PathNodes[i], PathNodes[i + 1], detail );
			}

			return length;
		}
		
		public float GetNodeDistance(CartPathNode node, int detail = PATH_DETAIL )
		{
			float length = 0;
			for ( int i = 0; i < PathNodes.Count - 1; i++ )
			{
				var current = PathNodes[i];
				if ( current == node )
				{
					return length;
				}
				length += GetCurveLength( current, PathNodes[i + 1], detail );
			}

			return length;
		}

		public float GetNodeDistance( CartPathNode start, CartPathNode node, int detail = PATH_DETAIL )
		{
			int startIndex = PathNodes.IndexOf( start );
			if ( startIndex == -1 )
				startIndex = 0;
			float length = 0;
			for ( int i = startIndex; i < PathNodes.Count - 1; i++ )
			{
				var current = PathNodes[i];
				if ( current == node )
				{
					return length;
				}
				length += GetCurveLength( current, PathNodes[i + 1], detail );
			}

			return length;
		}

		public override void Spawn()
		{
			base.Spawn();

			Transmit = TransmitType.Always;
		}

		public override void DrawPath( int segments, bool drawTangents = false )
		{
			for ( var nodeid = 0; nodeid < PathNodes.Count; nodeid++ )
			{
				BasePathNode node = PathNodes[nodeid];

				Vector3 nodePos = node.WorldPosition;

				// Nodes & IDs
				DebugOverlay.Sphere( nodePos, 4, Color.White );
				DebugOverlay.Text( $"{nodeid + 1}", nodePos + Vector3.Up * 12, 0, Color.White, 0, 2500 );
				if(node is CartPathNode cartNode)
				{
					DebugOverlay.Text( $"Mode: {cartNode.Mode}", nodePos + Vector3.Up * 12, 1, Color.White, 0, 2500 );
					if ( !string.IsNullOrEmpty(cartNode.LinkedControlPoint) )
					{
						DebugOverlay.Text( $"Linked CP Name: {cartNode.LinkedControlPoint}", nodePos + Vector3.Up * 12, 2, Color.White, 0, 2500 );
						if(cartNode.GetControlPoint() != null)
							DebugOverlay.Text( $"= Control Point: {cartNode.GetControlPoint().PrintName}", nodePos + Vector3.Up * 12, 3, Color.White, 0, 2500 );
					}

					if(!string.IsNullOrEmpty(cartNode.NextPath))
					{
						DebugOverlay.Text( $"Next Path Name: {cartNode.NextPath}", nodePos + Vector3.Up * 12, 4, Color.White, 0, 2500 );
						if ( cartNode.GetNextPath() != null )
							DebugOverlay.Text( $"= Next Path: {cartNode.GetNextPath()}", nodePos + Vector3.Up * 12, 5, Color.White, 0, 2500 );

					}
				}


				// Tangents
				if ( drawTangents )
				{
					Vector3 nodeTanIn = node.WorldTangentIn;
					DebugOverlay.Sphere( nodeTanIn, 2, Color.Yellow );
					DebugOverlay.Line( nodePos, nodeTanIn, Color.Yellow );

					Vector3 nodeTanOut = node.WorldTangentOut;
					DebugOverlay.Sphere( nodeTanOut, 6, Color.Orange );
					DebugOverlay.Line( nodePos, nodeTanOut, Color.Orange );
				}

				// The path itself
				BasePathNode nodeNext = (nodeid + 1 < PathNodes.Count) ? PathNodes[nodeid + 1] : null;
				if ( nodeNext == null ) continue;

				for ( int i = 1; i <= segments; i++ ) // Starting from i = 1 because i = 0 is start.Position
				{
					var lerpPos = GetPointBetweenNodes( node, nodeNext, (float)i / segments );

					DebugOverlay.Line( nodePos, lerpPos, Color.Green );

					nodePos = lerpPos;
				}
			}
		}
	}

	[Library("tf_cart_path_node"), PathNode]
	public partial class CartPathNode : BasePathNode
	{

		[Property( "CartMode", Title = "Movement Mode" )]
		//[JsonPropertyName( "CartMode" )]
		public PathNodeMode Mode { get; set; } = PathNodeMode.Default;
		protected CartPath Path => PathEntity as CartPath;

		/// <summary>
		/// This control point gets captured if a cart passes over this node.
		/// </summary>
		[Property(Title = "Control Point"), FGDType("target_destination")]
		public string LinkedControlPoint { get; set; }

		/// <summary>
		/// Path to jump to when this node is passed, currently only does something at the end of a path.
		/// </summary>
		[Property, FGDType( "target_destination" )]
		public string NextPath { get; set; }

		public ControlPoint GetControlPoint()
		{
			// Not sure why but this seems to be prepended to the linked control point KV of the some nodes so im bandaid fixing it for now
			// - quality 19/5/23

			if ( LinkedControlPoint.StartsWith( "[PR#]" ) ) 
				LinkedControlPoint = LinkedControlPoint.Substring( 5 );

			return Entity.FindByName( LinkedControlPoint ) as ControlPoint;
		}

		public CartPath GetNextPath()
		{
			if ( NextPath.StartsWith( "[PR#]" ) )
				NextPath = NextPath.Substring( 5 );

			return Entity.FindByName( NextPath ) as CartPath;
		}

		public override string ToString()
		{
			return $"{Mode}-{LinkedControlPoint ?? "none"}-{NextPath??"none"}";
		}
	}

	public enum PathNodeMode
	{
		Default,
		RollBack,
		RollForward
	}
}
