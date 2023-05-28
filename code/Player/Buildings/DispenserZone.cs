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
	const string HEALING_SOUND = "building_dispenser.heal";
	const string AMMO_SOUND = "building_dispenser.ammo";
	const string GENERATE_METAL_SOUND = "building_dispenser.generatemetal";

	[Property(Title = "Team")] public HammerTFTeamOption TeamOption { get; set; }
	public TFTeam Team { get; set; }
	[Property] public float HealingPerSecond { get; set; } = 10f;
	[Property] public float AmmoPercentagePerSecond { get; set; } = 0.2f;
	[Property] public int MetalPerInterval { get; set; } = 40;
	[Property] public float MetalInterval { get; set; } = 5f;
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
		if ( !Enabled ) return;

		var targets = TouchingEntities.OfType<TFPlayer>();
		float healing = HealingPerSecond * Time.Delta;
		foreach (var ply in targets)
		{
			ply.GiveHealth( healing );
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
			timeSinceMetalRegenerate = 0;

			StoredMetal += MetalPerInterval;
			if ( StoredMetal > MaxStoredMetal )
				StoredMetal = MaxStoredMetal;
		}
	}
	void GiveAmmo(TFPlayer ply)
	{
		Sound.FromEntity( To.Single( ply ), AMMO_SOUND, ply );

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

			ply.GiveMetal( usedMetal );
		}
	}

	public override bool PassesTriggerFilters( Entity other )
	{
		// TODO: Disguise check
		return other is TFPlayer ply && ply.Team == Team;
	}
}
