using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gamelogic.Extensions;
using JetBrains.Annotations;

namespace DataStructures;

public static class MergeSort
{
	public class UnsafeQueue<T>
	{
		private readonly IList<T> elements;
		private int start;
		private int end;

		public UnsafeQueue(IList<T> elements, int start, int end)
		{
			this.elements = elements ?? throw new ArgumentNullException(nameof(elements));

			if (end < 0 || end < start || end > elements.Count) throw new ArgumentException(null, nameof(end));
			if (start < 0 || start >= elements.Count) throw new ArgumentException(null, nameof(start));
			
			this.start = start;
			this.end = end;
		}

		public bool Empty() => start == end;
		
		public T Peek()
		{
			Debug.Assert(!Empty());
			return elements[start];
		}

		public T Pop()
		{
			Debug.Assert(!Empty());
			return elements[start++];
		}

		public void Push(T item)
		{
			Debug.Assert(end < elements.Count);
			elements[end++] = item;
		}
	}
	
	public static bool IsSorted(IList<IComparable> list, int start, int end)
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
			Debug.Assert(i - 1 >= 0);
			
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
	
	public static IComparable Max(IList<IComparable> list, int start, int end)
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
	
	public static void Merge<T>(IList<IComparable> list, int leftStart, int rightStart, int rightEnd)
	{
		var copy = list.ToList();
		var left = new UnsafeQueue<IComparable>(copy, leftStart, rightStart);
		var right = new UnsafeQueue<IComparable>(copy, rightStart, rightEnd);
		var destination = new UnsafeQueue<IComparable>(list, leftStart, rightEnd);

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
				Debug.Assert(right.Peek().CompareTo(left.Peek()) <= 0);
				destination.Push(left.Pop());
			}
			
			#if DEBUG
			void AssertLoopInvariants()
			{
				Debug.Assert(IsSorted(list, leftStart, destinationFrontIndex));
			
				var biggestElement = Max(list, leftStart, destinationFrontIndex);
				bool BiggestSmallerThan(IComparable other) => biggestElement.CompareTo(other) <= 0;

				if(!left.Empty())
				{
					Debug.Assert(BiggestSmallerThan(left.Peek()));
				}

				if (!right.Empty())
				{
					Debug.Assert(BiggestSmallerThan(right.Peek()));
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
