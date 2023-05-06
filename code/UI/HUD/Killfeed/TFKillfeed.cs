using Sandbox;
using Sandbox.UI;
using System.IO;
using System.Linq;
using Amper.FPS;

namespace TFS2.UI;

public partial class TFKillFeed : Panel
{
	public static string DefaultKillIcon => "ui/deathnotice/skull.png";
	public static string CaptureIconRed => "ui/deathnotice/captured_red.png";
	public static string CaptureIconBlue => "ui/deathnotice/captured_blue.png";
	public static string BlockedIconRed => "ui/deathnotice/blocked_red.png";
	public static string BlockedIconBlue => "ui/deathnotice/blocked_blue.png";

	public TFKillFeed()
	{
		EventDispatcher.Subscribe<PlayerDeathEvent>( OnDeath, this );
		EventDispatcher.Subscribe<ControlPointCapturedEvent>( OnPointCaptured, this );
	}

	[GameEvent.Tick.Server]
	public override void Tick()
	{
		base.Tick();

		// Move the arena player count down if in specator mode.
		SetClass( "if-spectator", true );
	}

	public void OnPointCaptured( ControlPointCapturedEvent args )
	{
		var entry = new TFKillFeedEntry(args, CaptureIconBlue, CaptureIconRed);
		AddChild( entry );
	}

	public void OnDeath( PlayerDeathEvent args )
	{
		var entry = new TFKillFeedEntry(args.Attacker, args.Victim, args.Assister, args.Weapon, args.Tags, DefaultKillIcon);

		AddChild( entry );
	}
}
