using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace TFS2.UI;

[UseTemplate]
partial class ClassLoadout : MenuOverlay
{
	PlayerClass PlayerClass { get; set; }
	Label ClassName { get; set; }
	Panel WeaponSlots { get; set; }
	List<LoadoutSlot> Slots { get; set; } = new();
	Label PlayerName { get; set; }
	Image PlayerAvatar { get; set; }

	public ClassLoadout()
	{
		PlayerClass = Local.Client.GetPlayerClass();
		SetupPage();
	}

	public ClassLoadout( PlayerClass playerClass )
	{
		PlayerClass = playerClass;
		SetupPage();
	}

	public override void Tick()
	{
		if ( !IsVisible ) return;

		PlayerName.Text = Local.Client.Name;
		PlayerAvatar.SetTexture( $"avatarbig:{Local.Client.PlayerId}" );
	}

	public async void SetupPage()
	{
		if ( PlayerClass == null )
			return;

		ClassName.Text = PlayerClass.Title;
		WeaponSlots.DeleteChildren();
		Slots.Clear();

		var loadout = Loadout.LocalLoadout;

		foreach ( TFWeaponSlot slot in Enum.GetValues( typeof( TFWeaponSlot ) ) )
		{
			// Don't show loadout slots past PDA2.
			// TODO: Make this better next time we're touching weapons.
			if ( slot >= TFWeaponSlot.PDA2 )
				break;

			var weapon = await loadout.GetLoadoutItem( PlayerClass, slot );
			if ( weapon == null )
				return;

			AddLoadoutSlot( WeaponSlots, slot, weapon );
		}
	}

	public void AddLoadoutSlot( Panel parent, TFWeaponSlot slot, WeaponData weapon )
	{
		var panel = new LoadoutSlot
		{
			Slot = slot,
			PlayerClass = PlayerClass,
			Parent = parent
		};

		Slots.Add( panel );
		panel.SetWeaponData( weapon );
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();
		SetupPage();
	}

	public override void OnHotloaded()
	{
		base.OnHotloaded();
	}

	public void OnClickBack()
	{
		Close();
	}
}
