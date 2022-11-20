using Sandbox;
using System.Collections.Generic;
using System.Linq;
using Amper.FPS;

namespace TFS2;

public abstract class PickupItem : AnimatedEntity
{
	[Property( "Model" )] public virtual string ModelPath { get; }
	[Property] public bool Respawns { get; set; } = true;
	[Property] public float RespawnTime { get; set; } = 10f;

	public PickupTrigger PickupTrigger { get; protected set; }
	private bool isRespawning = false;
	private TimeSince timeSincePickup;
	protected HashSet<TFPlayer> PlayersInContact = new();

	public override void Spawn()
	{
		base.Spawn();

		SetModel( ModelPath );
		SetAnimParameter( "idle", true );
		UsePhysicsCollision = true;

		Tags.Add( CollisionTags.Interactable );
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is TFPlayer player )
		{
			if ( ValidTouch( player ) )
			{
				PlayersInContact.Add(player);
			}
		}
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( other is TFPlayer player )
		{
			PlayersInContact.Remove( player );
		}
	}

	public virtual bool ValidTouch( TFPlayer player )
	{
		if ( player.LifeState != LifeState.Alive )
		{
			return false;
		}

		return true;
	}

	/// <summary>
	/// Override with code to perform on pickup, followed by base.OnPicked( player )
	/// </summary>
	/// <param name="player"></param>
	public virtual void OnPicked( TFPlayer player )
	{
		if ( !Respawns )
		{
			Delete();
			return;
		}
		isRespawning = true;
		EnableTouch = false;
		timeSincePickup = 0;

		// TODO: Reconsider if making the pickup half transparent would be better
		RenderColor = RenderColor.WithAlpha( 0f );
	}

	[Event.Tick.Server]
	public void Tick()
	{
		if ( isRespawning && timeSincePickup > RespawnTime )
		{
			RespawnPickup();
		}
		if ( !isRespawning )
		{
			if ( PlayersInContact.Any() )
				OnPicked( PlayersInContact.First() );
		}
	}
	protected virtual void RespawnPickup()
	{
		isRespawning = false;
		RenderColor = RenderColor.WithAlpha( 1f );
		EnableTouch = true;
	}
}
