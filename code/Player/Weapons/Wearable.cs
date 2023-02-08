using Amper.FPS;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2
{
	[Library("tf_wearable")]
	public class Wearable : TFWeaponBase, IPassiveChild
	{
		public virtual void PassiveSimulate( IClient client )
		{
			if ( !EnableDrawing )
				EnableDrawing = true;
		}
	}
}
