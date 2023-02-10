using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker.Commands
{
	public class ClientParser : ICommandParser<IClient>
	{
		public IClient Parse( IClient caller, string input )
		{
			Debug.Log( $"Parsing input {input} with caller {caller}" );
			if ( input.StartsWith( '@' ) )
			{

				input = string.Join( "", input.Skip( 1 ) );
				if ( singleSelectors.TryGetValue( input, out var selector ) )
				{
					Debug.Log( $"Using selector {input}" );
					return selector( caller, input );
				}
			}

			if ( long.TryParse( input, out var id ) )
			{
				return Game.Clients.FirstOrDefault( c => c.SteamId == id );
			}
			else
			{
				return Game.Clients.FirstOrDefault( c => c.Name.Contains( input ) );
			}
		}
		object ICommandParser.Parse( IClient caller, string input ) => Parse( caller, input );

		public class MultiParser : ICommandParser<IEnumerable<IClient>>
		{
			public IEnumerable<IClient> Parse( IClient caller, string input )
			{
				Debug.Log( $"Multi-Parsing input {input} with caller {caller}" );
				if ( input.StartsWith( '@' ) )
				{
					input = string.Join( "", input.Skip( 1 ) );
					if ( multiSelectors.TryGetValue( input, out var selector ) )
					{
						Debug.Log( $"Using selector {input}" );
						return selector( caller, input );
					}

					else if(singleSelectors.TryGetValue(input, out var singleSelector))
					{
						return new IClient[] { singleSelector( caller, input ) };
					}
				}

				return Game.Clients.Where( c => c.Name.Contains( input ) );
			}

			object ICommandParser.Parse( IClient caller, string input ) => Parse( caller, input );
		}

		#region Selectors
		private static Dictionary<string, Func<IClient, string, IClient>> singleSelectors = new()
		{
			{ "me" , SelectSelf },
			{ "self", SelectSelf },
			{ "random", SelectRandom }
		};
		public static bool RegisterSingleSelector( string id, Func<IClient, string, IClient> selector )
		{
			if ( singleSelectors.ContainsKey( id ) )
				return false;

			singleSelectors.Add( id, selector );
			return true;
		}
		private static IClient SelectSelf( IClient caller, string arg ) => caller;
		private static IClient SelectRandom( IClient caller, string arg ) => Game.Clients.OrderBy( cl => Guid.NewGuid() ).FirstOrDefault();

		private static Dictionary<string, Func<IClient, string, IEnumerable<IClient>>> multiSelectors = new()
		{
			{ "all", SelectAll }
		};
		public static bool RegisterMultiSelector( string id, Func<IClient, string, IEnumerable<IClient>> selector )
		{
			if ( multiSelectors.ContainsKey( id ) )
				return false;

			multiSelectors.Add( id, selector );
			return true;
		}
		private static IEnumerable<IClient> SelectAll( IClient caller, string arg ) => Game.Clients;


		#endregion
	}
}
