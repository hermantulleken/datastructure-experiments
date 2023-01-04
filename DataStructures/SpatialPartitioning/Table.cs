using System.Collections.Generic;

namespace DataStructures.SpatialPartitioning;

public class Table
{
	
}

public interface ITable
{
	public bool IsReadOnly { get; }
	public int Count { get; }
	public int CountAlong(int dimension);
}

public interface IReadonlyTable3<TKey, out TValue> : ITable
{
	public TValue this[TKey key1, TKey key2, TKey key3]
	{
		get;
	}
	
	public IEnumerable<(TKey, TKey, TKey)> Keys { get; }

	public bool ContainsKeys(TKey key1, TKey key2, TKey key3);
}

public interface ITable3<TKey, TValue> : ITable
{
	public TValue this[TKey key1, TKey key2, TKey key3]
	{
		get;
		set;
	}
	
	public IEnumerable<(TKey, TKey, TKey)> Keys { get; }

	public bool ContainsKeys(TKey key1, TKey key2, TKey key3);
}
