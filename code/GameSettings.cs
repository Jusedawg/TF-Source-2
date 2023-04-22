﻿using TFS2.UI;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TFS2;

internal class ClientSettings
{
	private static List<string> GroupOrder = new List<string>()
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

	[Display( Name = "#GameSettings.ViewmodelFov", Description = "#GameSettings.ViewmodelFov.Desc", GroupName = CombatGroup )]
	public float ViewmodelFov { get; set; } = 70f;

	[Display( Name = "#GameSettings.AutoReload", Description = "#GameSettings.AutoReload.Desc", GroupName = CombatGroup )]
	public bool AutoReload { get; set; } = true;

	[Display( Name = "#GameSettings.FastWeaponSwitch", Description = "#GameSettings.FastWeaponSwitch.Desc", GroupName = CombatGroup )]
	public bool FastWeaponSwitch { get; set; } = true;

	[Display( Name = "#GameSettings.MedigunAutoHeal", Description = "#GameSettings.MedigunAutoHeal.Desc", GroupName = ClassGroup )]
	public bool MedigunAutoHeal { get; set; } = true;

	[Display( Name = "#GameSettings.AutoZoomIn", Description = "#GameSettings.AutoZoomIn.Desc", GroupName = ClassGroup )]
	public bool AutoZoomIn { get; set; } = true;

	[Display( Name = "#GameSettings.PlayHitSound", Description = "#GameSettings.PlayHitSound.Desc", GroupName = SoundGroup )]
	public bool PlayHitSound { get; set; } = true;

	[Display( Name = "#GameSettings.PlayLastHitSound", Description = "#GameSettings.PlayLastHitSound.Desc", GroupName = SoundGroup )]
	public bool PlayLastHitSound { get; set; } = true;

	private static ClientSettings current;
	public static ClientSettings Current
	{
		get
		{
			if ( current == null )
				current = Cookie.Get<ClientSettings>( "tfs2.clientsettings", new() );
			return current;
		}
	}

	public static int GetGroupOrder( string group )
	{
		int index = GroupOrder.FindIndex( x => x == group );
		if ( index < 0 ) index = int.MaxValue;
		return index;
	}

	public void Save() => Cookie.Set( "tfs2.clientsettings", this );

	public static void Reset()
	{
		current = new();
		current.Save();
	}

}
