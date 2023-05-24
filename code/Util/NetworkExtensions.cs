using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TFS2;

public static class NetworkExtensions
{
	public static Vector3 ReadVector3(this NetRead read)
	{
		return (Vector3)read.ReadObject();
	}

	public static void WriteVector3(this NetWrite write, Vector3 value)
	{
		write.WriteObject( value );
	}

	public static T ReadEnum<T>(this NetRead read) where T : struct, Enum 
	{
		return (T)(object)read.Read<int>();
	}

	public static void WriteEnum<T>(this NetWrite write, T value) where T : struct, Enum
	{
		write.Write( (int)(object)value );
	}
}
