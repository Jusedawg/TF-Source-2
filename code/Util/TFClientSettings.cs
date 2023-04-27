using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Amper.FPS;

namespace TFS2;

internal class TFClientSettings
{
	protected static List<string> GroupOrder = new List<string>()
	{
		SocialGroup,
		CombatGroup,
		SoundGroup,
		ClassGroup,
		OtherGroup
	};

	public const string SocialGroup = "#GameSettings.Group.Social";
	public const string CombatGroup = "#GameSettings.Group.Combat";
	public const string ClassGroup = "#GameSettings.Group.Class";
	public const string SoundGroup = "#GameSettings.Group.Sound";
	public const string OtherGroup = "#GameSettings.Group.Misc";

	[Display( Name = "#GameSettings.Setting.ShowTextChat", Description = "#GameSettings.Description.ShowTextChat", GroupName = SocialGroup )]
	public bool ShowTextChat { get; set; } = true;

	[Display( Name = "#GameSettings.Setting.AutoReload", Description = "#GameSettings.Description.AutoReload", GroupName = CombatGroup )]
	public bool AutoReload { get; set; } = true;

	[Display( Name = "#GameSettings.Setting.FastWeaponSwitch", Description = "#GameSettings.Description.FastWeaponSwitch", GroupName = CombatGroup )]
	public bool FastWeaponSwitch { get; set; } = true;

	[Display( Name = "#GameSettings.Setting.MedigunAutoHeal", Description = "#GameSettings.Description.MedigunAutoHeal", GroupName = ClassGroup )]
	public bool MedigunAutoHeal { get; set; } = true;

	[Display( Name = "#GameSettings.Setting.AutoZoomIn", Description = "#GameSettings.Description.AutoZoomIn", GroupName = ClassGroup )]
	public bool AutoZoomIn { get; set; } = true;

	[Display( Name = "#GameSettings.Setting.LastHitSoundVolume", Description = "#GameSettings.Description.LastHitSoundVolume", GroupName = SoundGroup )]
	public float LastHitSoundVolume { get; set; } = 1f;

	[Display( Name = "#GameSettings.Setting.HitSoundVolume", Description = "#GameSettings.Description.HitSoundVolume", GroupName = SoundGroup )]
	public float HitSoundVolume { get; set; } = 1f;

	[Display( Name = "#GameSettings.Setting.PlayHitSound", Description = "#GameSettings.Description.PlayHitSound", GroupName = SoundGroup )]
	public bool PlayHitSound { get; set; } = true;

	[Display( Name = "#GameSettings.Setting.PlayLastHitSound", Description = "#GameSettings.Description.PlayLastHitSound", GroupName = SoundGroup )]
	public bool PlayLastHitSound { get; set; } = true;

	[Display( Name = "#GameSettings.Setting.ViewmodelFov", Description = "#GameSettings.Description.ViewmodelFov", GroupName = CombatGroup )]
	public int ViewmodelFov { get; set; } = 70;

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
				current = Cookie.Get<TFClientSettings>( "tfs2.clientsettings", new() );
			return current;
		}
	}

	public void Save() => Cookie.Set( "tfs2.clientsettings", this );

	public static void Reset()
	{
		current = new();
		current.Save();
	}
}
