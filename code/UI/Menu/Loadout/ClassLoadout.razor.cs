using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TFS2.Menu;

public partial class ClassLoadout : MenuOverlay
{
	string ClassName
	{
		set
		{
			PlayerClass = PlayerClass.Get( value );
		}
	}
	PlayerClass PlayerClass { get; set; }
	List<ItemPicker> Slots { get; set; } = new();

	public ClassLoadout()
	{
		if(Game.InGame)
		{
			PlayerClass = Game.LocalClient.GetPlayerClass();
		}
	}

	public ClassLoadout( PlayerClass playerClass )
	{
		PlayerClass = playerClass;
	}

	public ClassLoadout(string name)
	{
		if ( !Enum.TryParse<TFPlayerClass>( name, out var item ) )
			throw new ArgumentException();

		PlayerClass = PlayerClass.Get( item );
		if ( PlayerClass == null )
			throw new ArgumentException();
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
		this.Navigate("/loadout");
	}

	public void OnClickSlot(TFWeaponSlot slot)
	{
		if ( PlayerClass == null ) return;
		this.Navigate( $"/loadout/class/{PlayerClass.ResourceName}/slot/{slot}" );
	}
}
