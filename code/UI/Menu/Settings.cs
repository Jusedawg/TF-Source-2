using Sandbox;
using Sandbox.UI;

namespace TFS2.UI;

public partial class Settings : MenuOverlay
{
	[ConVar.ClientData] public static bool cl_autoreload { get; set; } = true;
	[ConVar.ClientData] public static bool tf_sniper_autoscope_enabled { get; set; } = true;
	[ConVar.ClientData] public static float tf_hitsound_volume { get; set; } = 1;
	[ConVar.ClientData] public static float tf_killsound_volume { get; set; } = 1;
	public void OnClickClose()
	{
		Close();
	}
}
