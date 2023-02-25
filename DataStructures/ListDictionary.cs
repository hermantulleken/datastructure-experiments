using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions;
using JetBrains.Annotations;

namespace DataStructures;

/*
	1. This class used to use a NotNull extension method to validate parameters inline. 
	(for example, other.NotNull(nameof(other)).Any(Contains))
	
	This allowed most members to be implemented with expression bodies. 
	
	However, this made the logic of the expression harder to follow, so I ended up 
	using normal validation.
	
	Once C# 11 comes out the param!! notation can be used. 
	
	2. Apparently ISet<T>.Add(T item) specifies that item is not null,
	but ICollection<T>.Add(T item) 
	
	3. The private Contains method is used so that the custom equality comparer is used. 
	
	4. This class does not implement the equivalent of HashSet<T>(SerializationInfo, StreamingContext).
 */
/// <summary>
/// Implements a set using a list. This container is suitable for small sets. 
/// </summary>
/// <remarks>
/// <p>
/// This set does not allow null to be an item.
/// </p>
/// <p>
/// This set cannot contain more than one item equal according to the comparer
/// given in the constructor, (or the default comparer, if none is given).
/// </p>
/// </remarks>
public class ListSet<T> : ISet<T>
{
	private const int NotFound = -1;
	private const int DefaultInitialCapacity = 10;
	
	private readonly List<T> list;

	public IEqualityComparer<T> Comparer { get; }
	
	public int Count => list.Count;
	public bool IsReadOnly => false;

	private ISet<T> ThisAsSet => this;

	public ListSet(IEqualityComparer<T> comparer = null) : this(DefaultInitialCapacity, comparer) { }

	public ListSet([NotNull] IEnumerable<T> items, IEqualityComparer<T> comparer = null) : this(comparer)
		=> UnionWith(items);

	public ListSet(int initialCapacity, IEqualityComparer<T> comparer = null)
	{
		list = new List<T>(initialCapacity);
		Comparer = comparer ?? EqualityComparer<T>.Default;
	}

	public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	
	/// <exception cref="ArgumentNullException">item is <see langref="null"/></exception>
	void ICollection<T>.Add(T item)
	{
		item.ThrowIfNull(nameof(item));
		ThisAsSet.Add(item!);
	}

	public void ExceptWith(IEnumerable<T> other)
	{
		other.ThrowIfNull(nameof(other));
		
		list.RemoveAll(item => Contains(other, item));
	}

	public void IntersectWith(IEnumerable<T> other)
	{
		other.ThrowIfNull(nameof(other));
		
		list.RemoveAll(x => !Contains(other, x));
	}

	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		other.ThrowIfNull(nameof(other));
		
		return IsSubsetOf(other) && !EqualsCount(other);
	}

	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		other.ThrowIfNull(nameof(other));
		
		return IsSupersetOf(other) && !EqualsCount(other);
	}

	public bool IsSubsetOf(IEnumerable<T> other)
	{
		other.ThrowIfNull(nameof(other));
		
		return list.All(item => Contains(other, item));
	}

	public bool IsSupersetOf(IEnumerable<T> other)
	{
		other.ThrowIfNull(nameof(other));
		
		return other.All(item => Contains(list, item));
	}

	public bool Overlaps(IEnumerable<T> other)
	{
		other.ThrowIfNull(nameof(other));
		
		return list.Any(item => Contains(other, item));
	}

	public bool SetEquals(IEnumerable<T> other)
	{
		other.ThrowIfNull(nameof(other));
		
		return EqualsCount(other) && IsSubsetOf(other);
	}

	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		other.ThrowIfNull(nameof(other));
		
		var intersection = this.Where(item => Contains(other, item));

		UnionWith(other);
		ExceptWith(intersection);
	}

	public void UnionWith(IEnumerable<T> other)
	{
		other.ThrowIfNull(nameof(other));
		
		foreach (var item in other)
		{
			ThisAsSet.Add(item);
		}
	}

	public bool Add(T item)
	{
		item.ThrowIfNull(nameof(item));
		
		if (Contains(item))
		{
			return false;
		}

		list.Add(item);

		return true;
	}

	public void Clear() => list.Clear();
	public bool Contains(T item) => Contains(list, item);
	
	public void CopyTo(T[] array, int arrayIndex)
	{
		array.ThrowIfNull(nameof(array));
		
		list.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		if (item == null)
		{
			return false; //set cannot contain null elements.
		}

		int index = list.FindIndex(x => Comparer.Equals(x, item));

		if (index == NotFound)
		{
			return false;
		}
		
		list.RemoveAt(index);
	
		return true;
	}
	private bool EqualsCount([NotNull] IEnumerable<T> other) => Count == other.Count();
	
	private bool Contains(IEnumerable<T> other, T item) => other.Any(x => Comparer.Equals(x, item));
}

public class ListDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
	private List<TKey> keys = new();
	private List<TValue> values = new();


	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => keys.Select((t, i) => new KeyValuePair<TKey, TValue>(t, values[i])).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		keys.Add(item.Key);
		values.Add(item.Value);
	}

	public void Clear()
	{
		keys.Clear();
		values.Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		int index = keys.IndexOf(item.Key);

		return index != -1 && values[index].Equals(item.Value);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		for (int i = arrayIndex; i < array.Length; i++)
		{
			keys.Add(array[i].Key);
			values.Add(array[i].Value);
		}
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		var (key, value) = item;
		int index = keys.IndexOf(key);

		if (values[index].Equals(value))
		{
			keys.RemoveAt(index);
			values.RemoveAt(index);

			return true;
		}

		return false;
	}

	public int Count => keys.Count;

	public bool IsReadOnly => false;
	public void Add(TKey key, TValue value)
	{
		keys.Add(key);
		values.Add(value);
	}

	public bool ContainsKey(TKey key) => keys.Contains(key);

	public bool Remove(TKey key)
	{
		int index = keys.IndexOf(key);

		if (index == -1) return false;
		
		keys.RemoveAt(index);
		values.RemoveAt(index);

		return true;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		int index = keys.IndexOf(key);

		if (index == -1)
		{
			value = default;
			return false;
		}

		value = values[index];
		return true;
	}

	public TValue this[TKey key]
	{
		get
		{
			int index = keys.IndexOf(key);

			if (index == -1)
			{
				throw new IndexOutOfRangeException();
			}

			return values[index];
		}

		set
		{
			int index = keys.IndexOf(key);

			if (index == -1)
			{
				Add(key, value);
			}
			else
			{
				values[index] = value;
			}
		}
	}

	public ICollection<TKey> Keys => keys;
	public ICollection<TValue> Values => values;
}
