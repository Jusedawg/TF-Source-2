using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker.UI
{
	public partial class BreakerMenu : Panel
	{
		public static BreakerMenu Instance { get; private set; }
		public static bool IsShowing => !Instance.HasClass( "hidden" );
		[ClientRpc]
		public static void Show()
		{
			if ( Instance == null )
				new BreakerMenu();

			Instance.SetClass( "hidden", false );
		}
		[ClientRpc]
		public static void Toggle()
		{
			if ( Instance == null )
				return;

			Instance.SetClass( "hidden", !IsShowing );
		}
		[ClientRpc]
		public static void Hide()
		{
			if (Instance == null)
				return;
			
			Instance.SetClass( "hidden", true );
		}

		public BreakerMenu()
		{
			if ( Instance != null )
				return;

			Instance = this;
			SetClass( "hidden", true );
		}
		
	}
}
