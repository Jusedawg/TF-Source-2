using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public class ParticleCollection
{
	public IReadOnlyList<Particles> Items => items;
	public int Count => items.Count;
	List<Particles> items = new();
	bool enableDrawing;
	public bool EnableDrawing
	{
		get
		{
			return enableDrawing;
		}
		set
		{
			foreach ( var p in items )
				p.EnableDrawing = value;
			enableDrawing = value;
		}
	}
	public void Add(Particles p)
	{
		items.Add( p );
	}
	public void Remove(Particles p)
	{
		items.Remove( p );
	}
	public void Destroy()
	{
		foreach(var p in items ) p.Destroy();
	}
}
