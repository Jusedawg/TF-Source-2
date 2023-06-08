using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TFS2.Menu;

public partial class ClassLoadout : MenuOverlay
{
	PlayerClass PlayerClass { get; set; }
	List<ItemPicker> Slots { get; set; } = new();

	public ClassLoadout()
	{
		if(Game.InGame)
		{
			PlayerClass = Game.LocalClient.GetPlayerClass();
		}
		else
		{
			PlayerClass = PlayerClass.All.Values.First();
		}
	}

	public ClassLoadout( PlayerClass playerClass )
	{
		PlayerClass = playerClass;
	}

	public WeaponData GetWeaponForSlot(TFWeaponSlot slot)
	{
		var wpn = Loadout.LocalLoadout.GetLoadoutItem( PlayerClass, slot );
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
