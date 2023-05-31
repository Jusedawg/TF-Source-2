using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2; 

/// <summary>
/// Apply to an entity to allow it to block timers. Blocked timers go into overtime.
/// </summary>
public interface IRoundTimerBlocker
{
	public bool ShouldBlock();
}
