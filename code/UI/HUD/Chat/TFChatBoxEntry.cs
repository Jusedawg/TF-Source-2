using Amper.FPS;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2.UI
{
	public partial class TFChatBoxEntry : Panel
	{
		public TimeSince TimeSinceCreated { get; set; }
		ColorFormattedString Text { get; set; }
		bool IsFaded => TimeSinceCreated > (TFClientSettings.Current.SayTextTime + TFClientSettings.Current.SayTextFadeTime);
		bool IsFading => !IsFaded && TimeSinceCreated > TFClientSettings.Current.SayTextTime;

		public TFChatBoxEntry( ColorFormattedString text )
		{
			TimeSinceCreated = 0;
			Text = text;
			
		}
		protected override void OnAfterTreeRender( bool firstTime )
		{
			Wrapper.AddChild( Text );
		}
		public override void Tick()
		{
			base.Tick();

			Wrapper.Style.Opacity = GetOpacity();
		}

		public float GetOpacity()
		{
			if ( TFChatBox.Instance.IsOpen )
				return 1;

			if ( IsFaded )
				return 0;

			if ( IsFading )
			{
				float lerp = (TimeSinceCreated - TFClientSettings.Current.SayTextTime) / TFClientSettings.Current.SayTextFadeTime;
				return lerp.RemapClamped( 0, 1, 1, 0 );
			}

			return 1;
		}
	}
}
