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

	public const string SocialGroup = "#GameSettings.Social.Group";
	public const string CombatGroup = "#GameSettings.Combat.Group";
	public const string ClassGroup = "#GameSettings.Class.Group";
	public const string SoundGroup = "#GameSettings.Sound.Group";
	public const string OtherGroup = "#GameSettings.Other.Group";

	[Display( Name = "#GameSettings.ShowTextChat", Description = "#GameSettings.ShowTextChat.Description", GroupName = SocialGroup )]
	public bool ShowTextChat { get; set; } = true;

	[Display( Name = "#GameSettings.AutoReload", Description = "#GameSettings.AutoReload.Desc", GroupName = CombatGroup )]
	public bool AutoReload { get; set; } = true;

	[Display( Name = "#GameSettings.FastWeaponSwitch", Description = "#GameSettings.FastWeaponSwitch.Desc", GroupName = CombatGroup )]
	public bool FastWeaponSwitch { get; set; } = true;

	[Display( Name = "#GameSettings.MedigunAutoHeal", Description = "#GameSettings.MedigunAutoHeal.Desc", GroupName = ClassGroup )]
	public bool MedigunAutoHeal { get; set; } = true;

	[Display( Name = "#GameSettings.AutoZoomIn", Description = "#GameSettings.AutoZoomIn.Desc", GroupName = ClassGroup )]
	public bool AutoZoomIn { get; set; } = true;

	[Display( Name = "#GameSettings.LastHitSoundVolume", Description = "#GameSettings.LastHitSoundVolume.Desc", GroupName = SoundGroup )]
	public float LastHitSoundVolume { get; set; } = 1f;

	[Display( Name = "#GameSettings.HitSoundVolume", Description = "#GameSettings.HitSoundVolume.Desc", GroupName = SoundGroup )]
	public float HitSoundVolume { get; set; } = 1f;

	[Display( Name = "#GameSettings.PlayHitSound", Description = "#GameSettings.PlayHitSound.Desc", GroupName = SoundGroup )]
	public bool PlayHitSound { get; set; } = true;

	[Display( Name = "#GameSettings.PlayLastHitSound", Description = "#GameSettings.PlayLastHitSound.Desc", GroupName = SoundGroup )]
	public bool PlayLastHitSound { get; set; } = true;

	[Display( Name = "#GameSettings.ViewmodelFov", Description = "#GameSettings.ViewmodelFov.Desc", GroupName = CombatGroup )]
	public int ViewmodelFov { get; set; } = 70;

	[Display( Name = "#GameSettings.GenericVolume", Description = "#GameSettings.GenericVolume.Desc", GroupName = SoundGroup )]
	public float GenericVolume { get; set; } = 1f;

	[Display( Name = "#GameSettings.AmbienceVolume", Description = "#GameSettings.AmbienceVolume.Desc", GroupName = SoundGroup )]
	public float AmbienceVolume { get; set; } = 1f;

	[Display( Name = "#GameSettings.SoundtrackVolume", Description = "#GameSettings.SoundtrackVolume.Desc", GroupName = SoundGroup )]
	public float SoundtrackVolume { get; set; } = 1f;

	[Display( Name = "#GameSettings.AnnouncerVolume", Description = "#GameSettings.AnnouncerVolume.Desc", GroupName = SoundGroup )]
	public float AnnouncerVolume { get; set; } = 1f;

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
