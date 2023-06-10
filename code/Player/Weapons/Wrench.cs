using System.Linq;
using Sandbox;
using Amper.FPS;

namespace TFS2;

[Library( "tf_weapon_wrench" )]
public class Wrench : TFMeleeBase 
{
	const string BUILDING_HIT_SUCCESS = "weapon_wrench.hit.building.success";
	const string BUILDING_HIT_FAIL = "weapon_wrench.hit.building.fail";
	bool wasSuccess;
	public virtual string[] AvailableBuildings => new[] { "sentry", "dispenser", "teleporter_entrance", "teleporter_exit" };

	protected override TraceResult FireBulletServer( float damage, int seedOffset = 0 )
	{
		// Same as built in FireBulletServer but with a point check
		Game.AssertServer();
		var tr = TraceFireBullet( seedOffset );

		// We didn't hit any entity, early out.
		var entity = tr.Entity;
		if ( entity == null )
			return tr;

		if(entity is not TFBuilding)
		{
			const float POINT_CHECK_RADIUS = 0.01f;
			var ents = FindInSphere( tr.StartPosition, POINT_CHECK_RADIUS ).Except( new Entity[] { this, Owner } );
			DebugOverlay.Sphere( tr.StartPosition, POINT_CHECK_RADIUS, Color.Red );
			if ( ents.Any() )
			{
				// Manually provide this information
				tr.Hit = true;
				tr.Entity = ents.First();
				tr.EndPosition = tr.StartPosition;
				tr.HitPosition = tr.EndPosition;
			}
		}


		OnHitEntity( tr.Entity, tr );

		var info = CreateDamageInfo( tr, damage );
		ApplyDamageModifications( entity, ref info, tr );
		entity.TakeDamage( info );

		return tr;
	}
	public override void OnHitEntity( Entity entity, TraceResult tr )
	{
		base.OnHitEntity( entity, tr );

		if(entity is TFBuilding building)
		{
			wasSuccess = false;
			if (building.IsConstructing)
			{
				building.ApplyConstructionBoost( this );
				wasSuccess = true;
			}
			else if (!building.IsUpgrading)
			{
				int usedMetal = building.ApplyMetal( TFOwner.Metal );
				TFOwner.ConsumeMetal( usedMetal );
				if ( usedMetal > 0 )
					wasSuccess = true;
			}
		}
	}

	public override void PlayImpactSound( Entity entity )
	{
		if(entity is TFBuilding)
		{
			if ( wasSuccess )
				PlaySound( BUILDING_HIT_SUCCESS );
			else
				PlaySound( BUILDING_HIT_FAIL );

			return;
		}
		else
			base.PlayImpactSound( entity );
	}
}
