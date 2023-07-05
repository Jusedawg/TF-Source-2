using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
namespace TFS2;

public partial class TFBuilding : IInteractableTargetID
{
	const string PICKUP_VO = "vo.engineer.building.pickup";
	const string DEPLOY_VO = "vo.engineer.building.deploy";
	public virtual void StartCarrying()
	{
		if ( Game.IsClient ) return;
		if ( !CanCarry(Owner) ) return;

		IsCarried = true;
		EnableAllCollisions = false;
		EnableDrawing = false;
		HasConstructed = false;
		SetLevel( 1 );

		Parent = Owner;
		Owner.PlayResponse( PICKUP_VO );
	}

	public virtual void StopCarrying( Transform deployTransform )
	{
		if ( Game.IsClient ) return;

		IsCarried = false;
		EnableAllCollisions = true;
		EnableDrawing = true;
		Parent = null;

		Transform = deployTransform;
		Owner.PlayResponse( DEPLOY_VO );
	}

	public virtual bool CanCarry(TFPlayer ply)
	{
		return ply?.CanPickupBuildings() == true && !IsConstructing && !IsUpgrading && !IsCarried && InPickupDistance();
	}
	bool IInteractableTargetID.CanInteract( TFPlayer ply ) => CanCarry(ply);

	string IInteractableTargetID.InteractText => "Pickup";
	string IInteractableTargetID.InteractButton => "Attack2";
}
