using Sandbox;
using Amper.FPS;

namespace TFS2;

#region Player

[EventDispatcherEvent]
public class PlayerSpawnEvent : DispatchableEventBase
{
	public Entity Client { get; set; }
	public TFTeam Team { get; set; }
	public PlayerClass Class { get; set; }
}

[EventDispatcherEvent]
public class PlayerDeathEvent : DispatchableEventBase
{
	public Entity Victim { get; set; }
	public Entity Attacker { get; set; }
	public Entity Assister { get; set; }
	public Entity Weapon { get; set; }
	public Entity Inflictor { get; set; }
	public string[] Tags { get; set; }
	public Vector3 Position { get; set; }
	public float Damage { get; set; }
}

[EventDispatcherEvent]
public class PlayerHurtEvent : DispatchableEventBase
{
	public Entity Victim { get; set; }
	public Entity Attacker { get; set; }
	public Entity Assister { get; set; }
	public Entity Weapon { get; set; }
	public Entity Inflictor { get; set; }
	public string[] Tags { get; set; }
	public Vector3 Position { get; set; }
	public float Damage { get; set; }
}

[EventDispatcherEvent]
public class PlayerChangeClassEvent : DispatchableEventBase
{
	public IClient Client { get; set; }
	public PlayerClass PreviousClass { get; set; }
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

[EventDispatcherEvent]
public class PlayerHealthKitPickUpEvent : DispatchableEventBase
{
	public float Health { get; set; }
}

[EventDispatcherEvent]
public class BuildingDeathEvent : DispatchableEventBase
{
	public TFBuilding Victim { get; set; }
	public TFPlayer Owner { get; set; }
	public Entity Attacker { get; set; }
	public Entity Weapon { get; set; }
	public string[] Tags { get; set; }
}

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
