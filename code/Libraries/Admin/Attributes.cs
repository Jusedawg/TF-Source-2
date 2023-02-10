using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker
{
	public static class Attributes
	{
		public static IEnumerable<MethodDescription> FindMethods<T>() where T : Attribute
		{
			List<MethodDescription> methods = new();
			
			foreach(var method in TypeLibrary.FindStaticMethods<T>())
			{
				methods.Add( method );
			}

			return methods;
		}
	}
}
