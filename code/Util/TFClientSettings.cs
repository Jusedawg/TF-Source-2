using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Amper.FPS;

namespace TFS2;

internal class TFClientSettings
{
	protected static List<string> GroupOrder = new List<string>()
	{
		GROUP_SOCIAL,
		GROUP_COMBAT,
		GROUP_SOUND,
		GROUP_CLASS,
		GROUP_OTHER
	};

	public const string GROUP_SOCIAL = "#GameSettings.Group.Social";
	public const string GROUP_COMBAT = "#GameSettings.Group.Combat";
	public const string GROUP_CLASS = "#GameSettings.Group.Class";
	public const string GROUP_SOUND = "#GameSettings.Group.Sound";
	public const string GROUP_OTHER = "#GameSettings.Group.Misc";
	const string COOKIE_NAME = "tfs2.clientsettings";

	[Display( Name = "#GameSettings.Setting.ShowTextChat", Description = "#GameSettings.Description.ShowTextChat", GroupName = GROUP_SOCIAL )]
	public bool ShowTextChat { get; set; } = true;

	[Display( Name = "#GameSettings.Setting.AutoReload", Description = "#GameSettings.Description.AutoReload", GroupName = GROUP_COMBAT )]
	public bool AutoReload { get; set; } = true;

	[Display( Name = "#GameSettings.Setting.FastWeaponSwitch", Description = "#GameSettings.Description.FastWeaponSwitch", GroupName = GROUP_COMBAT )]
	public bool FastWeaponSwitch { get; set; } = true;

	[Display( Name = "#GameSettings.Setting.MedigunAutoHeal", Description = "#GameSettings.Description.MedigunAutoHeal", GroupName = GROUP_CLASS )]
	public bool MedigunAutoHeal { get; set; } = true;

	[Display( Name = "#GameSettings.Setting.AutoZoomIn", Description = "#GameSettings.Description.AutoZoomIn", GroupName = GROUP_CLASS )]
	public bool AutoZoomIn { get; set; } = true;

	[Display( Name = "#GameSettings.Setting.MenuMusicVolume", Description = "#GameSettings.Destription.MenuMusicVolume", GroupName = GROUP_SOUND )]
	[MinMax( 0f, 1f )]
	public float MenuMusicVolume { get; set; } = 0.2f;
	[Display( Name = "#GameSettings.Setting.LastHitSoundVolume", Description = "#GameSettings.Description.LastHitSoundVolume", GroupName = GROUP_SOUND )]
	[MinMax( 1f, 10f )]
	public float LastHitSoundVolume { get; set; } = 1f;

	[Display( Name = "#GameSettings.Setting.HitSoundVolume", Description = "#GameSettings.Description.HitSoundVolume", GroupName = GROUP_SOUND )]
	[MinMax( 1f, 10f )]
	public float HitSoundVolume { get; set; } = 1f;

	[Display( Name = "#GameSettings.Setting.PlayHitSound", Description = "#GameSettings.Description.PlayHitSound", GroupName = GROUP_SOUND )]
	public bool PlayHitSound { get; set; } = true;

	[Display( Name = "#GameSettings.Setting.PlayLastHitSound", Description = "#GameSettings.Description.PlayLastHitSound", GroupName = GROUP_SOUND )]
	public bool PlayLastHitSound { get; set; } = true;

	[Display( Name = "#GameSettings.Setting.ViewmodelFov", Description = "#GameSettings.Description.ViewmodelFov", GroupName = GROUP_COMBAT )]
	[MinMax( 70, 95 )]
	public int ViewmodelFov { get; set; } = 70;

	[Display( Name = "#GameSettings.Setting.SayTextTime", Description = "#GameSettings.Description.SayTextTime", GroupName = GROUP_SOCIAL )]
	[MinMax( 15, 45 )]
	public int SayTextTime { get; set; } = 15;

	[Display( Name = "#GameSettings.Setting.SayTextFadeTime", Description = "#GameSettings.Description.SayTextFadeTime", GroupName = GROUP_SOCIAL )]
	[MinMax( 1f, 5f )]
	public float SayTextFadeTime { get; set; } = 1f;

	/*[Display( Name = "#GameSettings.Setting.GenericVolume", Description = "#GameSettings.Description.GenericVolume", GroupName = SoundGroup )]
	public float GenericVolume { get; set; } = 1f;

	[Display( Name = "#GameSettings.Setting.AmbienceVolume", Description = "#GameSettings.Description.AmbienceVolume", GroupName = SoundGroup )]
	public float AmbienceVolume { get; set; } = 1f;

	[Display( Name = "#GameSettings.Setting.SoundtrackVolume", Description = "#GameSettings.Description.SoundtrackVolume", GroupName = SoundGroup )]
	public float SoundtrackVolume { get; set; } = 1f;

	[Display( Name = "#GameSettings.Setting.AnnouncerVolume", Description = "#GameSettings.Description.AnnouncerVolume", GroupName = SoundGroup )]
	public float AnnouncerVolume { get; set; } = 1f; */

	public static int GetGroupOrder( string group )
	{
		int index = GroupOrder.FindIndex( x => x == group );
		if ( index < 0 ) index = int.MaxValue;
		return index;
	}

	private static TFClientSettings current;
	public static TFClientSettings Current
	{
		get
		{
			if ( current == null )
			{
				current = Cookie.Get<TFClientSettings>( COOKIE_NAME, new() );
			}

			return current;
		}
	}

	public void Save() => Cookie.Set( COOKIE_NAME, this );

	public static void Reset()
	{
		current = new();
		current.Save();
	}
}
