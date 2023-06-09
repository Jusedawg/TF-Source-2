using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Sandbox;

namespace TFS2;

[Library("dispenser_touch_trigger")]
[Title("Dispenser Trigger")]
[Category("Objectives")]
[HammerEntity]
public partial class DispenserZone : BaseTrigger
{
	const string IDLE_SOUND = "building_dispenser.idle";
	const string HEALING_SOUND = "building_dispenser.heal";
	const string AMMO_SOUND = "building_dispenser.ammo";
	const string GENERATE_METAL_SOUND = "building_dispenser.generatemetal";

	const string BLU_HEAL_FX = "particles/medicgun_beam/dispenser_heal_blue.vpcf";
	const string RED_HEAL_FX = "particles/medicgun_beam/dispenser_heal_red.vpcf";
	[Property(Title = "Team")] public HammerTFTeamOption TeamOption { get; set; }
	public TFTeam Team { get; set; }
	[Property] public float HealingPerSecond { get; set; } = 10f;
	[Property] public float AmmoPercentagePerSecond { get; set; } = 0.2f;
	[Property] public int MetalPerInterval { get; set; } = 40;
	[Property] public float MetalInterval { get; set; } = 5f;
	[Property] public Vector3 HealOrigin { get; set; }
	[Net, Property] public int MaxStoredMetal { get; set; } = 400;
	[Net] public int StoredMetal { get; set; }
	TimeSince timeSinceAmmoRegenerate;
	TimeSince timeSinceMetalRegenerate;
	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;

		if ( TeamOption != HammerTFTeamOption.Any )
			Team = TeamOption.ToTFTeam();
	}
	[GameEvent.Tick.Server]
	public void Tick()
	{
		if ( !Enabled )
		{
			if(healParticles.Count > 0)
			{
				foreach ( var ply in TouchingEntities.OfType<TFPlayer>() )
					DestroyEffects( ply );
			}

			return;
		}

		var targets = TouchingEntities.OfType<TFPlayer>();

		float healing = HealingPerSecond * Time.Delta;
		foreach (var ply in targets)
		{
			Heal( ply, healing );
		}

		if ( timeSinceAmmoRegenerate >= 1 )
		{
			timeSinceAmmoRegenerate = 0;

			foreach ( var ply in targets )
			{
				GiveAmmo( ply );
			}
		}

		if( timeSinceMetalRegenerate >= MetalInterval )
		{
			GenerateMetal();
		}

		TickEffects( targets );
	}
	void Heal(TFPlayer ply, float amount)
	{
		ply.GiveHealth( amount );
	}
	void GiveAmmo(TFPlayer ply)
	{
		Sound.FromScreen( To.Single( ply ), AMMO_SOUND );

		foreach ( var wpn in ply.Weapons.OfType<TFWeaponBase>() )
		{
			if ( !wpn.NeedsAmmo() ) continue;

			int maxAmmo = wpn.GetReserveSize();
			wpn.Reserve += MathX.CeilToInt( maxAmmo * AmmoPercentagePerSecond );
			if ( wpn.Reserve > maxAmmo )
				wpn.Reserve = maxAmmo;
		}

		if(ply.UsesMetal)
		{
			if ( StoredMetal == 0 ) return;
			int usedMetal = (int)MathF.Min( StoredMetal, MetalPerInterval );

			usedMetal = ply.GiveMetal( usedMetal );
			StoredMetal -= usedMetal;
		}
	}

	void GenerateMetal()
	{
		timeSinceMetalRegenerate = 0;
		if ( StoredMetal >= MaxStoredMetal ) return;

		Sound.FromEntity( GENERATE_METAL_SOUND, this );

		StoredMetal += MetalPerInterval;
		if ( StoredMetal > MaxStoredMetal )
			StoredMetal = MaxStoredMetal;
	}

	public override bool PassesTriggerFilters( Entity other )
	{
		// TODO: Disguise check
		return other is TFPlayer ply && ply.Team == Team;
	}

	Sound idleSound;
	Dictionary<TFPlayer, Sound> healSounds = new();
	Dictionary<TFPlayer, Particles> healParticles = new();
	private void TickEffects(IEnumerable<TFPlayer> targets)
	{
		if( !idleSound.IsPlaying)
		{
			const float iDLE_VOLUME = 0.4f;
			idleSound = Sound.FromEntity( IDLE_SOUND, this );
			idleSound.SetVolume( iDLE_VOLUME );
		}

		var healTargets = healParticles.Keys;
		foreach ( var target in targets.Except( healTargets ) )
		{
			healSounds.Add( target, Sound.FromEntity( To.Single( target ), HEALING_SOUND, this ) );
			healParticles.Add( target, CreateHealParticle( target ) );
		}

		foreach ( var oldTarget in healTargets.Except( targets ) )
		{
			DestroyEffects( oldTarget );
		}
	}

	private void DestroyEffects(TFPlayer ply)
	{
		if(healSounds.ContainsKey(ply))
		{
			healSounds[ply].Stop();
			healSounds.Remove( ply );
		}
		
		if(healParticles.ContainsKey(ply))
		{
			healParticles[ply].Destroy();
			healParticles.Remove( ply );
		}
	}

	private Particles CreateHealParticle(TFPlayer target)
	{
		Particles p;
		Vector3 origin = Transform.PointToWorld( HealOrigin );

		if (Team == TFTeam.Blue)
		{
			p = Particles.Create( BLU_HEAL_FX, origin );
		}
		else
		{
			p = Particles.Create( RED_HEAL_FX, origin );
		}

		p.SetEntityAttachment( 1, target, "back_lower" );

		return p;
	}
}
