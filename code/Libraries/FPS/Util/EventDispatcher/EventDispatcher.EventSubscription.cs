using Sandbox;
using Sandbox.UI;

namespace Amper.FPS;

public partial class EventDispatcher
{
	private struct EventSubscription
	{
		EventDelegate eventDelegate;
		object original;
		object target;
		bool verifyTarget;

		public EventSubscription( EventDelegate eventDelegate, object originalDelegate, object target )
		{
			this.eventDelegate = eventDelegate;
			this.original = originalDelegate;

			if ( target != null )
			{
				this.target = target;
				verifyTarget = true;
			}
			else
			{
				this.target = null;
				verifyTarget = false;
			}
		}

		/// <summary>
		/// Check if the given object is equal to the original delegate that was subscribed with.
		/// </summary>
		public bool IsEqualOriginal( object target )
		{
			return original == target;
		}

		/// <summary>
		/// Check if the given object is equal to the target object.
		/// </summary>
		public bool IsEqualTarget( object t )
		{
			return target != null && target == t;
		}

		/// <summary>
		/// Verify if this event is valid to be called based on the target objects state.
		/// </summary>
		/// <returns>If validation passed</returns>
		public bool IsValid()
		{
			//If the original given delegate is null then nothing is valid.
			if ( original == null ) return false;

			if ( !verifyTarget ) return true;

			if ( target != null ) 
			{
				//Check if its an Entity or Panel and perform an extra check for their validity.

				if ( target is Entity targetEntity ) 
				{
					if ( !targetEntity.IsValid ) return false;
				}

				if ( target is Panel panel ) 
				{
					if ( panel.Parent == null ) return false;
				}

				return true;
			}

			return false;
		}

		public void Invoke(DispatchableEventBase arg)
		{
			eventDelegate?.Invoke(arg);
		}
	}

}
