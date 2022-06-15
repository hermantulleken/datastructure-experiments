using System;
using System.Collections.Generic;

namespace DataStructures.Tiling;

/// <summary>
/// A graph that represents strip tilings.
/// </summary>
public class StripTilings<TTile, TStripEnd> 
	where TStripEnd : class, IStripEnd
	where TTile : ITile
{
	private struct Node
	{
		public Int2 Position;
		public TTile Tile;
		public TStripEnd StripEnd;
	}

	public int Count => forward.Count;
	
	private readonly Graph<IStripEnd, TStripEnd> forward;
	//private readonly Graph<Node, TStripEnd> backward;

	public StripTilings(IEqualityComparer<TStripEnd> comparer)
	{
		forward = new Graph<IStripEnd, TStripEnd>(comparer);
		//backward = new Graph<Node, TStripEnd>(comparer);
	}

	public void Add(TStripEnd vertex1, TStripEnd vertex2/*, Int2 position, TTile tile/*/)
	{
		/*
		var node = new Node
		{
			StripEnd = vertex1,
			Position = position,
			Tile = tile
		};
		*/	
		forward.Add(vertex1, vertex2);
		//backward.Add(vertex2, node);
		
		//
		//Console.WriteLine(vertex1 + " ---> " + vertex2);
	} 

	public bool ContainsForward(TStripEnd vertex) => forward.Contains(vertex);

	/// <summary>
	/// Gets a tiling in this set that ends with the given strip end. 
	/// </summary>
	/// <param name="stripEnd"></param>
	/// <returns></returns>
	public IEnumerable<PositionedTile<TTile>> GetTilingFromBack(TStripEnd stripEnd)
	{
		return Array.Empty<PositionedTile<TTile>>();
	/*
		Console.WriteLine(backward.Count());
		var list = new List<PositionedTile<TTile>>();
		int i = 0;
		while (backward.Contains(stripEnd) && i < 10000)
		{
			//Console.WriteLine(stripEnd);
			
			var node = backward[stripEnd].First(); //we assume for now there is only one path
			list.Add(new PositionedTile<TTile>
			{
				Position = node.Position,
				Tile = node.Tile
			});

			stripEnd = node.StripEnd;

			if(stripEnd.IsStraight) break;
			
			i++;
		}

		return list;/*/
	}
}
