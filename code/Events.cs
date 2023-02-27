using Sandbox;
using Amper.FPS;
using System.Collections.Generic;

namespace TFS2;

#region Player

[EventDispatcherEvent]
public class PlayerSpawnEvent : DispatchableEventBase
{
	public IClient Client { get; set; }
	public TFTeam Team { get; set; }
	public PlayerClass Class { get; set; }
}

[EventDispatcherEvent]
public class PlayerDeathEvent : DispatchableEventBase
{
	public IClient Victim { get; set; }
	public IClient Attacker { get; set; }
	public IClient Assister { get; set; }
	public WeaponData Weapon { get; set; }
	public string[] Tags { get; set; }
	public Vector3 Position { get; set; }
	public float Damage { get; set; }
}

[EventDispatcherEvent]
public class PlayerHurtEvent : DispatchableEventBase
{
	public IClient Victim { get; set; }
	public IClient Attacker { get; set; }
	public IClient Assister { get; set; }
	public WeaponData Weapon { get; set; }
	public string[] Tags { get; set; }
	public Vector3 Position { get; set; }
	public float Damage { get; set; }
}

[EventDispatcherEvent]
public class PlayerChangeClassEvent : DispatchableEventBase
{
	public IClient Client { get; set; }
	public PlayerClass Class { get; set; }
}

[EventDispatcherEvent]
public class PlayerChangeTeamEvent : DispatchableEventBase
{
	public IClient Client { get; set; }
	public TFTeam Team { get; set; }
}

[EventDispatcherEvent]
public class PlayerRegenerateEvent : DispatchableEventBase
{
	public IClient Client { get; set; }
}

#endregion

#region Game

[EventDispatcherEvent]
public class GameResetEvent : DispatchableEventBase { }

[EventDispatcherEvent]
public class GameOverEvent : DispatchableEventBase { }

#endregion

#region Control Points

[EventDispatcherEvent]
public class ControlPointCapturedEvent : DispatchableEventBase
{
	public ControlPoint Point { get; set; }
	public TFTeam NewTeam { get; set; }
	public IClient[] Cappers { get; set; }
}

#endregion

#region Flags
[EventDispatcherEvent]
public class FlagCapturedEvent : DispatchableEventBase
{
	public Flag Flag { get; set; }
	public TFPlayer Capper { get; set; }
	public FlagCaptureZone Zone { get; set; }
}

[EventDispatcherEvent]
public class FlagPickedUpEvent : DispatchableEventBase
{
	public Flag Flag { get; set; }
	public TFPlayer Capper { get; set; }
}

[EventDispatcherEvent]
public class FlagDroppedEvent : DispatchableEventBase
{
	public Flag Flag { get; set; }
	public TFPlayer Capper { get; set; }
}

[EventDispatcherEvent]
public class FlagReturnedEvent : DispatchableEventBase
{
	public Flag Flag { get; set; }
}
#endregion
