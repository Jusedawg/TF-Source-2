using Sandbox;
using Amper.FPS;

namespace TFS2;

partial class TFViewModel : SDKViewModel
{
	ModelEntity Attachment;

	public override void PlaceViewmodel()
	{
		base.PlaceViewmodel();
		Camera.Main.SetViewModelCamera( TFClientSettings.Current.ViewmodelFov );
	}

	public override void SetWeaponModel( string viewmodel, SDKWeapon entity )
	{
		ClearWeapon( entity );

		var weapon = entity as TFWeaponBase;
		if ( !weapon.IsValid() )
			return;

		Weapon = weapon;

		var handsModel = GetPlayerHandsModel();
		if ( weapon.AttachToHands )
		{
			SetModel( handsModel );
			SetAnimParameter( "weapon", (int)weapon.HoldPose );
			Attachment = CreateAttachment( viewmodel );
		}
		else
		{
			SetModel( viewmodel );
			Attachment = CreateAttachment( handsModel );
		}

		var matGroup = (int)weapon.Team - 2;
		SetMaterialGroup( matGroup );
		Attachment?.SetMaterialGroup( matGroup );
	}

	public string GetPlayerHandsModel()
	{
		var player = Player as TFPlayer;
		if ( !player.IsValid() )
			return "";

		return player.PlayerClass.Hands;
	}

	public override bool ShouldDraw()
	{
		var player = Player as TFPlayer;
		if ( !player.IsValid() )
			return false;

		var activeWeapon = player.ActiveWeapon;
		if ( (activeWeapon as SniperRifle)?.IsZoomed ?? false ) 
			return false;

		return true;
	}

	public override void CalculateView()
	{
		CalculateViewBob( );
		AddViewModelBobHelper();
	}
}
