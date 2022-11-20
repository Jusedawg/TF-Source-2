using Sandbox;
using System;
using System.Collections.Generic;

namespace TFS2;

partial class TFPlayer
{
	/// <summary>
	/// Control point we're inside right now.
	/// </summary>
	[Net] public ControlPoint ControlPoint { get; set; }
}
