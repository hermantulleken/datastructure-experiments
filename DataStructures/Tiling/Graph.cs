using System.Collections.Generic;

namespace DataStructures.Tiling;

public class Graph<T, TStripEnd>
{
	private readonly Dictionary<TStripEnd, IList<T>> graphEdges;
	
	public IList<T> this[TStripEnd stripEnd] => graphEdges[stripEnd];

	public Graph(IEqualityComparer<TStripEnd> comparer)
	{
		graphEdges = new Dictionary<TStripEnd, IList<T>>(comparer);
	}
	
	public void Add(TStripEnd vertex1, T vertex2)
	{
		if (!graphEdges.ContainsKey(vertex1))
		{
			graphEdges[vertex1] = new List<T>();
		}

		graphEdges[vertex1].Add(vertex2);
	}

	public bool Contains(TStripEnd vertex) => graphEdges.ContainsKey(vertex);

	public int Count => graphEdges.Count;
}
