using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2.UI;

public interface IKillfeedName
{
	string Name { get; }
}
public interface IKillfeedIcon
{
	string GetIcon( bool isCrit, string[] tags );
}
