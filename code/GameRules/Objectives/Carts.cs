using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2
{
	partial class TFGameRules
	{
		[Net] public bool MapHasCarts { get; set; }

		/// <summary>
		/// Can this cart becaptured?
		/// </summary>
		/// <param name="cart"></param>
		/// <returns></returns>
		public bool CartMayBeCaptured(Cart cart)
		{
			return AreObjectivesActive();
		}
	}
}
