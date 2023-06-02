using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public interface IFalloffProvider
{
	public bool UseFalloff { get; }
	public bool UseRampup { get; }
}
