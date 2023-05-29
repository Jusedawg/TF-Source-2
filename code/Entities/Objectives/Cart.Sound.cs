using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2
{
	public partial class Cart
	{
		protected Sound currentMoveSound;
		protected Sound currentRollingSound;

		protected virtual void StartMoveSounds()
		{
			if (currentMoveSound.IsPlaying )
                currentMoveSound.Stop();

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
			currentRollingSound = currentRollingSound.Stop();
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
