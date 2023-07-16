using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public class AttackDefend : ControlPoints
{
	public const TFTeam DEFENDING_TEAM = TFTeam.Red;
	public override string Title => "Attack / Defend";
	public override GamemodeProperties Properties => new() { IsAttackDefense = true };
	public override int Priority => 1;
	public override bool IsActive()
	{
		var controlPoints = ControlPoint.All;
		return controlPoints.Any() && controlPoints.All( cp => cp.GetDefaultTeamOwner() == DEFENDING_TEAM );
	}
}
