using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2.UI;

public partial class PayloadDisplay : Panel
{
	Dictionary<Cart, PayloadPath> pathEntries = new();
	public override void Tick()
	{
		SetClass( "visible", ShouldDraw() );

		if ( !IsVisible ) return;

		var carts = Cart.All;
		var localCarts = pathEntries.Keys;

		foreach ( var item in carts.Except( localCarts ) ) AddCart( item );
		foreach ( var item in localCarts.Except( carts ) ) RemoveCart( item );
	}


	private void AddCart( Cart cart )
	{
		pathEntries.Add( 
			cart, 
			new() { 
				Cart = cart, 
				Parent = this
			} 
		);
	}
	private void RemoveCart( Cart cart )
	{
		if(pathEntries.TryGetValue(cart, out var panel))
		{
			panel.Delete();
			pathEntries.Remove( cart );
		}
	}

	public bool ShouldDraw() => TFGameRules.Current.IsPlaying<Payload>();
}
