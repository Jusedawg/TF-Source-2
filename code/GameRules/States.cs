using Sandbox;
using System.Linq;
using Amper.FPS;

namespace TFS2;

partial class TFGameRules
{
	[ConVar.Replicated] public static bool mp_tournament_readymode { get; set; } = false;
	public override bool WaitingForPlayersEnabled() => !ReadyUpEnabled();
	public override bool ReadyUpEnabled() => mp_tournament_readymode || GetGamemode()?.Properties.RequireReadyUp == true;
	public override void OnRoundRestart()
	{
		FirstBloodAnnounced = false;

		foreach ( var ply in Entity.All.OfType<TFPlayer>() )
			ply.ResetPoints();
	}

	int LastAnnouncerSeconds { get; set; }
	bool WillPlayGameStartSong { get; set; }
	[Net] public bool HasSetup { get; set; }
	[Net] public bool IsInSetup { get; set; }

	public override void StartedPreGame()
	{
		base.StartedPreGame();

		if ( !Game.IsServer )
			return;

		WillPlayGameStartSong = ShouldPlayGameStartSong();
		LastAnnouncerSeconds = -1;
	}

	public override void SimulatePreGame()
	{
		base.SimulatePreGame();

		if ( !Game.IsServer )
			return;

		if ( WillPlayGameStartSong )
		{
			if ( IsWaitingForPlayers && TimeUntilWaitingForPlayersEnds <= 4.5f )
			{
				PlaySoundToAll( "music.game_start", SoundBroadcastChannel.Soundtrack );
				PlaySoundToAll( "announcer.are_you_ready", SoundBroadcastChannel.Announcer );

				WillPlayGameStartSong = false;
			}
		}
	}

	public override void SimulatePreRound()
	{
		if ( Game.IsServer )
		{
			//
			// Play preround effects
			//

			// don't run this if we're waiting for players.
			if ( !IsWaitingForPlayers && !HasSetup )
			{
				var timeUntilEnd = GetPreRoundFreezeTime() - TimeSinceStateChange;
				var seconds = timeUntilEnd.CeilToInt() - 1;

				if ( seconds != LastAnnouncerSeconds )
				{
					LastAnnouncerSeconds = seconds;

					if ( seconds >= 1 )
					{
						PlaySoundToAll( $"announcer.begins.{seconds}sec.comp", SoundBroadcastChannel.Announcer );
					}

					if ( seconds == -1 )
					{
						PlaySoundToAll( "ambience.siren", SoundBroadcastChannel.Ambience );
						PlaySoundToAll( "announcer.round_start", SoundBroadcastChannel.Announcer );
					}
				}
			}
		}

		base.SimulatePreRound();
	}
}
