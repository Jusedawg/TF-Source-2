using Sandbox;

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
