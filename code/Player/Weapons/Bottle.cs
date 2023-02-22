using Sandbox;

namespace TFS2;

[Library( "tf_weapon_bottle" )]
public class Bottle : TFMeleeBase
{
	public const string BrokenModel = "models/weapons/c_models/c_bottle/c_bottle_broken.vmdl";
	public const string BreakSound = "weapon_bottle.break";

	// TODO: Break model on hit, restore model on regenerate.
}
