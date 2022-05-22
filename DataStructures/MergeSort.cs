using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Xsl;
using Gamelogic.Extensions;
using JetBrains.Annotations;

namespace DataStructures;

public static class MergeSort
{
	public static void Merge<T>(IList<IComparable> list, int leftStart, int leftEnd, int rightEnd)
	{
		bool Less(IComparable x, IComparable y) => x.CompareTo(y) < 0;
		bool LessOrEqual(IComparable x, IComparable y) => x.CompareTo(y) <= 0;

		IComparable TakeNextFrom(IList<IComparable> source, ref int index) => source[index++];
		
		var copy = list.ToList();
		
		// Merge list[leftStart..leftEnd] with list[leftEnd+1..rightEnd].
		int leftIndex = leftStart;
		int rightIndex = leftEnd + 1; //right start
		
		bool LeftListEmpty() => leftIndex > leftEnd;
		bool RightIsEmpty() => rightIndex > rightEnd;
		bool RightSmaller() => Less(copy[rightIndex], copy[leftIndex]);
		bool LeftSmallerOrEqual() => LessOrEqual(copy[leftIndex], copy[rightIndex]);

		for (int destinationIndex = leftStart; destinationIndex <= rightEnd; destinationIndex++) // Merge back to list[leftStart..rightEnd].
		{
			if (LeftListEmpty())
			{
				list[destinationIndex] = TakeNextFrom(copy, ref rightIndex);
			}
			else if (RightIsEmpty())
			{
				list[destinationIndex] =  TakeNextFrom(copy, ref leftIndex);
			}
			else if (RightSmaller())
			{
				list[destinationIndex] = TakeNextFrom(copy, ref rightIndex);
			}
			else 
			{
				Debug.Assert(LeftSmallerOrEqual());
				list[destinationIndex] = TakeNextFrom(copy, ref leftIndex);
			}
		}
	}
	
	/// <summary>
	/// Checks whether a list is sorted.
	/// </summary>
	/// <param name="list">A list of elements to check. Cannot be null.</param>
	/// <param name="comparer">A comparer to use to compare elements. If not supplied
	/// or null, <see cref="Comparer{T}.Default"/> is used.</param>
	/// <typeparam name="T"></typeparam>
	/// <returns><see langword="true"/> the list is sorted, <see langword="false"/> otherwise.</returns>
	public static bool IsSorted<T>([NotNull] IList<T> list, [CanBeNull] IComparer<T> comparer = null)
	{
		list.ThrowIfNull(nameof(list));
		
		comparer ??= Comparer<T>.Default;
		
		if (list.Count <= 1)
		{
			return true;
		}
		
		//We have at least two elements
		Debug.Assert(list.Count >= 2);
		
		for (int i = 1; i < list.Count; i++)
		{
			//Negative indexes are impossible
			Debug.Assert(i - 1 >= 0);
			
			var item0 = list[i - 1];
			var item1 = list[i];
			
			if(comparer.Compare(item0, item1) > 0)
			{
				return false;
			}
			
			//All items up to i are sorted
		}
		
		//All items up to the last index are sorted
		return true;
	}
}
