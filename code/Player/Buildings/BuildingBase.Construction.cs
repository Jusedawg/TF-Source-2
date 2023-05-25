using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace TFS2;
public partial class TFBuilding
{
	/// <summary>
	/// Have we completed our first construction?
	/// </summary>
	[Net] public bool HasFirstConstructed { get; set; } = false;
	/// <summary>
	/// Has this building been constructed fully?
	/// </summary>
	[Net] public bool HasConstructed { get; protected set; } = false;
	[Net] public bool IsConstructing { get; protected set; }
	/// <summary>
	/// How many seconds of progress has been applied.
	/// This is NOT TimeSince to allow for construction boosting.
	/// </summary>
	[Net] public float ConstructionProgress { get; protected set; }
	[Net] public float ConstructionTime { get; protected set; }
	protected bool ConstructionCompleted => ConstructionProgress >= ConstructionTime;
	protected float healthToGain;
	/// <summary>
	/// Get the completion of this buildings construction
	/// </summary>
	/// <returns>Construction Completion in Percent (0..1)</returns>
	public float GetConstructionCompletion()
	{
		return MathF.Min( ConstructionProgress / ConstructionTime, 1 );
	}

	public float GetConstructionRate()
	{
		// TODO: Redeploy
		// TODO: Wrench boosting
		return 1;
	}

	public virtual void StartConstruction(float time = default)
	{
		if ( !IsInitialized ) return;

		IsConstructing = true;
		ConstructionProgress = 0;
		SetLevel( 1 );
		ConstructionTime = time > 0 ? time : Data.BuildTime;

		var levelData = GetLevelData();
		InitializeModel( levelData.DeployModel );
		healthToGain = levelData.MaxHealth - Health;
	}

	/// <summary>
	/// Called while the building is being initially constructed
	/// </summary>
	public virtual void TickConstruction()
	{
		if( ConstructionCompleted )
		{
			FinishConstruction();
			return;
		}

		var scale = GetConstructionRate();
		float gain = Time.Delta * scale;
		ConstructionProgress += gain;
		SetAnimParameter( "f_speed", scale );

		float gainFraction = gain / ConstructionTime;
		Health += healthToGain * gainFraction;
	}

	public virtual void FinishConstruction()
	{
		IsConstructing = false;
		HasConstructed = true;
		HasFirstConstructed = false;

		var levelData = GetLevelData();
		InitializeModel( levelData.Model );
	}
}
