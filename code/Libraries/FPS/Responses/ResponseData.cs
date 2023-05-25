using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;

namespace Amper.FPS;

public partial class ResponseData<Concepts, Contexts> : GameResource where Concepts : Enum where Contexts : Enum
{
	public virtual string Base { get; set; }
	public Dictionary<string, Criterion> Criteria { get; set; }
	public List<Response> Responses { get; set; }

	public struct Criterion
	{
		public Contexts Context { get; set; }
		public string Value { get; set; }
		public override string ToString()
		{
			return $"{Context} {Value}";
		}
	}

	public struct Response
	{
		public Concepts Concept { get; set; }
		public List<string> Criteria { get; set; }
		[ResourceType( "sound" )] public string SoundEvent { get; set; }

		public override string ToString()
		{
			var name = $"{Concept}";

			if ( Criteria != null && Criteria.Count > 0 ) 
			{
				name += " while ";
				name += string.Join( ", ", Criteria );
			}

			if ( !string.IsNullOrEmpty( SoundEvent ) )
			{
				var soundName = System.IO.Path.GetFileNameWithoutExtension( SoundEvent );
				name += $" ({soundName})";
			}

			return name;
		}
	}
}
