namespace TFS2;

partial class TFPlayer 
{
	public int GetPoints()
	{
		// this is ugly but thats just how the points are defined
		// todo: check if tracking points seperately is a better idea then this mess
		return Kills + Assists + Destructions + Captures
				+ Defenses + Dominations + Revenges + Invulns
				+ Headshots + Teleports + Healing + Backstabs
				+ Bonus + DamageScore;
	}

	public void ResetPoints()
	{
		Kills = 0;
		Deaths = 0;
		Assists = 0;
		Destructions = 0;
		Captures = 0;
		Defenses = 0;
		Dominations = 0;
		Revenges = 0;
		Invulns = 0;
		Headshots = 0;
		Teleports = 0;
		Healing = 0;
		Backstabs = 0;
		Bonus = 0;
		Support = 0;
		DamageScore = 0;
	}

	#region Helper Properties

	public int Kills { get; set; }
	public int Deaths { get; set; }
	public int Assists { get; set; }
	public int Destructions { get; set; }
	public int Captures { get; set; }
	public int Defenses { get; set; }
	public int Dominations { get; set; }
	public int Revenges { get; set; }
	public int Invulns { get; set; }
	public int Headshots { get; set; }
	public int Teleports { get; set; }
	public int Healing { get; set; }
	public int Backstabs { get; set; }
	public int Bonus { get; set; }
	public int Support { get; set; }
	public int DamageScore { get; set; }
	#endregion
}
