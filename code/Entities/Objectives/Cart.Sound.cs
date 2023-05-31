using Sandbox;
using System.Linq;

namespace TFS2
{
	public partial class Cart
	{
		protected Sound currentMoveSound;
		private bool currentMoveSoundPlaying;
		protected Sound currentRollingSound;
		private bool currentRollingSoundPlaying;

		[ConCmd.Server("tf_cartsnd")]
		public static void CartSoundInfoCmd()
		{
			Cart currentCart = Entity.All.FirstOrDefault(x => x is Cart) as Cart;
			if(currentCart != null)
			{
                Log.Info($"currentMoveSound: Playing? {currentCart.currentMoveSound.IsPlaying}");
                Log.Info($"currentRollingSound: Playing? {currentCart.currentRollingSound.IsPlaying}");
            }
        }

        protected virtual void StopAllSound()
        {
            currentMoveSound.Stop();
			currentMoveSoundPlaying = false;

            currentRollingSound.Stop();
			currentRollingSoundPlaying = false;
        }

		protected virtual void StartMoveSounds()
		{
			if (currentMoveSoundPlaying)
			{
                currentMoveSound.Stop();
            }

            currentMoveSound = PlaySound( StartMoveSound );
			currentMoveSoundPlaying = true;

        }

		protected virtual void MoveSounds()
		{
			if ( !currentMoveSoundPlaying)
			{
                currentMoveSound = PlaySound( MoveSound );
				currentMoveSoundPlaying = true;
            }
		}

		protected virtual void StopMoveSounds()
		{
            currentMoveSound.Stop();
            currentMoveSound = PlaySound( StopMoveSound );
			currentMoveSoundPlaying = true;

            currentRollingSound.Stop();
			currentRollingSoundPlaying = false;
		}

		/// <summary>
		/// Plays rollback/rollforward sounds (grinding)
		/// </summary>
		protected virtual void SetRollingSoundState(bool isRolling)
		{
			if (isRolling && !currentRollingSoundPlaying)
			{
				currentRollingSound = PlaySound(RollbackSound);
                currentRollingSoundPlaying = true;
            }
			else if(!isRolling && currentRollingSoundPlaying)
			{
				currentRollingSound.Stop();
				currentRollingSoundPlaying = false;
            }
		}
	}
}
