using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2
{
	public interface IGamemode
	{
		public string Title { get; }
		public string Icon { get; }
		public bool IsActive();
	}
}
