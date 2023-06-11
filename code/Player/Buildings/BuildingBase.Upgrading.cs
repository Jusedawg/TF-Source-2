using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public abstract partial class TFBuilding
{
	[Net] public bool IsUpgrading { get; set; }
	[Net] public float UpgradeProgress { get; set; }
	[Net] public float UpgradeTime { get; set; }
	protected bool UpgradeCompleted => UpgradeProgress >= UpgradeTime;
	public virtual void StartUpgrade(int level, float time = default, bool setRequested = false)
	{
		if ( !IsInitialized ) return;

		if(level <= 1)
		{
			Log.Warning( $"Cant upgrade to level {level}, should be constructed instead!" );
			return;
		}

		if ( level > MaxLevel ) level = MaxLevel;

		IsUpgrading = true;
		UpgradeProgress = 0;
		UpgradeTime = time > 0 ? time : Data.UpgradeTime;
		SetLevel( level );

		var levelData = GetLevelData();
		InitializeModel( levelData.DeployModel );
		if(setRequested)
		{
			RequestedLevel = level;
			Health += levelData.MaxHealth - Health;
		}
	}

	public virtual void TickUpgrade()
	{
		if(UpgradeCompleted)
		{
			FinishUpgrade();
			return;
		}

		// If we have variable upgrade speeds in the future, change this code
		UpgradeProgress += Time.Delta;
	}

	public virtual void FinishUpgrade()
	{
		IsUpgrading = false;

		var levelData = GetLevelData();
		InitializeModel( levelData.Model );
	}
}
