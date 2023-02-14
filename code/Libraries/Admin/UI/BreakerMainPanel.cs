using Breaker.UI;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker
{
	public partial class BreakerMainPanel : Panel
	{
		public static BreakerMainPanel Instance { get; set; }

		public BreakerMainPanel()
		{
			if ( Instance != null )
				return;

			Debug.Log( $"Creating main panel..." );
			Instance = this;
			Game.RootPanel.AddChild( this );
			
			Style.Width = Length.Percent(100);
			Style.Height = Length.Percent( 100 );

			AddChild<BreakerMenu>();
		}

		//[Event.Client.Frame]
		private static void OnFrame()
		{
			if ( Instance == null )
				new BreakerMainPanel();
		}
	}
}
