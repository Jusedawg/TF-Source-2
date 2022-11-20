using Sandbox;
using Sandbox.UI;

namespace TFS2.UI;

[UseTemplate]
public class Settings : MenuOverlay
{
	[ConVar.ClientData] public static bool cl_autoreload { get; set; } = true;
	[ConVar.ClientData] public static float tf_hitsound_volume { get; set; } = 1;
	[ConVar.ClientData] public static float tf_killsound_volume { get; set; } = 1;
	[ConVar.ClientData] public static bool tf_sniper_autoscope_enabled { get; set; } = true;

	Checkbox AutoReload { get; set; }
	public float HitsoundVolume { get; set; }
	public float KillsoundVolume { get; set; }
	Checkbox AutoScope { get; set; }

	public Settings()
	{
		AutoReload.Checked = cl_autoreload;
		HitsoundVolume = tf_hitsound_volume;
		KillsoundVolume = tf_killsound_volume;
		AutoScope.Checked = tf_sniper_autoscope_enabled;
	}

	public override void Tick()
	{
		base.Tick();
		cl_autoreload = AutoReload.Checked;
		tf_hitsound_volume = HitsoundVolume;
		tf_killsound_volume = KillsoundVolume;
		tf_sniper_autoscope_enabled = AutoScope.Checked;

		AutoReload.SetClass( "checked", cl_autoreload );
		AutoScope.SetClass( "checked", tf_sniper_autoscope_enabled );
	}

	public void OnClickClose()
	{
		CloseActive();
	}
}
