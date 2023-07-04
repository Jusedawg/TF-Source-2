using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public static class EntityUtils
{
	public const char LINE_SPLIT_CHAR = ' ';
	public const char ELEMENT_SPLIT_CHAR = ',';
	/// <summary>
	/// Gets list of entities by target name. Targets are split with space.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="targetNames"></param>
	/// <returns></returns>
	public static IEnumerable<T> ResolveTargetNames<T>( string targetNames )
	{
		if ( string.IsNullOrWhiteSpace( targetNames ) )
			yield break;

		foreach ( var part in targetNames.Split( LINE_SPLIT_CHAR ) )
		{
			var ent = Entity.FindByName( part.Trim() );
			if ( ent is null )
			{
				Log.Error( new ArgumentException(), $"Unable to find entity with target name {part}" );
				continue;
			}

			if ( ent is not T obj )
			{
				Log.Error( new ArgumentException(), $"Referenced entity {part} is not an an object of type {typeof( T )}" );
				continue;
			}

			yield return obj;
		}
	}

	/// <summary>
	/// Gets list of lists of entities by target name. Groups of target names are split with space, individual targets with comma.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="targetNames"></param>
	/// <returns></returns>
	public static T[][] ResolveTargetNameLists<T>( string targetNames )
	{
		if ( string.IsNullOrWhiteSpace( targetNames ) )
			return default;

		List<T[]> elements = new();
		List<T> currentElements = new();
		foreach ( var list in targetNames.Split( LINE_SPLIT_CHAR ) )
		{
			currentElements.Clear();
			foreach ( var part in list.Split( ELEMENT_SPLIT_CHAR ) )
			{
				var ent = Entity.FindByName( part );
				if ( ent is null )
				{
					Log.Warning( $"Unable to find entity with target name {part}" );
					continue;
				}

				if ( ent is not T obj )
				{
					Log.Warning( $"Referenced entity {part} is not an an object of type {typeof( T )}" );
					continue;
				}

				currentElements.Add( obj );
			}
			elements.Add( currentElements.ToArray() );
			currentElements.Clear();
		}

		return elements.ToArray();
	}

	public static string[][] SplitTargetNameLists(string targetNames)
	{
		if ( string.IsNullOrWhiteSpace( targetNames ) )
			return default;

		List<string[]> elements = new();
		foreach ( var part in targetNames.Split( LINE_SPLIT_CHAR ).Select(e => e.Split( ELEMENT_SPLIT_CHAR ) ) )
		{
			elements.Add( part );
		}

		return elements.ToArray();
	}

	public static bool IsAlive(this Entity ent)
	{
		return ent.LifeState == LifeState.Alive;
	}
}
