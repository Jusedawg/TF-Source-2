using System.Collections.Generic;
using Sandbox.UI;

namespace TFS2.UI
{
	[UseTemplate]
	public class PayloadPath : Panel
	{
		public CartPath Path { get; set; }
		protected Dictionary<CartPath.Section, Panel> section;
		Dictionary<Cart, PayloadCart> carts { get; set; } = new();
		Dictionary<ControlPoint, Panel> points { get; set; } = new();

		protected override void PostTemplateApplied()
		{
			var sectionData = Path.GetSections();
			foreach ( var s in sectionData )
			{
				string modeClass = "";
				switch ( s.Mode )
				{
					case PathNodeMode.RollBack:
						modeClass = "rollback";
						break;
					case PathNodeMode.RollForward:
						modeClass = "rollforward";
						break;
				}

				var panel = Add.Panel( modeClass );
				panel.Style.Width = Length.Percent( s.Distance * 100 );
				section.Add( s, panel );
			}

			foreach ( var cp in Path.GetControlPoints() )
			{
				AddPoint( cp.Point );
			}
		}

		public void AddCart( Cart cart )
		{
			carts.Add( cart, new()
			{
				Parent = this,
				Cart = cart
			} );
		}

		public void AddPoint( ControlPoint point )
		{
			points.Add( point, Add.Panel( "point" ) );
		}
	}
}
