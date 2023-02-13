using Sandbox;

namespace TFS2
{
	partial class TFFirstPersonCamera : FirstPersonCamera
	{
		public override void Update()
		{
			base.Update();
			var pawn = Local.Pawn;
			var eyeRot = pawn.EyeRot;
			var fov = 80f;

			//
			// View Punch
			//

			if ( tf_enable_view_punch )
			{
				var punchRot = (pawn as TFPlayer).ActualViewPunchAngle;
				eyeRot = Rotation.From( eyeRot.Pitch() + punchRot.Pitch(), eyeRot.Yaw() + punchRot.Yaw(), eyeRot.Roll() + punchRot.Roll() );
			}

			//
			// Fov Kick
			//

			if ( tf_enable_fov_kick )
			{
				var punchFov = (pawn as TFPlayer).ActualFOVKick;
				fov += punchFov;
			}

			FieldOfView = fov;
			Rotation = eyeRot;
		}

		[ClientVar( "tf_enable_view_punch" )]
		public static bool tf_enable_view_punch { get; set; } = true;

		[ClientVar( "tf_enable_fov_kick" )]
		public static bool tf_enable_fov_kick { get; set; } = true;

		[ClientVar( "tf_enable_view_tilt" )]
		public static bool tf_enable_view_tilt { get; set; } = false;
	}
}
