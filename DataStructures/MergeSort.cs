using System;
using System.Collections.Generic;
using System.Diagnostics;
using Gamelogic.Extensions;
using JetBrains.Annotations;

namespace DataStructures;

public static class MergeSort
{
	public record struct UnsafeQueue<T>
	{
		public readonly IList<T> Elements;
		public int Start;
		public int End;

		public UnsafeQueue(IList<T> elements, int start, int end)
		{
			AssertInRange(elements, start, end);

			Elements = elements;
			Start = start;
			End = end;
		}

		public bool Empty() => Start == End;
		
		public T Peek()
		{
			GLDebug.Assert(!Empty());
			return Elements[Start];
		}

		public T Pop()
		{
			GLDebug.Assert(!Empty());
			return Elements[Start++];
		}

		public void Push(T item)
		{
			GLDebug.Assert(End < Elements.Count);
			Elements[End++] = item;
		}
	}
	
	public static bool IsSorted<T>(IList<T> list, int start, int end) where T : IComparable
	{
		list.ThrowIfNull(nameof(list));
		if (start >= list.Count) throw new ArgumentOutOfRangeException(nameof(start));
		if (end > list.Count) throw new ArgumentOutOfRangeException(nameof(end));
		if (end <= start) throw new ArgumentOutOfRangeException(nameof(end));

		int length = end - start;

		if (length <= 1)
		{
			return true;
		}
		
		//We have at least two elements
		for (int i = start + 1; i < end; i++)
		{
			//Negative indexes are impossible
			GLDebug.Assert(i - 1 >= 0);
			
			var item0 = list[i - 1];
			var item1 = list[i];
			
			if(item0.CompareTo(item1) > 0)
			{
				return false;
			}
			
			//All items up to i are sorted
		}
		
		//All items up to the last index are sorted
		return true;
	}
	
	public static IComparable Max<T>(IList<T> list, int start, int end) where T : IComparable
	{
		list.ThrowIfNull(nameof(list));
		if (start >= list.Count) throw new ArgumentOutOfRangeException(nameof(start));
		if (end > list.Count) throw new ArgumentOutOfRangeException(nameof(end));
		if (end <= start) throw new ArgumentOutOfRangeException(nameof(end));

		int length = end - start;

		if (length == 0) throw new ArgumentException(null, nameof(list));

		if (length == 1)
		{
			return true;
		}

		var biggest = list[start];

		for (int i = start + 1; i < end; i++)
		{
			if (list[i].CompareTo(biggest) > 0)
			{
				biggest = list[i];
			}
			//biggest is larger than all elements from start to i
		}
		//biggest is larger than all elements from start to end - 1

		return biggest;
	}
	
	public static void Sort<T>(IList<T> list) where T : IComparable
	{
		var helpList = new List<T>(list.Count);
		Sort(list, 0, list.Count - 1, helpList);
	}

	private static void Merge<T>(IList<T> list, int leftStart, int rightStart, int rightEnd, IList<T> copy) where T : IComparable
	{
		AssertInRange(list, leftStart, rightEnd);
		AssertInRange(copy, leftStart, rightEnd);
		
		Copy(list, copy, leftStart, rightEnd);
			
		var left = new UnsafeQueue<T>(copy, leftStart, rightStart);
		var right = new UnsafeQueue<T>(copy, rightStart, rightEnd);
		var destination = new UnsafeQueue<T>(list, leftStart, rightEnd);

		// Merge left and right into destination.

		for (int destinationFrontIndex = leftStart; destinationFrontIndex <= rightEnd; destinationFrontIndex++)
		{
			if (left.Empty())
			{
				destination.Push(right.Pop());
			}
			else if (right.Empty())
			{
				destination.Push(left.Pop());
			}
			else if (left.Peek().CompareTo(right.Peek()) < 0)
			{
				destination.Push(right.Pop());
			}
			else 
			{
				GLDebug.Assert(right.Peek().CompareTo(left.Peek()) <= 0);
				destination.Push(left.Pop());
			}
			
#if DEBUG
			void AssertLoopInvariants()
			{
				GLDebug.Assert(IsSorted(list, leftStart, destinationFrontIndex));
			
				var biggestElement = Max(list, leftStart, destinationFrontIndex);
				bool BiggestSmallerThan(T other) => biggestElement.CompareTo(other) <= 0;

				if(!left.Empty())
				{
					GLDebug.Assert(BiggestSmallerThan(left.Peek()));
				}

				if (!right.Empty())
				{
					GLDebug.Assert(BiggestSmallerThan(right.Peek()));
				}
			}
			
			AssertLoopInvariants();
#endif
		}
	}

	private static void Copy<T>(IList<T> source, IList<T> destination, int start, int end)
	{
		AssertInRange(source, start, end);
		AssertInRange(destination, start, end);

		for (int i = start; i < end; i++)
		{
			destination[i] = source[i];
		}
	}

	[Conditional("DEBUG")]
	private static void AssertInRange<T>(ICollection<T> source, int start, int end)
	{
		GLDebug.Assert(source != null);
		GLDebug.Assert(start > 0);
		GLDebug.Assert(end > 0);
		GLDebug.Assert(start <= end);
		GLDebug.Assert(end < source.Count);
	}

	private static void Sort<T>(IList<T> list, int start, int end, IList<T> helpList) where T : IComparable
	{
		AssertInRange(list, start, end);
		AssertInRange(helpList, start, end);
		
		if (end <= start) return;
		int mid = start + (end - start)/2;
		Sort(list, start, mid, helpList); // Sort left half.
		Sort(list, mid+1, end, helpList); // Sort right half.
		Merge(list, start, mid, end, helpList); 
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
		GLDebug.Assert(list.Count >= 2);
		
		for (int i = 1; i < list.Count; i++)
		{
			//Negative indexes are impossible
			GLDebug.Assert(i - 1 >= 0);
			
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
