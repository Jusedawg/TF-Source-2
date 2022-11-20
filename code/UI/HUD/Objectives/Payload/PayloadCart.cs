using Sandbox.UI;

namespace TFS2.UI
{
	public class PayloadCart : Panel
	{
		public Cart Cart { get; set; }
		public Label Status { get; set; }

		public override void Tick()
		{
			Status.Text = $"x{Cart.GetStatus().CapRate}";
		}
	}
}
