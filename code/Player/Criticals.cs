using Amper.FPS;
using Sandbox;

namespace TFS2;

partial class TFPlayer
{
	private Sound CritBoostSoundLoop { get; set; }
	private Particles CritBoostEffect { get; set; }

	public bool IsCritBoosted => InCondition( TFCondition.CritBoosted );
}
