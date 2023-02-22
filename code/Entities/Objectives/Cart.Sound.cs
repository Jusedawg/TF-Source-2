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
		protected Sound RollingSound;
		protected Sound GrindingSound;

		protected virtual void StartMoveSounds()
		{
			if ( RollingSound.IsPlaying )
				RollingSound.Stop();

			RollingSound = PlaySound( StartMoveSound );
		}

		protected virtual void MoveSounds()
		{
			if ( !RollingSound.IsPlaying )
			{
				//Log.Info( $"MoveSound {MoveSound}" );
				RollingSound = PlaySound( MoveSound );
			}
		}

		protected virtual void StopMoveSounds()
		{
			RollingSound = RollingSound.Stop();
			RollingSound = PlaySound( StopMoveSound );
		}

		/// <summary>
		/// Plays rollback/rollforward sounds (grinding)
		/// </summary>
		protected virtual void RollbackSounds()
		{
			GrindingSound = PlaySound( RollbackSound );
		}
	}
}
