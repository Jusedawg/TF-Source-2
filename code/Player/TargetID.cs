using Sandbox;
using System.Linq;

namespace TFS2;

partial class TFPlayer : ITargetID, ITargetIDSubtext
{
	string ITargetID.Name => Client.Name;
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
