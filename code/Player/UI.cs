using Sandbox;
using System.Linq;
using TFS2.UI;

namespace TFS2;

partial class TFPlayer : ITargetID, ITargetIDSubtext, IKillfeedName
{
	private string UIName => Client.Name;
	string IKillfeedName.Name => UIName;
	string ITargetID.Name => UIName;
	string ITargetID.Avatar => $"avatar:{Client.SteamId}";

	string ITargetIDSubtext.Subtext { 
		get {
			foreach(var wpn in Weapons)
			{
				if ( wpn is Medigun medigun )
					return $"ÜberCharge: {MathX.Floor(medigun.ChargeLevel * 100)}%";
;			}

			return "";
		}
	}
}
