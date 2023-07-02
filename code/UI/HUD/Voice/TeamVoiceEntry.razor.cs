using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2.UI;

public partial class TeamVoiceEntry : Panel
{
	public IClient Client { get; set; }
	float TargetVoiceLevel;
	float VoiceLevel;
	RealTimeSince timeSincePlayed;
	Panel DeadIndicator;
	public void Update(float level)
	{
		TargetVoiceLevel = level;
		Log.Info( level );

		timeSincePlayed = 0;
	}

	public override void Tick()
	{
		base.Tick();

		if ( IsDeleting || Client == default )
			return;

		const float VOICE_TIMEOUT = 2.0f;
		var timeoutInv = 1 - (timeSincePlayed / VOICE_TIMEOUT);
		timeoutInv = MathF.Min( timeoutInv * 2.0f, 1.0f );

		if ( timeoutInv <= 0 )
		{
			Delete();
			return;
		}

		var team = Client.GetTeam();
		SetClass( "blu", team == TFTeam.Blue );
		SetClass( "red", team == TFTeam.Red );

		bool isDead = (Client.Pawn is TFPlayer ply) && !ply.IsAlive;
		DeadIndicator.SetClass( "visible", isDead );

		VoiceLevel = VoiceLevel.LerpTo( TargetVoiceLevel, Time.Delta * 40.0f );
		Style.Left = VoiceLevel * -32.0f * timeoutInv;
	}
}
