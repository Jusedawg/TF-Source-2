using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2
{
	public partial class Cart
	{
		/// <summary>
		/// The carts status at one point in time, used by the UI
		/// </summary>
		public class Status
		{
			public float PathFraction { get; init; }
			public int CapRate { get; init; }
			public bool IsRollingBack { get; init; }
			public bool IsRollingForward { get; init; }
		}

		public Status GetStatus()
		{
			bool movable = CanMove();
			return new()
			{
				PathFraction = Path.GetFraction( Path.GetNodeDistance( CurrentNode ) + NodeDistance ),
				CapRate = GetCapRate(),
				IsRollingBack = movable && CanRollback(),
				IsRollingForward = movable && CanRollforward()
			};
		}
	}
}
