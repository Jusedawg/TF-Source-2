using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;
using TFS2;

namespace TFS2.UI
{
	public class PayloadCart : Panel
	{
		public Cart Cart { get; set; }
		public Label Status { get; set; }

		public override void Tick()
		{
			var status = Cart.GetStatus();
			Status.Text = $"x{status.CapRate}";
		}
	}
}
