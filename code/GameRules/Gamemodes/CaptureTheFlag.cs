using Amper.FPS;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TFS2
{
	public partial class CaptureTheFlag : UniversalGamemode
	{
		public override string Title => "Capture The Flag";

		public override string Icon => IGamemode.DEFAULT_ICON;
		public override GamemodeProperties Properties => default;

		public CaptureTheFlag()
		{
			EventDispatcher.Subscribe<FlagCapturedEvent>( FlagCaptured, this );
			EventDispatcher.Subscribe<RoundRestartEvent>( Reset, this );
		}

		private void Reset(RoundRestartEvent args)
		{
			FlagCaptures.Clear();
		}

		public override bool HasWon( out TFTeam team, out TFWinReason reason  )
		{
			// if there is a team that has flag count > limit
			team = TFTeam.Unassigned;
			reason = TFWinReason.FlagCaptureLimit;

			foreach ( var ( key, captures ) in FlagCaptures )
			{
				team =  key;

				if ( captures >= tf_flag_caps_per_round )
				{
					return true;
				}
			}

			return false;
		}

		public override bool IsActive() => Entity.All.OfType<Flag>().Any();

		[Net] public IDictionary<TFTeam, int> FlagCaptures { get; set; }

		public virtual void FlagReturned( Flag flag )
		{
			TFGameRules.SendHUDAlertToTeam( flag.Team, "Your INTELLIGENCE was RETURNED!", "/ui/icons/ico_flag_home.png", 5, flag.Team );
			TFGameRules.PlaySoundToTeam( flag.Team, "announcer.intel.teamreturned", SoundBroadcastChannel.Announcer );

			foreach ( TFTeam team in Enum.GetValues( typeof( TFTeam ) ) )
			{
				if ( !team.IsPlayable() ) continue;
				if ( team == flag.Team ) continue;

				TFGameRules.SendHUDAlertToTeam( team, "The ENEMY INTELLIGENCE was RETURNED!", "/ui/icons/ico_flag_home.png", 5, flag.Team );
				TFGameRules.PlaySoundToTeam( team, "announcer.intel.enemyreturned", SoundBroadcastChannel.Announcer );
			}
		}

		public virtual void FlagPickedUp( Flag flag, TFPlayer picker )
		{
			TFGameRules.SendHUDAlertToTeam( picker.Team, "Your team has PICKED the enemy INTELLIGENCE!", "/ui/icons/ico_flag_moving.png", 5, flag.Team );
			TFGameRules.PlaySoundToTeam( picker.Team, "announcer.intel.teamstolen", SoundBroadcastChannel.Announcer );

			TFGameRules.SendHUDAlertToTeam( flag.Team, "Your INTELLIGENCE has been PICKED UP!", "/ui/icons/ico_flag_moving.png", 5, flag.Team );
			TFGameRules.PlaySoundToTeam( flag.Team, "announcer.intel.enemystolen", SoundBroadcastChannel.Announcer );
		}

		public virtual void FlagDropped( Flag flag, TFPlayer dropper )
		{
			TFGameRules.SendHUDAlertToTeam( dropper.Team, "Your team DROPPED the enemy INTELLIGENCE!", "/ui/icons/ico_flag_dropped.png", 5, flag.Team );
			TFGameRules.PlaySoundToTeam( dropper.Team, "announcer.intel.teamdropped", SoundBroadcastChannel.Announcer );

			TFGameRules.SendHUDAlertToTeam( flag.Team, "Your INTELLIGENCE has been DROPPED!", "/ui/icons/ico_flag_dropped.png", 5, flag.Team );
			TFGameRules.PlaySoundToTeam( flag.Team, "announcer.intel.enemydropped", SoundBroadcastChannel.Announcer );
		}

		public virtual void FlagCaptured( FlagCapturedEvent args )
		{
			Flag flag = args.Flag;
			TFTeam team = flag.Team.GetEnemy();

			TFGameRules.SendHUDAlertToTeam( team, "Your team CAPTURED the ENEMY INTELLIGENCE!", "/ui/icons/ico_flag_home.png", 5, flag.Team );
			TFGameRules.PlaySoundToTeam( team, "announcer.intel.teamcaptured", SoundBroadcastChannel.Announcer );

			TFGameRules.SendHUDAlertToTeam( flag.Team, "Your INTELLIGENCE was CAPTURED!", "/ui/icons/ico_flag_home.png", 5, flag.Team );
			TFGameRules.PlaySoundToTeam( flag.Team, "announcer.intel.enemycaptured", SoundBroadcastChannel.Announcer );

			if ( !FlagCaptures.ContainsKey( team ) ) FlagCaptures[team] = 0;
			FlagCaptures[team]++;
		}

		[ConVar.Replicated] public static int tf_flag_caps_per_round { get; set; } = 3;
	}
}
