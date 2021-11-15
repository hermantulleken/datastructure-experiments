﻿// Copyright Gamelogic (c) http://www.gamelogic.co.za

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using JetBrains.Annotations;

namespace Gamelogic.Extensions.Algorithms
{
	/// <summary>
	/// Combinatorial algorithms, including generating tuples, combinations, permutations and
	/// partitions.
	/// 
	/// Except for PowerSet, all methods are implemented after Knuth, described in The Art of 
	/// Computer Programming Volume 4.
	/// </summary>
	public static class Combinatorial
	{
		#region Static Methods

		#region Partitions		
		/// <summary>
		/// Returns a list of all the partitions of a list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list.</param>
		/// <returns>IEnumerable&lt;IEnumerable&lt;IEnumerable&lt;T&gt;&gt;&gt;.</returns>
		/// <remarks>If the list is (a b c), then ((a) (b c)) and ((a) (b) (c)) would be examples of partitions.</remarks>
		public static IEnumerable<IEnumerable<IEnumerable<T>>> Partitions<T>(this IEnumerable<T> list)
		{
			var listArray = list.ToArray();
			int elementCount = listArray.Length;
			var indexes = RestrictedGrowthStrings(elementCount);

			return indexes.Select(index => AccessByRestrictedGrowthString(listArray, index));
		}
		#endregion

		#region PowerSet		
		/// <summary>
		/// Returns the power set of the input, that is, the set of all subsets of the input.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="input">The input.</param>
		/// <returns>IEnumerable&lt;IEnumerable&lt;T&gt;&gt;.</returns>
		public static IEnumerable<IEnumerable<T>> PowerSet<T>(this IEnumerable<T> input)
		{
			input.ThrowIfNull("input");

			return PowerSetImpl(input.ToList());
		}

		#endregion

		#region Tuples

		/// <summary>
		/// Generate all possible tuples of length n with digits 0 to n-1.
		/// </summary>
		/// <param name="n">The length of tuples to generate. All digits are also between 0 and n-1.</param>
		public static IEnumerable<int[]> Tuples(int n)
		{
			n.ThrowIfNegative(nameof(n));
			
			var radixes = RangeArray(n);

			return MultiRadixTuplesImpl(radixes, e => e);
		}

		/// <summary>
		/// Generates all tuples with mixed radixes.
		/// </summary>
		/// <param name="radixes">The array of radixes for each position in the tuple.</param>
		/// <example>
		/// The call <c>MultiRadixTuples(new int[]{2, 3})</c> will generate the following tuples:
		/// <code>
		/// <![CDATA[
		/// [0, 0]
		/// [0, 1]
		/// [0, 2]
		/// [1, 0]
		/// [1, 1]
		/// [1, 2]
		/// ]]>
		/// </code>
		/// </example>
		public static IEnumerable<int[]> MultiRadixTuples(int[] radixes)
		{
			radixes.ThrowIfNull("radixes");
			ValidateAllValuesPositive(radixes);

			return MultiRadixTuplesImpl(radixes, e => e);
		}

		public static IEnumerable<T[]> MultiRadixTuples<T>(this IEnumerable<IEnumerable<T>> elements)
		{
			var elementArray = elements.Select(l => (IList<T>)l.ToArray()).ToArray();
			var indexes = MultiRadixTuples(elementArray.Select(l => l.Count));

			return indexes.Select(index => AccessByIndex(elementArray, index));
		}

		public static IEnumerable<T[]> Tuples<T>(this IEnumerable<T> list)
		{
			list.ThrowIfNull(nameof(list));

			var objList = list.ToArray();
			var indexes = Tuples(objList.Length);

			return indexes.Select(index => index.Select(i => objList[i]));
		}

		/// <summary>
		/// Generates n-tuples of integers 0 to n-1 and applies the selector to them.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"><see cref="n"/> is negative</exception>
		public static IEnumerable<T> Tuples<T>(int n, Func<int[], T> select)
		{
			n.ThrowIfNegative(nameof(n));
		
			select.ThrowIfNull("select");
			var radixes = new int[n].Init(n);

			return MultiRadixTuplesImpl(radixes, select);
		}

		#endregion

		#region Permutations

		/// <summary>
		/// Generates all permutations of the numbers 0 to n - 1.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"><see cref="n"/> is negative</exception>
		public static IEnumerable<int[]> Permutations(int n)
		{
			n.ThrowIfNegative(nameof(n));

			return PermutationsImpl(n, e => e);
		}


		/// <summary>
		/// Generates all permutations of the list of elements.
		/// </summary>
		public static IEnumerable<T[]> Permutations<T>([NotNull] this IEnumerable<T> list)
		{
			list.ThrowIfNull(nameof(list));
		
			var items = list.ToArray();
			var permutationIndexes = Permutations(items.Length);

			return permutationIndexes.Select(permutationIndex => permutationIndex.Select(i => items[i]));
		}

		#endregion

		#region Combinations		
		/// <summary>
		/// Generates all combinations of m elements selected from the list.
		/// </summary>
		/// <typeparam name="T">The type of elements in the list.</typeparam>
		/// <param name="list">The list to select from.</param>
		/// <param name="m">The number of elements in each combination.</param>
		/// <returns>IEnumerable&lt;T[]&gt;.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// m;must be nonnegative
		/// or
		/// m;must be smaller than n
		/// </exception>
		public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> list, int m)
		{
			m.ThrowIfNegative(nameof(m));
			list.ThrowIfNull("list");

			var listArray = list.ToArray();

			if (m > listArray.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(m), m, $"Must be smaller than the number of elements in {nameof(list)}.");
			}

			var combinationIndexes = CombinationsImpl(m, listArray.Length, e => e);

			return combinationIndexes.Select(combinationIndex => combinationIndex.Select(i => listArray[i]));
		}

		public static IEnumerable<T> Combinations<T>(int m, int n, Func<int[], T> select)
		{
			m.ThrowIfNegative(nameof(m));
			n.ThrowIfNegative(nameof(n));

			if (m > n)
			{
				throw new ArgumentOutOfRangeException(nameof(m), m, $"Must be smaller than {nameof(n)}.");
			}

			select.ThrowIfNull("select");

			return CombinationsImpl(m, n, select);
		}

		#endregion
		
		#endregion

		#region Implementation
		[AssertionMethod]
		private static void ValidateAllValuesPositive(int[] radixes)
		{
			static bool IsNotPositive (int n) => n <= 0;

			if (radixes.Any(IsNotPositive))
			{
				throw new ArgumentOutOfRangeException($"All values in {nameof(radixes)} must be positive");
			}
		}

		private static IEnumerable<T> CombinationsImpl<T>(int m, int n, Func<int[], T> select)
		{
			var combination = RangeArray(m + 2);

			combination[m] = n;
			combination[m + 1] = 0;

			while (true)
			{
				yield return select(combination.Take(m));
				int j = 0;

				while (combination[j] + 1 == combination[j + 1])
				{
					combination[j] = j;
					j++;

					if (j >= m) yield break;
				}

				combination[j]++;
			}
		}

		private static IEnumerable<T> MultiRadixTuplesImpl<T>(IList<int> radixes, Func<int[], T> select)
		{
			var tuple = new int[radixes.Count].Init(0);

			while (true)
			{
				yield return select(tuple.Copy());

				int currentIndex = radixes.Count - 1;
				tuple[currentIndex]++;

				while (tuple[currentIndex] >= radixes[currentIndex]) //while we overflow, we carry over
				{
					currentIndex--;

					if (currentIndex < 0) yield break;

					tuple[currentIndex]++;
					tuple[currentIndex + 1] = 0;
				}
			}
		}

		private static IEnumerable<T> PermutationsImpl<T>(int n, Func<int[], T> select)
		{
			if (n == 0)
			{
				yield break;
			}

			var permutation = RangeArray(n);

			if (n == 1)
			{
				yield return select(permutation.Copy());	
				yield break;
			}

			while (true)
			{
				int index = n - 2;

				Debug.Assert(index >= 0, "index must be greater than or equal to 0");
				Debug.Assert(index + 1 < permutation.Length, "index + 1 must be smaller than permutation length, but index == " + index);

				while (permutation[index] >= permutation[index + 1]) //while we overflow, we carry over
				{
					index--;

					if (index < 0) yield break;
				}

				// Find the right most index where the value
				// is smaller than at the current index 
				int index2 = n - 1;

				while (permutation[index] >= permutation[index2])
				{
					index2--;
				}

				Swap(permutation, index, index2);

				ReverseRange(permutation, index + 1, n);

				yield return select(permutation.Copy());	
			}
		}


		//P417
		private static IEnumerable<int[]> RestrictedGrowthStrings(int n)
		{
			//H1
			var a = new int[n].Init(0);
			var b = new int[n - 1].Init(1);

			int m = 1;

			while (true)
			{
				//H2
				yield return a;

				if (a[n - 1] == m)
				{
					//H4
					int j = n - 2;

					while (a[j] == b[j])
					{
						j--;
					}

					//H5
					if (j == 0) yield break;
		
					a[j]++;

					//H6
					m = b[j] + ((a[j] == b[j]) ? 1 : 0);
					j++;

					while (j < n - 1)
					{
						a[j] = 0;
						b[j] = m;
						j++;
					}

					a[n - 1] = 0;
				}
				else
				{
					//H3
					a[n - 1]++;
				}
			}
		}

		//indexes[m][n] means object at m should be in set n in the result
		private static IEnumerable<IEnumerable<T>> AccessByRestrictedGrowthString<T>(IList<T> values, IList<int> indexes)
		{
			var setCount = indexes.Max() + 1;
			var sets = new IList<T>[setCount];

			for (int i = 0; i < setCount; i++)
			{
				sets[i] = new List<T>();
			}

			for (int i = 0; i < indexes.Count; i++)
			{
				sets[indexes[i]].Add(values[i]);
			}

			return sets;
		}

		//indexes
		private static T[] AccessByIndex<T>(IList<IList<T>> values, IList<int> indexes)
		{
			var set = new T[values.Count];

			for (int i = 0; i < values.Count; i++)
			{
				set[i] = values[i][indexes[i]];
			}

			return set;
		}

		//From http://stackoverflow.com/questions/19890781/creating-a-power-set-of-a-sequence
		private static IEnumerable<IEnumerable<T>> PowerSetImpl<T>(IList<T> input)
		{
			int n = input.Count;

			// Power set contains 2^N subsets.
			int powerSetCount = 1 << n;

			for (int setMask = 0; setMask < powerSetCount; setMask++)
			{
				var s = new List<T>();

				for (int i = 0; i < n; i++)
				{
					// Check whether element at position i of the input collection should go to the current subset.
					if ((setMask & (1 << i)) > 0)
					{
						s.Add(input[i]);
					}
				}

				yield return s;
			}
		}

		#endregion

		#region Array Extensions

		private static T[] Init<T>(this T[] array, T item)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = item;
			}

			return array;
		}

		private static T[] Take<T>(this T[] array, int m)
		{
			var newArray = new T[Math.Min(array.Length, m)];

			for (int i = 0; i < newArray.Length; i++)
			{
				newArray[i] = array[i];
			}

			return newArray;
		}
		
		private static TResult[] Select<TSource, TResult>(this TSource[] array, Func<TSource, TResult> select)
		{
			var newArray = new TResult[array.Length];

			for (int i = 0; i < array.Length; i++)
			{
				newArray[i] = select(array[i]);
			}

			return newArray;
		}

		private static int[] RangeArray(int n)
		{
			int[] newArray = new int[n];

			for (int i = 0; i < n; i++)
			{
				newArray[i] = i;
			}

			return newArray;
		}

		private static T[] Copy<T>(this T[] array)
		{
			return array.ToArray();
		}

		private static void Swap<T>(IList<T> permutation, int index1, int index2)
		{
			var tmp = permutation[index1];
			permutation[index1] = permutation[index2];
			permutation[index2] = tmp;
		}

		//reverses the elements in range [startIndex endIndex)
		private static void ReverseRange<T>(T[] permutation, int startIndex, int endIndex)
		{
			int leftIndex = startIndex;
			int rightIndex = endIndex - 1;

			while (leftIndex < rightIndex)
			{
				Swap(permutation, rightIndex, leftIndex);

				rightIndex--;
				leftIndex++;
			}
		}

		#endregion
	}
}