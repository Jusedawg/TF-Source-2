using Sandbox;
using Sandbox.UI;

namespace TFS2.UI;

[UseTemplate]
partial class TFKillFeedEntry : Panel
{
	public Label AttackerName { get; set; }
	public Label PostAttackerMessage { get; set; }
	public Label VictimName { get; set; }
	public Label PostVictimMessage { get; set; }
	public Label StreakCounter { get; set; }
	public Image Icon { get; set; }
	public float LifeTimeMultiplier { get; set; } = 1;
	public float LifeTime => hud_deathnotice_time * LifeTimeMultiplier;

	TimeSince TimeSinceCreated { get; set; } = 0;

	public override void Tick()
	{
		var texture = Icon.Texture;

		if ( texture != null )
		{
			float width = texture.Width;
			float height = texture.Height;
			var aspectRatio = width / height;

			Icon.Style.AspectRatio = aspectRatio;
		}

		if ( TimeSinceCreated > LifeTime )
			Delete();
	}

	[ConVar.Client] public static float hud_deathnotice_time { get; set; } = 6;
}
