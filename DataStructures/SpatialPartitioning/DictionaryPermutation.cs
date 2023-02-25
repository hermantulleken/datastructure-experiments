using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DataStructures.SpatialPartitioning;

public sealed class DictionaryPermutation : IPermutation
{
	private sealed class NoIdentityDictionary
	{
		private readonly IReadOnlyDictionary<int, int> entries;
		
		public static readonly NoIdentityDictionary Empty = new NoIdentityDictionary();

		public int this[int key] => entries.ContainsKey(key) ? entries[key] : key;

		public IEnumerable<int> Keys => entries.Keys;
		public IEnumerable<int> Values => entries.Values;

		private NoIdentityDictionary() => entries = new Dictionary<int, int>();
		public NoIdentityDictionary(IEnumerable<KeyValuePair<int, int>> mappings)
			=> entries = new Dictionary<int, int>(mappings.Where(pair => pair.Key != pair.Value));

		public NoIdentityDictionary Invert() => new (entries.Select(pair => new KeyValuePair<int, int>(pair.Value, pair.Key)));
	}
	
	private const string SetCountsMustMatch = $"Cannot multiply permutations defined on different sets (that is, their {nameof(SetCount)} properties must match).";

	private readonly NoIdentityDictionary map;
	public int this[int index] => map[index];
	
	public int SetCount { get; }

	private DictionaryPermutation(int setCount, NoIdentityDictionary map)
	{
		SetCount = setCount;
		this.map = map;
	}

	public static IPermutation Identity(int setCount) => new DictionaryPermutation(setCount, NoIdentityDictionary.Empty);
	
	public static DictionaryPermutation FromMappings(int setCount, IEnumerable<KeyValuePair<int, int>> mappings) 
		=> new (setCount, new NoIdentityDictionary(mappings));
	public IPermutation Inverse() => new DictionaryPermutation(SetCount, map.Invert());

	IBijection<int, int> IBijection<int, int>.Inverse() => Inverse();

	public IPermutation Compose(IPermutation other)
	{
		ValidateSetCountsMatch(other);
		
		var keys1 = map.Keys;
		var values1 = map.Values;
		var map2 = GetMap(other);
		var keys2 = map2.Keys.Except(values1);
		var allKeys = keys1.Concat(keys2);
		var newMappings = allKeys.Select(key => new KeyValuePair<int, int>(key, map2[map[key]]));

		return FromMappings(SetCount, newMappings);
	}

	public IBijection<int, T> Compose<T>(IBijection<int, T> other) => Bijection.Compose(this, other);

	public bool Equivalent(IPermutation other)
	{
		if (other == null || SetCount != other.SetCount)
		{
			return false;
		}
		
		if (this == other)
		{
			return true;
		}
		
		//If the number of non-fixed entries are the same, and keys of this maps to keys of others, the maps must be identical. 
		return CountNonFixedPoints() == other.CountNonFixedPoints() 
		       && map.Keys.All(key => this[key] == other[key]);
	}
	
	public int CountNonFixedPoints() => map.Keys.Count();

	private static NoIdentityDictionary GetMap(IPermutation permutation)
	{
		if (permutation is DictionaryPermutation dictionaryPermutation)
		{
			return dictionaryPermutation.map;
		}

		var mappings = permutation.Elements()
			.Where(element => element != permutation[element])
			.Select(element => new KeyValuePair<int, int>(element, permutation[element]));

		return new NoIdentityDictionary(mappings);
	}
	
	private void ValidateSetCountsMatch(IPermutation other,
		[CallerArgumentExpression("other")] string otherArgumentName = null)
	{
		if (SetCount != other.SetCount) throw new ArgumentException(SetCountsMustMatch, otherArgumentName);
	}
}
