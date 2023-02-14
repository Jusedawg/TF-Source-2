using Breaker;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker.Commands
{
	public class TimeSpanParser : ICommandParser<TimeSpan>
	{
		public TimeSpan Parse( IClient caller, string input )
		{
			// Try our more user-friendly format first
			if(input.EndsWith('d'))
				return TimeSpan.FromDays( double.Parse( input.TrimEnd('d') ) );

			if(input.EndsWith('h'))
				return TimeSpan.FromHours( double.Parse( input.TrimEnd('h') ) );

			if(input.EndsWith('m'))
				return TimeSpan.FromMinutes( double.Parse( input.TrimEnd('m') ) );

			// Then resort to the builtin parser
			return TimeSpan.Parse( input );
		}

		object ICommandParser.Parse( IClient caller, string input ) => Parse( caller, input );
	}
}
