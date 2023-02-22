using Sandbox;

namespace TFS2;

[Library( "tf_weapon_fists" )]
public partial class Fists : TFMeleeBase
{
	[Net, Predicted] public bool IsSecondaryAttack { get; set; }

	public override bool CanSecondaryAttack()
	{
		return CanPrimaryAttack();
	}

	public override void SecondaryAttack()
	{
		base.SecondaryAttack();

		IsSecondaryAttack = true;
		PrimaryAttack();
		IsSecondaryAttack = false;

		CalculateNextAttackTime();
	}

	public override void SendAnimParametersOnAttack()
	{
		if ( IsCurrentAttackCritical )
		{
			SendAnimParameter( "b_fire_critical" );
			return;
		}

		SendAnimParameter( IsSecondaryAttack ? "b_fire_secondary" : "b_fire" );
	}
}
