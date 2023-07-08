using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TFS2.UI;

[Library( "tf_ui_config" ), HammerEntity]
[Title( "UI Config" )]
[Category( "Configuration" )]
[Icon( "architecture" )]
public partial class UIConfig : Entity
{
	public static bool Exists => Current != null;
	public static UIConfig Current { get; set; }
	/// <summary>
	/// The order of control point in the UI
	/// 
	/// Use , to seperate in one line and use space for a new line
	/// </summary>
	[Title( "Control Point Layout" ), Property, Net] public string ControlPointLayoutInput { get; set; }
	public string[][] ControlPointOrder { get; set; }
	public int ControlPointRows => ControlPointOrder.Length;
	public UIConfig()
	{
		Current = this;
		Transmit = TransmitType.Always;
	}

	public override void ClientSpawn()
	{
		ControlPointOrder = EntityUtils.SplitTargetNameLists( ControlPointLayoutInput );
	}

	public int GetControlPointRow( string controlPointName )
	{
		return Array.IndexOf( ControlPointOrder,
			ControlPointOrder.FirstOrDefault(
				row => row.Contains( controlPointName )
			)
		);
	}

	public int GetControlPointRow( ControlPoint controlPoint ) => GetControlPointRow( controlPoint.Name );

	public int GetControlPointIndex( string controlPointName )
	{
		return Array.IndexOf( ControlPointOrder.FirstOrDefault( row => row.Contains( controlPointName ) ), controlPointName );
	}

	public int GetControlPointIndex( ControlPoint controlPoint ) => GetControlPointIndex( controlPoint.Name );
}
