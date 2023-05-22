using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public interface IResettable
{
	/// <summary>
	/// Reset this entity into an initial state.
	/// </summary>
	/// <param name="fullRoundReset">IS the current round entirely over? (ex: For multistage this would be false)</param>
	public void Reset( bool fullRoundReset = true );
}
