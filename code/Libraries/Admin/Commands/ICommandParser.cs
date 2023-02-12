using Sandbox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker
{
	/// <summary>
	/// Parses input into object of specified type
	/// </summary>
	/// <typeparam name="T">Type to parse to object into</typeparam>
	public interface ICommandParser<T> : ICommandParser
	{
		public new T Parse( IClient caller, string input );
	}
	
	/// <summary>
	/// Parses input into some object
	/// </summary>
	public interface ICommandParser
	{
		// This second interface is required to be able to handle these objects with sbox reflection.
		// Related issue: https://github.com/sboxgame/issues/issues/2881
		public object Parse( IClient caller, string input );
	}
}
