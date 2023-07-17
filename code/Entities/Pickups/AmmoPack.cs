using Sandbox;
using Editor;
using System;
using System.Linq;
using Amper.FPS;

namespace TFS2;

public abstract class AmmoPack : PickupItem, IAcceptsExtendedDamageInfo
{
	private const int BlastScale = 41;

	public virtual float AmmoMultiplier => 1f;

	public override void OnPicked( TFPlayer player )
	{
		if ( !player.GiveAmmo( AmmoMultiplier ) ) 
			return;

		Sound.FromEntity( To.Single( player ), "player.pickupammo", player );
		base.OnPicked( player );
	}

	public void TakeDamage( ExtendedDamageInfo info )
	{
		if ( PhysicsBody.BodyType != PhysicsBodyType.Dynamic || !info.HasTag( TFDamageTags.Blast ) )
			return;

		var vec = (info.HitPosition - info.Inflictor.WorldSpaceBounds.Center).Normal;
		vec *= info.Damage * BlastScale;

		ApplyAbsoluteImpulse( vec );
	}
}

[Library( "tf_ammopack_small" )]
[Title("Small Ammo Pack")]
[Category( "Pickups" )]
[Icon( "backpack" )]
[EditorModel( "models/items/ammopack_small.vmdl" )]
[HammerEntity]
public class AmmoPackSmall : AmmoPack
{
	public override string ModelPath => "models/items/ammopack_small.vmdl";
	public override float AmmoMultiplier => 0.2f;
}

[Library( "tf_ammopack_medium" )]
[Title("Medium Ammo Pack")]
[Category( "Pickups" )]
[Icon( "backpack" )]
[EditorModel( "models/items/ammopack_medium.vmdl" )]
[HammerEntity]
public class AmmoPackMedium : AmmoPack
{
	public override string ModelPath => "models/items/ammopack_medium.vmdl";
	public override float AmmoMultiplier => 0.5f;
}

[Library( "tf_ammopack_full" )]
[Title("Full Ammo Pack")]
[Category( "Pickups" )]
[Icon("backpack")]
[EditorModel( "models/items/ammopack_large.vmdl" )]
[HammerEntity]
public class AmmoPackFull : AmmoPack
{
	public override string ModelPath => "models/items/ammopack_large.vmdl";
	public override float AmmoMultiplier => 1f;
}
