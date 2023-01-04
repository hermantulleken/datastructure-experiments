using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DataStructures.SpatialPartitioning.MultiType;

public static class EnumerableExtensions
{
	public static int IndexOf<T>(this List<T> list, T value, IEqualityComparer<T> comparer)
	{
		int index = 0;

		foreach (var item in list)
		{
			if (comparer.Equals(item, value))
			{
				return index;
			}

			index++;
		}
		
		return -1;
	}
}
/// <summary>
/// A data structure similar to a ND-array, but one that can be indexed with keys from arbitrary types.
/// </summary>
/// <remarks>
/// A table cannot expand once created, and the structure of it's "rows" cannot change.
/// </remarks>
public interface ITable
{
	public bool IsReadOnly { get; }
	public int Count { get; }

	public int CountAlong(int dimension);
}

public interface IReadonlyTable<TKey1, out TValue> : ITable
{
	public TValue this[TKey1 key1] { get; }
	public IEnumerable<TKey1> Keys { get; }

	public bool ContainsKeys(TKey1 key1);
}

public interface IReadonlyTable<TKey1, TKey2, out TValue> : ITable
{
	public TValue this[TKey1 key1, TKey2 key2] { get; }
	
	public IEnumerable<(TKey1, TKey2)> Keys { get; }

	public bool ContainsKeys(TKey1 key1, TKey2 key2);
}

public interface IReadonlyTable<TKey1, TKey2, TKey3, out TValue> : ITable
{
	public TValue this[TKey1 key1, TKey2 key2, TKey3 key3]
	{
		get;
	}
	
	public IEnumerable<(TKey1, TKey2, TKey3)> Keys { get; }

	public bool ContainsKeys(TKey1 key1, TKey2 key2, TKey3 key3);
}

public interface ITable<TKey1, TValue> : ITable
{
	public TValue this[TKey1 key1]
	{
		get;
		set;
	}
	
	public IEnumerable<TKey1> Keys { get; }

	public bool ContainsKeys(TKey1 key1);
}

public interface ITable<TKey1, TKey2, TValue> : ITable
{
	public TValue this[TKey1 key1, TKey2 key2]
	{
		get;
		set;
	}
	
	public IEnumerable<(TKey1, TKey2)> Keys { get; }

	public bool ContainsKeys(TKey1 key1, TKey2 key2);
}

public interface ITable<TKey1, TKey2, TKey3, TValue> : ITable
{
	public TValue this[TKey1 key1, TKey2 key2, TKey3 key3]
	{
		get;
		set;
	}
	
	public IEnumerable<(TKey1, TKey2, TKey3)> Keys { get; }

	public bool ContainsKeys(TKey1 key1, TKey2 key2, TKey3 key3);
}

public class Table<TKey1, TKey2, TKey3, TValue> : ITable<TKey1, TKey2, TKey3, TValue>
{
	private readonly TValue[,,] table;
	
	// ReSharper disable InconsistentNaming
	private readonly List<TKey1> key1s;
	private readonly List<TKey2> key2s;
	private readonly List<TKey3> key3s;
	// ReSharper restore InconsistentNaming

	private readonly IEqualityComparer<TKey1> comparer1;
	private readonly IEqualityComparer<TKey2> comparer2;
	private readonly IEqualityComparer<TKey3> comparer3;

	private readonly int[] counts;
	
	public bool IsReadOnly => false;
	public int Count { get; }

	public int CountAlong(int dimension) => counts[dimension];

	public TValue this[TKey1 key1, TKey2 key2, TKey3 key3]
	{
		get
		{
			(int index1, int index2, int index3) = GetIndex(key1, key2, key3);

			return table[index1, index2, index3];
		}
		
		set
		{
			(int index1, int index2, int index3) = GetIndex(key1, key2, key3);

			table[index1, index2, index3] = value;
		}
	}

	public IEnumerable<(TKey1, TKey2, TKey3)> Keys 
		=> 
			from key1 in key1s 
			from key2 in key2s 
			from key3 in key3s 
			select (key1, key2, key3);

	public Table(
		// ReSharper disable InconsistentNaming
		IEnumerable<TKey1> key1s,
		IEnumerable<TKey2> key2s,
		IEnumerable<TKey3> key3s, 
		// ReSharper restore InconsistentNaming
		IEqualityComparer<TKey1> comparer1 = null,
		IEqualityComparer<TKey2> comparer2 = null,
		IEqualityComparer<TKey3> comparer3 = null)
	{
		this.key1s = key1s.Distinct(comparer1).ToList();
		this.key2s = key2s.Distinct(comparer2).ToList();
		this.key3s = key3s.Distinct(comparer3).ToList();

		this.comparer1 = comparer1;
		this.comparer2 = comparer2;
		this.comparer3 = comparer3;
		
		counts = new[]
		{
			this.key1s.Count, 
			this.key2s.Count, 
			this.key3s.Count
		};

		Count = counts[0] * counts[1] * counts[2];
		table = new TValue[counts[0], counts[1], counts[1]];
	}
	
	public bool ContainsKeys(TKey1 key1, TKey2 key2, TKey3 key3) =>
		ContainsKey1(key1) 
		&& ContainsKey2(key2) 
		&& ContainsKey3(key3);
	
	private (int index1, int index2, int index3) GetIndex(TKey1 key1, TKey2 key2, TKey3 key3)
	{
		ValidateArguments(key1, key2, key3);
		
		int index1 = Index(key1s, key1, comparer1);
		int index2 = Index(key2s, key2, comparer2);
		int index3 = Index(key3s, key3, comparer3);
		
		return (index1, index2, index3);
	}

	private void ValidateArguments(
		TKey1 key1,
		TKey2 key2, 
		TKey3 key3,
		[CallerArgumentExpression("key1")] string key1ArgumentName = null,
		[CallerArgumentExpression("key2")] string key2ArgumentName = null,
		[CallerArgumentExpression("key3")] string key3ArgumentName = null
	)
	{
		if (!ContainsKey1(key1)) throw new ArgumentOutOfRangeException(key1ArgumentName);
		if (!ContainsKey2(key2)) throw new ArgumentOutOfRangeException(key2ArgumentName);
		if (!ContainsKey3(key3)) throw new ArgumentOutOfRangeException(key3ArgumentName);
	}

	private bool ContainsKey1(TKey1 key1) => key1s.Contains(key1, comparer1);
	private bool ContainsKey2(TKey2 key2) => key2s.Contains(key2, comparer2);
	private bool ContainsKey3(TKey3 key3) => key3s.Contains(key3, comparer3);

	private static int Index<TKey>(List<TKey> keys, TKey key, IEqualityComparer<TKey> comparer) => keys.IndexOf(key, comparer);

	//The identity permutation
	private Table<TKey1, TKey2, TKey3, TValue> PermuteKeys() => this;
	
	private Table<TKey2, TKey1, TKey3, TValue> PermuteKeys12()
	{
		var newTable = new Table<TKey2, TKey1, TKey3, TValue>(key2s, key1s, key3s);
		
		foreach (var (key1, key2, key3) in Keys)
		{
			newTable[key2, key1, key3] = this[key1, key2, key3];
		}

		return newTable;
	}
	
	private Table<TKey3, TKey2, TKey1, TValue> PermuteKeys13()
	{
		var newTable = new Table<TKey3, TKey2, TKey1, TValue>(key3s, key2s, key1s);
		
		foreach (var (key1, key2, key3) in Keys)
		{
			newTable[key3, key2, key1] = this[key1, key2, key3];
		}

		return newTable;
	}
	
	private Table<TKey1, TKey3, TKey2, TValue> PermuteKeys23()
	{
		var newTable = new Table<TKey1, TKey3, TKey2, TValue>(key1s, key3s, key2s);
		
		foreach (var (key1, key2, key3) in Keys)
		{
			newTable[key1, key3, key2] = this[key1, key2, key3];
		}

		return newTable;
	}


	private Table<TKey2, TKey3, TKey1, TValue> PermuteKeys123()
	{
		var newTable = new Table<TKey2, TKey3, TKey1, TValue>(key2s, key3s, key1s);
		
		foreach (var (key1, key2, key3) in Keys)
		{
			newTable[key2, key3, key1] = this[key1, key2, key3];
		}

		return newTable;
	}

	private Table<TKey3, TKey1, TKey2, TValue> PermuteKeys132()
		=> PermuteKeys13().PermuteKeys23();
}
