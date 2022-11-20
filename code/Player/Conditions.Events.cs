using System.Collections.Generic;
using Sandbox;

namespace TFS2;

partial class TFPlayer
{
	protected delegate void ConditionEventDelegate();

	protected enum ConditionEventType
	{
		Tick,
		Added,
		Removed,
		Changed
	};

	Dictionary<TFCondition, ConditionEventDelegate> ConditionAddedSubscriptions = new();
	Dictionary<TFCondition, ConditionEventDelegate> ConditionRemovedSubscriptions = new();
	Dictionary<TFCondition, ConditionEventDelegate> ConditionChangedSubscriptions = new();
	Dictionary<TFCondition, ConditionEventDelegate> ConditionTickSubscriptions = new();

	public void ClearConditionEvents()
	{
		ConditionTickSubscriptions.Clear();
		ConditionAddedSubscriptions.Clear();
		ConditionRemovedSubscriptions.Clear();
		ConditionChangedSubscriptions.Clear();
	}

	/// <summary>
	/// Adds a callback function to a condition event change.
	/// </summary>
	protected void SubscribeToConditionEvent( TFCondition condition, ConditionEventDelegate onTick = null, ConditionEventDelegate onAdded = null, ConditionEventDelegate onRemoved = null, ConditionEventDelegate onChanged = null )
	{
		SubscribeToConditionTick( condition, onTick );
		SubscribeToConditionAdded( condition, onAdded );
		SubscribeToConditionRemoved( condition, onRemoved );
		SubscribeToConditionChanged( condition, onChanged );
	}

	/// <summary>
	/// Runs the function every tick on BOTH CLIENT AND SERVER while condition is active on the player.
	/// </summary>
	protected void SubscribeToConditionTick( TFCondition condition, ConditionEventDelegate func )
	{
		if ( func == null )
			return;

		ConditionTickSubscriptions[condition] = func;
	}

	/// <summary>
	/// Runs the function on BOTH CLIENT AND SERVER while condition when condition is added to the player.
	/// </summary>
	protected void SubscribeToConditionAdded( TFCondition condition, ConditionEventDelegate func )
	{
		if ( func == null )
			return;

		ConditionAddedSubscriptions[condition] = func;
	}

	/// <summary>
	/// Runs the function on BOTH CLIENT AND SERVER while condition when condition is removed from the player.
	/// </summary>
	protected void SubscribeToConditionRemoved( TFCondition condition, ConditionEventDelegate func )
	{
		if ( func == null )
			return;

		ConditionRemovedSubscriptions[condition] = func;
	}

	/// <summary>
	/// Runs the function on BOTH CLIENT AND SERVER while condition when condition is changed on the player.
	/// </summary>
	protected void SubscribeToConditionChanged( TFCondition condition, ConditionEventDelegate func )
	{
		if ( func == null )
			return;

		ConditionChangedSubscriptions[condition] = func;
	}

	[Event.Hotload]
	public void HotloadConditionEvents()
	{
		ClearConditionEvents();
		SubscribeToConditionEvents();
	}
}
