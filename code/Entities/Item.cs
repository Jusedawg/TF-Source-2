using Sandbox;
using Amper.FPS;

namespace TFS2;

public abstract partial class Item : AnimatedEntity
{
	/// <summary>
	/// How far the flag should be above ground when dropped
	/// </summary>
	public virtual float DistanceOverGround => 8f;
	/// <summary>
	/// Cooldown before item can be picked up again.
	/// </summary>
	public virtual float PickupCooldown => 0.75f;
	/// <summary>
	/// How long it takes for the item to return.
	/// If the item isnt set to respawn, the flag just gets deleted after this (but the outputs still get fired!)
	/// </summary>
	[Property, Net] public int ReturnTime { get; set; } = 60;
	public Transform SpawnState { get; set; }
	public TFPlayer TFOwner => Owner as TFPlayer;

	public override void Spawn()
	{
		UsePhysicsCollision = true;
		Tags.Add( CollisionTags.Interactable );

		EnableTouch = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		SpawnState = Transform;

		base.Spawn();
	}

	[GameEvent.Tick.Server]
	public virtual void Tick()
	{
	}

	public virtual void Pickup( TFPlayer player )
	{
		Owner = player;
		SetParent( player );

		player.PickedItem = this;
	}

	public virtual bool Drop()
	{
		if ( !Game.IsServer || Owner == null ) return false;

		TFOwner.PickedItem = null;

		// Clear the parent.
		SetParent( null );
		Parent = null;
		Owner = null;

		return true;
	}

	/// <summary>
	/// Check if the player specified can pick this thing up
	/// </summary>
	public virtual bool CanBePickedBy( TFPlayer player )
	{
		// if we already have a carrier don't pickup
		if ( Owner != null )
			return false;

		return true;
	}

	/// <summary>
	/// What to do when we pick up this item?
	/// </summary>
	public virtual void OnPickedUp()
	{
	}

	/// <summary>
	/// What to do when we drop this item?
	/// </summary>
	public virtual void OnDropped()
	{
	}

	/// <summary>
	/// What to do when we are returned?
	/// </summary>
	public virtual void OnReturned()
	{
	}
}
