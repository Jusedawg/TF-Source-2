using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2.UI;

public partial class PayloadPath : Panel
{
	private class PathInfo
	{
		public float Distance;
		public float Length;
	}

	public Cart Cart { get; set; }
	private Cart lastCart;

	TFTeam CartTeam;
	float PathLength;
	Dictionary<CartPath, PathInfo> _pathLengths;
	Dictionary<CartPath.Section, Panel> PathSections;
	Dictionary<CartPath.ControlPointInfo, Panel> ControlPoints;

	Panel ProgressBar;
	Panel HomePanel;

	Panel CartContainer;
	Panel CartPanel;
	Label StatusLabel;
	Label PointerMessage;

	public override void Tick()
	{
		if ( !Cart.IsValid() || !Cart.IsLoaded ) return;

		if ( Cart != lastCart )
			InitCart( Cart );

		if ( Cart == null ) return;

		float cartFraction = CartFraction();
		Length cartPos = FractionLength( cartFraction );
		Length remainder = FractionLength( 1 - cartFraction );

		if(Cart.tf_debug_cart)
		{
			DebugOverlay.ScreenText( $"CartPos: {cartPos}" );
			DebugOverlay.ScreenText( $"Remainder: {remainder}", 1 );

			DebugOverlay.ScreenText( $"CartFraction: {cartFraction}", 2 );
			DebugOverlay.ScreenText( $"PathLength: {_pathLengths}", 3 );
			DebugOverlay.ScreenText( $"CartDistance: {Cart.GetCurrentDistance()}", 4 );
		}


		var cartPanelPos = cartPos;
		float cartHalfWidth = 6f;
		cartPanelPos.Value -= cartHalfWidth;
		CartContainer.Style.Left = cartPanelPos;

		if ( CartTeam == TFTeam.Blue)
		{
			CartPanel.SetClass( "blu", true );
			CartPanel.SetClass( "red", false );

			HomePanel.SetClass( "blu", true );
			HomePanel.SetClass( "red", false );
		}
		else
		{
			CartPanel.SetClass( "blu", false );
			CartPanel.SetClass( "red", true );

			HomePanel.SetClass( "blu", false );
			HomePanel.SetClass( "red", true );
		}

		if ( Cart.Pushers.Any() )
			StatusLabel.Text = $"x{Cart.GetCapRate()}";
		else
			StatusLabel.Text = "";

		foreach((CartPath.ControlPointInfo info, Panel panel) in ControlPoints)
		{
			float fraction = ControlPointFraction( info );
			Length cpPos = FractionLength( fraction );

			if(fraction.AlmostEqual(1))
			{
				// Offset the control point position by half the image (TODO: Get this value at runtime)
				const float LAST_CP_HALF_WIDTH = 5f;
				cpPos.Value -= LAST_CP_HALF_WIDTH;
			}
			else
			{
				const float CP_HALF_WIDTH = 5f;
				cpPos.Value -= CP_HALF_WIDTH;
			}

			panel.Style.Left = cpPos;

			switch(info.Point.OwnerTeam)
			{
				case TFTeam.Blue:
					panel.SetClass( "blu", true );
					panel.SetClass( "red", false );
					panel.SetClass( "neutral", false );
					break;
				case TFTeam.Red:
					panel.SetClass( "blu", false );
					panel.SetClass( "red", true );
					panel.SetClass( "neutral", false );
					break;
				default:
					panel.SetClass( "blu", false );
					panel.SetClass( "red", false );
					panel.SetClass( "neutral", true );
					break;
			}
		}
	}

	private void InitCart(Cart cart)
	{
		var paths = cart.GetPaths();
		if(paths == null || !paths.Any())
		{
			Log.Error( "Cant have path UI with no paths!" );
			return;
		}

		if(lastCart != null)
		{
			foreach ( var section in PathSections )
				section.Value.Delete();

			foreach ( var cp in ControlPoints )
				cp.Value.Delete();
		}

		lastCart = Cart;
		Cart = cart;

		CartTeam = cart.Team;
		_pathLengths = new();
		PathSections = new();
		ControlPoints = new();

		foreach(var path in paths)
		{
			PathInfo info = new()
			{
				Distance = _pathLengths?.Values?.Sum( info => info.Length ) ?? 0f,
				Length = path?.GetFullLength() ?? 0f
			};
			_pathLengths.Add( path, info);

			var sections = path.GetSections();
			var cps = path.GetControlPoints();
			foreach ( var section in sections )
			{
				PathSections.Add( section, ProgressBar.Add.Panel( "section" ) );
			}

			foreach ( var cp in cps )
			{
				ControlPoints.Add( cp, ProgressBar.Add.Panel( "point neutral" ) );
			}

			//Log.Info( string.Join(',', sections ));
		}

		PathLength = _pathLengths.Values.Sum(info => info.Length);

		// Check if teres a single control point which has been excluded for the path, this is common in payload maps with deathpits!
		var leftOutCps = ControlPoint.All.Except( ControlPoints.Keys.Select( info => info.Point ) );
		if ( leftOutCps.Count() == 1 )
		{
			var singleLeftOut = leftOutCps.FirstOrDefault();
			if ( singleLeftOut.OwnerTeam != CartTeam )
			{
				var lastPath = paths.Last();
				ControlPoints.Add( new() { Path = lastPath, Distance = -1, Point = singleLeftOut}, ProgressBar.Add.Panel( "point neutral" ) );
			}
		}
	}

	private Length FractionLength(float fraction)
	{
		return Length.Fraction( fraction ) ?? 0;
	}

	/// <summary>
	/// Gets the distance from the left the cart element should be
	/// </summary>
	/// <returns></returns>
	private float CartFraction()
	{
		var distance = Cart.GetCurrentDistance() + PathOffset(Cart.Path);
		return distance / PathLength;
	}

	private float PathOffset( CartPath path ) => _pathLengths?.GetValueOrDefault( path )?.Distance ?? 0f;
	private float ControlPointFraction(CartPath.ControlPointInfo info)
	{
		if ( info.Distance == -1 ) return 1;

		return (info.Distance + PathOffset( info.Path ) ) / PathLength;
	}
}
