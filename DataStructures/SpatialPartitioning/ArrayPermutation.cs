using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace DataStructures.SpatialPartitioning;

public class ArrayPermutation : IPermutation
{
	#region Error messages
	private const string SetCountsMustMatch = $"Cannot multiply permutations defined on different sets (that is, their {nameof(SetCount)} properties must match).";
	private const string NotABijection = "Not a bijection";
	private const string CannotBeNegative = "Argument cannot be negative.";
	private const string MustBePositive = "Argument must be positive.";
	#endregion 
	
	//Using an immutable array allows us to share these arrays between permutations if we chose to
	private ImmutableArray<int> Map { get; }

	public int this[int element]
	{
		get
		{
			ValidateElement(element);

			return Map[element];
		}
	}
	
	public int SetCount => Map.Length;
	
	/*
		Since the array is immutable, it is safe to use directly and we don't need to copy it. 
	 
		We assume that it is structurally sound - i.e. a permutation
		(however, we do verify this in debug versions).
	*/
	private ArrayPermutation(ImmutableArray<int> map)
	{
		DebugValidateIsPermutation(map);
		
		Map = map;
	}
	
	#region Factory methods
	/// <remarks>
	/// If <paramref name="permutation"/> is already of type <see cref="ArrayPermutation"/>, it is returned directly.
	/// This is safe since instances of <see cref="ArrayPermutation"/> are immutable.
	/// </remarks>
	/*
		C# does not allow us to define explicit conversion methods on interfaces.
		See https://stackoverflow.com/questions/2433204/why-cant-i-use-interface-with-explicit-operator
		
		MakeMap is an local function; any method that requires the map of a IPermutation
		is better defined on ArrayPermutation directly. Use this conversion method to get the result. 
		See for example how this strategy is used to implement IPermutationExtensions.LexicographicalRank 
		(implemented by ArrayPermutation.LexicographicalRank).
	*/ 
	public static ArrayPermutation FromPermutation([NotNull] IPermutation permutation)
	{
		static ImmutableArray<int> MakeMap(IPermutation permutation)
		{
			int[] map = new int[permutation.SetCount];
		
			for (int i = 0; i < permutation.SetCount; i++)
			{
				map[i] = permutation[i];
			}

			return ImmutableArray.Create(map);
		}
		
		ValidateNotNull(permutation);
		
		if (permutation is ArrayPermutation arrayPermutation)
		{
			return new ArrayPermutation(arrayPermutation.Map);
		}

		return new ArrayPermutation(MakeMap(permutation));
	}

	public static ArrayPermutation FromPermutationMap(params int[] map)
	{
		ValidateNotNull(map);
		DebugValidateIsPermutation(map);
		
		return FromPermutationMap(ImmutableArray.Create(map));
	}

	public static ArrayPermutation FromPermutationMap(ImmutableArray<int> map)
	{
		ValidateIsPermutation(map);

		return new ArrayPermutation(map);
	}

	public static ArrayPermutation FromCycle(int setCount, params int[] cycle)
	{
		ValidateIsPositive(setCount);
		ValidateNotNull(cycle);
		ValidateCycle(setCount, cycle);
		
		int[] map = GetIdentityMap(setCount);

		for (int i = 0; i < cycle.Length - 1; i++)
		{
			map[cycle[i]] = cycle[i + 1];
		}

		map[cycle[^1]] = cycle[0];

		return FromPermutationMap(map);
	}

	public static ArrayPermutation FromLexicographicalRank(int setCount, int rank)
	{
		ValidateIsPositive(setCount);
		ValidateNotNegative(rank);
		
		return FromPermutationMap(MapFromLexicographicalRank(setCount, rank));
	}
	
	public static ArrayPermutation Identity(int setCount)
	{
		ValidateIsPositive(setCount);
		
		return FromPermutationMap(GetIdentityMap(setCount));
	}

	#endregion 

	IBijection<int, int> IBijection<int, int>.Inverse() => Inverse();

	public IBijection<int, T> Compose<T>(IBijection<int, T> other) => Bijection.Compose(this, other);
	
	/// <remarks>
	/// This is an eager, recursive algorithm. A different algorithm should be used if
	/// lazy evaluation of the list is required. 
	/// </remarks>
	public static IEnumerable<ArrayPermutation> GeneratePermutations(int setCount)
	{
		ValidateIsPositive(setCount);

		int currentSetCount = 1;
		var permutations = new List<ArrayPermutation> { Identity(currentSetCount) };

		while (currentSetCount < setCount)
		{
			permutations = ExpandPermutations(permutations);
			setCount++;
		}

		return permutations;
	}

	public IPermutation Inverse()
	{
		int[] newMap = new int[SetCount];
		
		for (int element = 0; element < SetCount; element++)
		{
			newMap[Map[element]] = element;
		}

		return FromPermutationMap(newMap);
	}

	public IPermutation Compose(IPermutation other)
	{
		ValidateNotNull(other);
		ValidateSetCountsMatch(other);

		int[] newMap = new int[SetCount];

		for (int element = 0; element < SetCount; element++)
		{
			newMap[element] = other[this[element]];
		}

		return FromPermutationMap(newMap);
	}

	/// <inheritdoc />
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

		for (int element = 0; element < SetCount; element++)
		{
			if (this[element] != other[element])
			{
				return false;
			}
		}

		return true;
	}
	
	//From https://www.movingai.com/GDC16/permutations.html
	public int LexicographicalRank()
	{
		int[] map = Map.ToArray();
		int rank = 0;
		
		for (int index = 0; index < SetCount; index++)
		{
			int entriesLeftCount = SetCount - index;
			rank += map[index] * (int) Combinatorics.Factorial(entriesLeftCount - 1);
			
			for (int remainingIndex = index; remainingIndex < SetCount; remainingIndex++)
			{
				if (map[remainingIndex] > map[index])
				{
					map[remainingIndex]--;
				}
			}
		}
		return rank;
	}

	//From https://www.movingai.com/GDC16/permutations.html
	private static int[] MapFromLexicographicalRank(int setCount, int rank)
	{
		GLDebug.Assert(setCount >= 0);
		GLDebug.Assert(rank >= 0);
		
		int[] map = new int[setCount];
		
		for (int index = setCount - 1; index >= 0; index--)
		{
			int entriesLeftCount = setCount - index;
			map[index] = rank % entriesLeftCount;
			rank /= entriesLeftCount;

			//Why is this index + 1, but in Rank it is only from index?
			for (int remainingIndex = index + 1; remainingIndex < setCount; remainingIndex++)
			{
				if (map[remainingIndex] >= map[index])
				{
					map[remainingIndex]++;
				}
			}
		}

		return map;
	}

	private bool InRange(int element) => InRange(element, SetCount);
	private static bool InRange(int element, int setCount) => 0 <= element && element < setCount;

	private static int[] GetIdentityMap(int setCount)
	{
		int[] map = new int[setCount];

		for (int element = 0; element < setCount; element++)
		{
			map[element] = element;
		}

		return map;
	}
	private static List<ArrayPermutation> ExpandPermutations(IReadOnlyCollection<ArrayPermutation> permutations)
	{
		var first = permutations.First();
		int setCount = first.SetCount;
		var newPermutations = new List<ArrayPermutation>();
		var maps = permutations.Select(permutation => permutation.Map);
		
		foreach (var map in maps)
		{
			for (int insertPosition = 0; insertPosition < setCount + 1; insertPosition++)
			{
				var newMap = map.Insert(insertPosition, setCount);
				newPermutations.Add(new ArrayPermutation(newMap));
			}
		}

		return newPermutations;
	}
	
	#region Validation methods
	private static void ValidateIsPositive(
		int setCount, 
		[CallerArgumentExpression("setCount")] string setCountArgumentName = null)
	{
		if (setCount < 0) throw new ArgumentOutOfRangeException(setCountArgumentName, MustBePositive);
	}
	
	private static void ValidateNotNegative(
		int setCount, 
		[CallerArgumentExpression("setCount")] string setCountArgumentName = null)
	{
		if (setCount < 0) throw new ArgumentOutOfRangeException(setCountArgumentName, CannotBeNegative);
	}

	private static void ValidateNotNull<T>(
		T obj,
		[CallerArgumentExpression("obj")] string objArgumentName = null) where T : class
	{
		if (obj == null) throw new ArgumentNullException(objArgumentName);
	}

	private void ValidateSetCountsMatch(IPermutation other,
		[CallerArgumentExpression("other")] string otherArgumentName = null)
	{
		if (SetCount != other.SetCount) throw new ArgumentException(SetCountsMustMatch, otherArgumentName);
	}

	[Conditional(GLDebug.Debug)]
	private static void DebugValidateIsPermutation(IList<int> map,
		[CallerArgumentExpression("map")] string mapArgumentName = null) => ValidateIsPermutation(map, mapArgumentName);

	private static void ValidateIsPermutation(
		// ReSharper disable once SuggestBaseTypeForParameter (We do want indexable containers for this.)
		IList<int> map, 
		[CallerArgumentExpression("map")] string mapArgumentName = null)
	{
		int setCount = map.Count;
		int distinctCount = map.Distinct().Count();

		if (setCount != distinctCount)
		{
			throw new ArgumentException(NotABijection, mapArgumentName);
		}

		if (map.Any(element => !InRange(element, setCount)))
		{
			int element = map.First(element => !InRange(element, setCount));
			
			throw new ArgumentException($"Has elements out of range. Elements must be in [0, {setCount}] but contains {element}.", mapArgumentName);
		}
	}
	
	private void ValidateElement(
		int element, 
		[CallerArgumentExpression("element")] string elementArgumentName = null)
	{
		if (!InRange(element))
		{
			throw new ArgumentOutOfRangeException(elementArgumentName, $"Must be in [0, {SetCount}), but is {element}.");
		}
	}

	private static void ValidateCycle(
		int setCount,
		// ReSharper disable once SuggestBaseTypeForParameter (We do want indexable containers for this.)
		IList<int> cycle, 
		[CallerArgumentExpression("cycle")] string cycleArgumentNam = null)
	{
		int distinctCount = cycle.Distinct().Count();

		if (cycle.Count != distinctCount)
		{
			throw new ArgumentException("Has repeated elements", cycleArgumentNam);
		}

		if (cycle.Any(element => !InRange(element, setCount)))
		{
			throw new ArgumentException("Has elements out of range", cycleArgumentNam);
		}
	}
	#endregion
}
