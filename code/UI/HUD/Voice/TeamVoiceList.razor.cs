using Amper.FPS;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFS2.UI;

namespace TFS2.UI
{
	public partial class TeamVoiceList : Panel
	{
		public static TeamVoiceList Current { get; private set; }
		protected Dictionary<IClient, TeamVoiceEntry> Entries = new();
		public TeamVoiceList()
		{
			Current = this;
		}

		public void OnVoicePlayed(IClient cl)
		{
			var info = cl.Voice;
			
			if ( !Entries.TryGetValue( cl, out var entry ) )
			{
				entry = new() { Client = cl};
				Entries.Add( cl, entry );
			}

			entry.Update( info.CurrentLevel );
		}
	}
}

namespace TFS2
{
	public partial class TFGameRules
	{
		[ConVar.Replicated]
		public static bool tf_alltalk { get; set; } = false;
		public override bool CanHearPlayerVoice( IClient source, IClient receiver )
		{
			return tf_alltalk || ITeam.IsSame(source, receiver);
		}

		public override void OnVoicePlayed( IClient cl )
		{
			TeamVoiceList.Current.OnVoicePlayed( cl );
		}
	}
}
