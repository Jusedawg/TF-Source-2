using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2
{
	public class Payload : ControlPoints
	{
		public override string Title => "Payload";
		public override bool IsActive() => Entity.All.OfType<Cart>().Any();
	}
}
