using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
namespace TFS2;

public partial class TFBuilding : IInteractableTargetID
{
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
	}

	public virtual void StopCarrying( Transform deployTransform )
	{
		if ( Game.IsClient ) return;

		IsCarried = false;
		EnableAllCollisions = true;
		EnableDrawing = true;
		Parent = null;

		Transform = deployTransform;
	}

	public virtual bool CanCarry(TFPlayer ply)
	{
		return ply == Owner && !IsConstructing && !IsUpgrading && !IsCarried;
	}
	public bool CanInteract( TFPlayer user ) => CanCarry( user );

	string IInteractableTargetID.InteractText => "Pickup";
	string IInteractableTargetID.InteractButton => "Attack2";
}
