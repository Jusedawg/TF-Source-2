namespace TFS2;

partial class TFPlayer
{
	public override int MaxAirDashes => PlayerClass?.Abilities.AirBorneJumps ?? 0;


	public bool IsImmuneToAirBlasts( TFWeaponBase weapon, TFPlayer attacker )
	{
		// We're never immune to our own airblasts.
		if ( attacker == this )
			return false;

		// TODO: Quickfix is immune to airblasts.
		return false;
	}
}
