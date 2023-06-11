using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using TFS2;

namespace TFS2.UI;

public partial class Metal
{
	Label MetalAmount;

	public override void Tick()
	{
		SetClass( "visible", ShouldDraw() );

		if ( !IsVisible ) return;

		var ply = (TFPlayer)Game.LocalPawn;

		SetClass( "blue", ply.Team == TFTeam.Blue );
		SetClass( "red", ply.Team == TFTeam.Red );
		MetalAmount.Text = ply.Metal.ToString();
	}

	protected virtual bool ShouldDraw()
	{
		return Game.LocalPawn is TFPlayer ply && ply.UsesMetal;
	}
}
