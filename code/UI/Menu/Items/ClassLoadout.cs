using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using static Sandbox.Clothing;

namespace TFS2.UI;

public partial class ClassLoadout : MenuOverlay
{
	PlayerClass PlayerClass { get; set; }
	List<ItemPicker> Slots { get; set; } = new();

	public ClassLoadout()
	{
		PlayerClass = Sandbox.Game.LocalClient.GetPlayerClass();
	}

	public ClassLoadout( PlayerClass playerClass )
	{
		PlayerClass = playerClass;
	}

	public WeaponData GetWeaponForSlot(TFWeaponSlot slot)
	{
		var wpn = Loadout.LocalLoadout.GetLoadoutItemAsync( PlayerClass, slot ).Result;
		if ( wpn != null )
			return wpn;
		else
			return PlayerClass.GetDefaultWeaponForSlot( slot );
	}

	public void OnClickBack()
	{
		Close();
	}
}
