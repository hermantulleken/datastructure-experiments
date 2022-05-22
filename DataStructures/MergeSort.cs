using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Xsl;
using Gamelogic.Extensions;
using JetBrains.Annotations;

namespace DataStructures;

public static class MergeSort
{
	public static void Merge<T>(IList<IComparable> list, int start, int middle, int high)
	{
		bool Less(IComparable x, IComparable y) => x.CompareTo(y) < 0;

		var helpList = new List<IComparable>(list.Count);
		
		// Merge list[start..middle] with list[middle+1..high].
		int i = start, j = middle+1;
		
		for (int k = start; k <= high; k++) // Copy list[start..high] to helpList[start..high].
		{
			helpList[k] = list[k];
		}
		
		for (int k = start; k <= high; k++) // Merge back to list[start..high].
		{
			if (i > middle)
			{
				list[k] = helpList[j++];
			}
			else if (j > high)
			{
				list[k] = helpList[i++];
			}
			else if (Less(helpList[j], helpList[i]))
			{
				list[k] = helpList[j++];
			}
			else
			{
				list[k] = helpList[i++];
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
