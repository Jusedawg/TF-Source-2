using Sandbox;

namespace Amper.FPS;

//
// Round
//

[EventDispatcherEvent] public class RoundRestartEvent : DispatchableEventBase { }
[EventDispatcherEvent] public class RoundActiveEvent : DispatchableEventBase { }
[EventDispatcherEvent] public class RoundEndEvent : DispatchableEventBase 
{
	public int WinningTeam { get; set; }
	public int WinReason { get; set; }
}

//
// Game
//

[EventDispatcherEvent] public class GameRestartEvent : DispatchableEventBase { }
[EventDispatcherEvent] public class GameOverEvent : DispatchableEventBase { }

//
// Client
//

[EventDispatcherEvent] public class ClientReadyToggleEvent : DispatchableEventBase 
{
	public IClient Client { get; set; }
	public bool Status { get; set; }
}
