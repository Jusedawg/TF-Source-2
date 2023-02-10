using Sandbox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker.Commands
{
	public abstract class SplitListParser<T> : ICommandParser<IEnumerable>
	{
#pragma warning disable SB3000 // Hotloading not supported
		const char SPLIT_CHAR = ';';
#pragma warning restore SB3000 // Hotloading not supported
		public virtual IEnumerable Parse( IClient caller, string input )
		{
			if ( !input.Contains( SPLIT_CHAR ) )
				return new object[] { ParseSingle(caller, input) };

			var sections = input.Split(SPLIT_CHAR);
			var results = new List<T>();
			foreach (var section in sections)
			{
				var result = ParseSingle(caller, section);
				if (result != null)
					results.Add(result);
			}
			return results;
		}

		public abstract T ParseSingle(IClient caller, string inputSection );

		object ICommandParser.Parse( IClient caller, string input ) => Parse( caller, input );
	}

	public sealed class StringListParser : SplitListParser<string>
	{
		public override string ParseSingle(IClient caller, string inputSection)
		{
			return inputSection;
		}
	}

	public sealed class IntListParser : SplitListParser<int>
	{
		public override int ParseSingle(IClient caller, string inputSection)
		{
			if (int.TryParse(inputSection, out var result))
				return result;
			return 0;
		}
	}

	public sealed class FloatListParser : SplitListParser<float>
	{
		public override float ParseSingle(IClient caller, string inputSection)
		{
			if (float.TryParse(inputSection, out var result))
				return result;
			return 0;
		}
	}

	public sealed class LongListParser : SplitListParser<long>
	{
		public override long ParseSingle(IClient caller, string inputSection)
		{
			if (long.TryParse(inputSection, out var result))
				return result;
			return 0;
		}
	}

	public sealed class BoolListParser : SplitListParser<bool>
	{
		public override bool ParseSingle(IClient caller, string inputSection)
		{
			if (bool.TryParse(inputSection, out var result))
				return result;
			return false;
		}
	}
}
