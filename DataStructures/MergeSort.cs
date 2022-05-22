using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Xml.Xsl;
using Gamelogic.Extensions;
using JetBrains.Annotations;

namespace DataStructures;

public static class MergeSort
{

	public static bool IsSorted(IList<IComparable> list, int start, int end)
	{
		return true;
	}
	
	public static IComparable Max(IList<IComparable> list, int start, int end)
	{
		return list.First();
	}
	
	public static void Merge<T>(IList<IComparable> list, int leftStart, int rightStart, int rightEnd)
	{
		var copy = list.ToList();
		int leftFrontIndex = leftStart;
		int rightFrontIndex = rightStart;

		// Merge copy[leftStart..rightStart] with copy[rightStart..rightEnd] back into list[leftStart..rightEnd].

		IComparable PopFromLeft() => copy[leftFrontIndex++];
		IComparable PopFromRight() => copy[rightFrontIndex++];
		bool IsLeftListEmpty() => leftFrontIndex == rightStart;
		bool IsRightIsEmpty() => rightFrontIndex == rightEnd;
		bool IsLeftFrontSmaller() => copy[leftFrontIndex].CompareTo(copy[rightFrontIndex]) < 0;
		bool IsRightFrontSmallerOrEqual() => copy[rightFrontIndex].CompareTo(copy[leftFrontIndex]) <= 0;
		
		for (int destinationFrontIndex = leftStart; destinationFrontIndex <= rightEnd; destinationFrontIndex++)
		{
			void PushToDestination(IComparable item) => list[destinationFrontIndex] = item;
			
			if (IsLeftListEmpty())
			{
				PushToDestination(PopFromRight());
			}
			else if (IsRightIsEmpty())
			{
				PushToDestination(PopFromLeft());
			}
			else if (IsLeftFrontSmaller())
			{
				PushToDestination(PopFromRight());
			}
			else 
			{
				Debug.Assert(IsRightFrontSmallerOrEqual());
				PushToDestination(PopFromLeft());
			}
			
			#if DEBUG
			void AssertLoopInvariants()
			{
				Debug.Assert(IsSorted(list, leftStart, destinationFrontIndex));
			
				var biggestElement = Max(list, leftStart, destinationFrontIndex);
				bool BiggestSmallerThanElementAt(int index) => biggestElement.CompareTo(copy[index]) <= 0;

				if(!IsLeftListEmpty())
				{
					Debug.Assert(BiggestSmallerThanElementAt(leftFrontIndex));
				}

				if (!IsRightIsEmpty())
				{
					Debug.Assert(BiggestSmallerThanElementAt(rightFrontIndex));
				}
			}
			
			AssertLoopInvariants();
			#endif
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
