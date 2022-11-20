using Sandbox.UI;
using Amper.FPS;
using Sandbox;
using System;

namespace TFS2.UI;

[UseTemplate]
public partial class TFChatBoxEntry : Panel
{
	public TimeSince TimeSinceCreated { get; set; }

	Panel Wrapper { get; set; }
	ColorFormattedString Text { get; set; }
	bool IsFaded => TimeSinceCreated > (hud_saytext_time + hud_saytext_fadetime);
	bool IsFading => !IsFaded && TimeSinceCreated > hud_saytext_time;

	public TFChatBoxEntry( ColorFormattedString text )
	{
		TimeSinceCreated = 0;
		Text = text;
		Wrapper.AddChild( text );
	}

	public override void Tick()
	{
		base.Tick();

		Wrapper.Style.Opacity = GetOpacity();

		if ( TimeSinceCreated > 30 )
			Delete();
	}

	public float GetOpacity()
	{
		if ( TFChatBox.Instance.IsOpen )
			return 1;

		if ( IsFaded )
			return 0;

		if ( IsFading )
		{
			float lerp = (TimeSinceCreated - hud_saytext_time) / hud_saytext_fadetime;
			return lerp.RemapClamped( 0, 1, 1, 0 );
		}

		return 1;
	}

	[ConVar.Client] public static float hud_saytext_time { get; set; } = 15;
	[ConVar.Client] public static float hud_saytext_fadetime { get; set; } = 1;
}
