using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace TFS2;

[StyleSheet( "/UI/HUD/TargetID/TargetID.scss" )]
public partial class TargetIDHealing : TargetID
{
	public static TargetIDHealing Current;
	string Prepend;

	public TargetIDHealing()
	{
		Current = this;
	}

	public override ITargetID FindTarget()
	{
		var player = TFPlayer.LocalPlayer;
		if ( !player.IsValid() )
			return null;

		// Check if we're healing someone.
		var medigun = player.ActiveWeapon as Medigun;
		if ( medigun.IsValid() )
		{
			if ( medigun.Patient is ITargetID healingTarget )
			{
				Prepend = "Healing:";
				return healingTarget;
			}
		}

		// Check if we are healed by any medic
		var healingMediguns = player.Healers.Keys.OfType<Medigun>();

		// Getting medigun that has the most charge
		var medigunWithMostCharge = healingMediguns.OrderByDescending( x => x.ChargeLevel ).FirstOrDefault();
		if ( medigunWithMostCharge.IsValid() )
		{
			if ( medigunWithMostCharge.Owner is ITargetID healingMedic )
			{
				Prepend = "Healed by:";
				return healingMedic;
			}
		}

		return null;
	}

	public override bool UpdatePretext( Label label )
	{
		if ( string.IsNullOrEmpty( Prepend ) )
			return false;

		label.Text = Prepend;
		return true;
	}
}
