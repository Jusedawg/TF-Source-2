using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public partial class TFPlayer
{
	[ClientInput] public bool AutoRezoom { get; set; }
	[ClientInput] public bool AutoReload { get; set; }
	public override string UseButton => "Inspect";

	public override void BuildInput()
	{
		AutoRezoom = TFClientSettings.Current.AutoZoomIn;
		AutoReload = TFClientSettings.Current.AutoReload;

		base.BuildInput();
	}
}
