using Sandbox;
using System.Linq;

namespace TFS2
{
	public partial class Cart
	{
		protected Sound currentMoveSound;
		protected Sound currentRollingSound;

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
            currentRollingSound.Stop();
        }

		protected virtual void StartMoveSounds()
		{
			if ( currentMoveSound.IsPlaying )
			{
                currentMoveSound.Stop();
            }

            currentMoveSound = PlaySound( StartMoveSound );
		}

		protected virtual void MoveSounds()
		{
			if ( !currentMoveSound.IsPlaying )
			{
                currentMoveSound = PlaySound( MoveSound );
			}
		}

		protected virtual void StopMoveSounds()
		{
            currentMoveSound.Stop();
            currentMoveSound = PlaySound( StopMoveSound );
			currentRollingSound.Stop();
		}

		/// <summary>
		/// Plays rollback/rollforward sounds (grinding)
		/// </summary>
		protected virtual void SetRollingSoundState(bool isRolling)
		{
			if (isRolling && !currentRollingSound.IsPlaying)
			{
				currentRollingSound = PlaySound(RollbackSound);
            }
			else if(!isRolling && currentRollingSound.IsPlaying)
			{
				currentRollingSound.Stop();
            }
		}
	}
}
