using Sandbox;
using Amper.FPS;

namespace TFS2;

public abstract partial class GamemodeEntity : Entity, IGamemode
{
	public virtual string Title => ClassName;
	public virtual string Icon => IGamemode.DEFAULT_ICON;

	public virtual GamemodeProperties Properties => default;

	public GamemodeEntity()
	{
		EventDispatcher.Subscribe<RoundEndEvent>( RoundEnd, this );
		EventDispatcher.Subscribe<RoundActiveEvent>( RoundActivate, this );
		EventDispatcher.Subscribe<RoundRestartEvent>( RoundRestart, this );
	}

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}

	public virtual void Reset() { }
	public virtual bool IsActive() => true;
	public abstract bool HasWon( out TFTeam team, out TFWinReason reason );

	[Event.Tick.Server] public virtual void Tick() { }
	[Event.Entity.PostSpawn] public virtual void PostLevelSetup() { }

	public virtual void RoundEnd( RoundEndEvent args ) { }
	public virtual void RoundActivate( RoundActiveEvent args ) { }
	public virtual void RoundRestart( RoundRestartEvent args ) { Reset(); }

}
