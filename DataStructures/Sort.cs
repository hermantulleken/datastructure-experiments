using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace DataStructures
{
	public static class Sort
	{
		private static readonly int[] CiuraSequence = new int[] { 1, 4, 10, 23, 57, 132, 301, 701 };
		//KnuthSequence  1, 4, 13, 40, 121, 364, 1093, ...
		
		private const int WhenToSwitch_ForPerformanceMeasurements = 8;

		public static void RotateLeft<T>(IList<T> list) => RotateLeft(list, 0, list.Count);
		public static void RotateRight<T>(IList<T> list) => RotateRight(list, 0, list.Count);

		public static void RotateLeft<T>(IList<T> list, int start, int end)
		{
			bool nothingToRotate = end - start <= 1;

			if (nothingToRotate) return;

			for (int i = start; i < end - 1; i++)
			{
				Algorithms.SwapAt(list, i, i + 1);
			}
		}

		public static void RotateRight<T>(IList<T> list, int start, int end)
		{
			bool nothingToRotate = end - start <= 1;

			if (nothingToRotate) return;

			for (int i = end - 1; i > start; i--)
			{
				Algorithms.SwapAt(list, i, i - 1);
			}
		}

		public static void MergeSort_Queues<T>(IList<T> list) where T : IComparable<T>
		{
			void MergedFrontTwoElementsAndPutAtBack(ref Queue<T> target, Queue<Queue<T>> queues)
			{
				var queue1 = queues.Dequeue();
				var queue2 = queues.Dequeue();

				MergeQueues(target, queue1, queue2);
				queues.Enqueue(target);
				target = queue1; //reuse queue we don't need anymore

				Debug.Assert(queues.Count > 0);
			}

			void CopyToList(Queue<T> finalSortedQueue1)
			{
				for (int i = 0; i < list.Count; i++)
				{
					list[i] = finalSortedQueue1.Dequeue();
				}
			}

			Queue<Queue<T>> InitializeQueues()
			{
				var sortedQueues1 = new Queue<Queue<T>>();

				foreach (var item in list)
				{
					var queue = new Queue<T>();
					queue.Enqueue(item);
					sortedQueues1.Enqueue(queue);
				}

				return sortedQueues1;
			}

			static void MergeQueues(Queue<T> target, Queue<T> queue1, Queue<T> queue2)
			{
				Debug.Assert(target.Empty);

				while (true)
				{
					if (queue1.Empty)
					{
						if (queue2.Empty) return;

						target.Enqueue(queue2.Dequeue());
					}
					else if (queue2.Empty)
					{
						if (queue1.Empty) return;

						target.Enqueue(queue1.Dequeue());
					}
					else if (queue1.Peek.CompareTo(queue2.Peek) < 0)
					{
						target.Enqueue(queue1.Dequeue());
					}
					else
					{
						target.Enqueue(queue2.Dequeue());
					}
				}
			}

			if (list.Count <= 1)
			{
				return;
			}

			var sortedQueues = InitializeQueues();
			var mergedQueue = new Queue<T>();

			while (sortedQueues.Count > 1)
			{
				MergedFrontTwoElementsAndPutAtBack(ref mergedQueue, sortedQueues);
			}

			var finalSortedQueue = sortedQueues.Dequeue();

			CopyToList(finalSortedQueue);
		}

		public static void MergeInPlace<T>(IList<T> list, int start, int mid, int end) where T : IComparable<T>
		{
			AssertArgumentsValid(list, start, mid, end);

			int pointer0 = start;
			int pointer1 = mid;

			void TakeFromList1()
			{
				RotateRight(list, pointer0, pointer1 + 1);

				pointer0++;
				pointer1++;
			}

			void TakeFromList0() => pointer0++;

			void TakeNextSmallestElement()
			{
				if (SmallerThan(list[pointer1], list[pointer0]))
				{
					TakeFromList1();
				}
				else
				{
					TakeFromList0();
				}
			}

			bool SomethingLeftToMerge() => pointer0 < pointer1 && pointer1 < end;

			while (SomethingLeftToMerge())
			{
				TakeNextSmallestElement();
				AssertMergeLoopInvariant(list, start, pointer0, pointer1, end);
			}
		}

		[AssertionMethod]
		private static void AssertMergeLoopInvariant<T>(IList<T> list, int start, int pointer0, int pointer1, int end)
			where T : IComparable<T>
		{
			Debug.Assert(IsSorted(list, start, pointer0));
			Debug.Assert(IsSorted(list, pointer0, pointer1));
			Debug.Assert(IsSorted(list, pointer1, end));

			if (pointer0 > start && pointer1 < end)
			{
				Debug.Assert(SmallerThanOrEquals(list[pointer0 - 1], list[pointer0]));
				Debug.Assert(SmallerThanOrEquals(list[pointer0 - 1], list[pointer1]));
			}
		}

		[AssertionMethod]
		private static void AssertArgumentsValid<T>(IList<T> list, int start, int mid, int end) where T : IComparable<T>
		{
			bool IndexRelationsValid() => start <= mid && mid <= end;

			Debug.Assert(IndexRelationsValid());
			Debug.Assert(IsSorted(list, start, mid));
			Debug.Assert(IsSorted(list, mid, end));
		}

		public static bool IsSorted<T>(IList<T> list) where T : IComparable<T>
			=> IsSorted(list, 0, list.Count);

		public static bool IsSorted<T>(IList<T> list, int start0, int end0) where T : IComparable<T>
		{
			if (end0 - start0 <= 1)
			{
				return true;
			}

			for (int i = start0; i < end0 - 1; i++)
			{
				if (SmallerThan(list[i + 1], list[i]))
				{
					return false;
				}
			}

			return true;
		}

		public static void InsertionSort<T>(IList<T> list, int start, int end) where T : IComparable<T>
		{
			for (int i = start + 1; i < end; i++)
			{
				// Insert a[i] among a[i-1], a[i-2], a[i-3]... ..
				for (int j = i; j > start && SmallerThan(list[j], list[j - 1]); j--)
				{
					Algorithms.SwapAt(list, j, j - 1);
				}
			}
		}

		public static void InsertionSort<T>(IList<T> list) where T : IComparable<T>
			=> InsertionSort(list, 0, list.Count);

		public static void ShellSort<T>(IList<T> list) where T : IComparable<T>
			=> ShellSort(list, 0, list.Count);

		public static void ShellSort<T>(IList<T> list, int start, int end) where T : IComparable<T> 
		{
			
			// Sort a[] into increasing order.
			int count = end-start;
			int k = 0;

			while (CiuraSequence[k] < count)
			{
				k++;
			}

			k--;
			
			while (k >= 0)
			{
				int h = CiuraSequence[k];
				// h-sort the array.
				for (int i = start + h; i < end; i++)
				{ // Insert a[i] among a[i-h], a[i-2*h], a[i-3*h]... .
					for (int j = i; j >= (start + h) && SmallerThan(list[j], list[j - h]); j -= h)
					{
						Algorithms.SwapAt(list, j, j-h);
					}
				}
				
				k--;
			}
		}

		public static void SelectionSort<T>(IList<T> list) where T:IComparable<T>
		{
			void SwapAt(int a, int b)
			{
				if (a == b) return;
				
				(list[a], list[b]) = (list[b], list[a]);
			}
			
			void SwapSmallestAfterIndexToIndex(int index)
			{
				int indexOfSmallest = FindIndexOfMinimumAtOrAfter(index);
				SwapAt(index, indexOfSmallest);
			}

			int FindIndexOfMinimumAtOrAfter(int startIndex)
			{
				int indexOfMinimum = startIndex;
				
				void ReplaceIndexOfMinimumIfElementAtIsSmaller(int index)
				{
					if (SmallerThan(list[index], list[indexOfMinimum]))
					{
						indexOfMinimum = index;
					}
				}

				for (int j = startIndex + 1; j < list.Count; j++)
				{
					ReplaceIndexOfMinimumIfElementAtIsSmaller(j);
				}

				return indexOfMinimum;
			}
			
			bool alreadySorted = list.Count <= 1;
			
			if (alreadySorted) 
			{
				return; 
			}
			
			Debug.Assert(list.Count > 1);

			for (int i = 0; i < list.Count - 1; i++)
			{
				SwapSmallestAfterIndexToIndex(i);
			}
			
			Debug.Assert(IsSorted(list, 0, list.Count));
		}

		public static void SelectionSort_Inline<T>(IList<T> list) where T : IComparable<T>
		{
			SelectionSort_Inline(list, 0, list.Count);
		}

		public static void SelectionSort_Inline<T>(IList<T> list, int start, int end) where T:IComparable<T>
		{
			if (end - start <= 1) 
			{
				return; 
			}
			
			Debug.Assert(list.Count > 1);

			for (int i = start; i < end - 1; i++)
			{
				int indexOfMinimum = i;

				for (int j = i + 1; j < end; j++)
				{
					if (list[j].CompareTo(list[indexOfMinimum]) < 0)
					{
						indexOfMinimum = j;
					}
				}

				int indexOfSmallest = indexOfMinimum;
				
				if (i != indexOfSmallest)
				{
					(list[i], list[indexOfSmallest]) = (list[indexOfSmallest], list[i]);
				}
			}

			Debug.Assert(IsSorted(list, start, end));
		}

		public static void MergeSort_Recursive_InPlace<T>(IList<T> list) where T : IComparable<T>
		{
			void MergeSortRange(int start, int end)
			{
				bool alreadySorted = end - start <= 2;
				
				if (alreadySorted) return;
				
				int mid = start + (end - start) / 2;
					
				MergeSortRange(start, mid);
				MergeSortRange(mid, end);
				
				MergeInPlace(list, start, mid, end);
			}

			MergeSortRange(0, list.Count);
		}
		
		private static void MergeWithHelpList<T>(IList<T> listToMerge, int start, int mid, int end, IList<T> helpList) where T : IComparable<T>
		{ 
			int listIndex1 = start;
			int listIndex2 = mid;
				
			Copy(listToMerge, helpList, start, end);
				
			for (int k = start; k < end; k++)
			{
				if (listIndex1 >= mid)
				{
					listToMerge[k] = helpList[listIndex2++];
				}
				else if (listIndex2 >= end)
				{
					listToMerge[k] = helpList[listIndex1++];
				}
				else if (SmallerThan(helpList[listIndex2], helpList[listIndex1]))
				{
					listToMerge[k] = helpList[listIndex2++];
				}
				else
				{
					listToMerge[k] = helpList[listIndex1++];
				}
			}
		}

		private static void Copy<T>(IList<T> source, T[] destination, int start, int end)
		{
			for (int i = start; i < end; i++)
			{
				destination[i] = source[i];
			}
		}
		
		private static void MergeWithHelpList<T>(IList<T> listToMerge, int start, int mid, int end, T[] helpList) where T : IComparable<T>
		{ 
			int listIndex1 = start;
			int listIndex2 = mid;
				
			Copy(listToMerge, helpList, start, end);
				
			for (int k = start; k < end; k++)
			{
				if (listIndex1 >= mid)
				{
					listToMerge[k] = helpList[listIndex2++];
				}
				else if (listIndex2 >= end)
				{
					listToMerge[k] = helpList[listIndex1++];
				}
				else if (SmallerThan(helpList[listIndex2], helpList[listIndex1]))
				{
					listToMerge[k] = helpList[listIndex2++];
				}
				else
				{
					listToMerge[k] = helpList[listIndex1++];
				}
			}
		}
		
		public static void MergeSort_Recursive_NewList<T>(IList<T> list) where T : IComparable<T>
		{
			var helpList = new T[list.Count];
			
			void Sort(IList<T> listToSort, int start, int end)
			{
				if (end < start + 2)
				{
					return;
				}
				
				int mid = start + (end - start)/2;
				
				Sort(listToSort, start, mid); 
				Sort(listToSort, mid, end); 
				MergeWithHelpList(listToSort, start, mid, end, helpList);
			}
			
			Sort(list, 0, list.Count);
		}
		
		public static void MergeSort_Recursive_NewList_Smart<T>(IList<T> list) where T : IComparable<T>
		{
			var helpList = new T[list.Count];
			const int minimumListCount = 26;
			
			void Sort(IList<T> a, int start, int end)
			{
				if (end - start <= minimumListCount)
				{
					ShellSort(list, start, end);
					return;
				}

				int mid = start + (end - start)/2;
				
				Sort(a, start, mid); 
				Sort(a, mid, end);
				MergeWithHelpList(a, start, mid, end, helpList);
			}
			
			Sort(list, 0, list.Count);
		}
		
		public static void MergeSort_Iterative_InPlace<T>(IList<T> list) where T : IComparable<T>
		{
			int count = list.Count;
			
			void MergePieces(int pieceLength)
			{
				for (int j = 0; j < count; j += pieceLength)
				{
					int start = j;
					int end = j + pieceLength;
					int mid = Math.Min((start + end) / 2, count);
					end = Math.Min(end, count);

					MergeInPlace(list, start, mid, end);
				}
			}

			for (int pieceLength = 2; pieceLength < 2 * count; pieceLength *= 2)
			{
				MergePieces(pieceLength);
			}
			
			Debug.Assert(IsSorted(list, 0, list.Count));
		}

		public static void MergeSort_Iterative_NewList<T>(IList<T> list) where T : IComparable<T>
			=> MergeSort_Iterative_NewList(list, 0, list.Count);
		
		public static void MergeSort_Iterative_NewList<T>(IList<T> list, PerformanceMonitor monitor, PerformanceData data) where T : IComparable<T>
			=> MergeSort_Iterative_NewList(list, 0, list.Count, monitor, data);
		
		private static void MergeSort_Iterative_NewList<T>(IList<T> list, int sortStart, int sortEnd) where T : IComparable<T>
		{
			int sortCount = sortEnd - sortStart;
			var helpList = new T[list.Count]; //This is bigger than necessary
			
			void MergePieces(int pieceLength)
			{
				for (int j = sortStart; j < sortEnd; j += pieceLength)
				{
					int start = j;
					int end = j + pieceLength;
					int mid = Math.Min((start + end) / 2, sortEnd);
					end = Math.Min(end, sortEnd);

					if (end > mid && mid > start)
					{
						MergeWithHelpList(list, start, mid, end, helpList);
					}
				}
			}

			for (int pieceLength = 2; pieceLength < 2 * sortCount; pieceLength *= 2)
			{
				MergePieces(pieceLength);
			}
			
			Debug.Assert(IsSorted(list, 0, list.Count));
		}
		
		private static void MergeSort_Iterative_NewList<T>(
			IList<T> list,
			int sortStart, 
			int sortEnd,
			PerformanceMonitor monitor, 
			PerformanceData data) where T : IComparable<T>
		{
			const int minimumCount = WhenToSwitch_ForPerformanceMeasurements;
			int sortCount = sortEnd - sortStart;
			var helpList = new T[list.Count]; //This is bigger than necessary

			monitor.Reset();
			monitor.Watch("Small");
			
			void MergePieces(int pieceLength)
			{
				for (int j = sortStart; j < sortEnd; j += pieceLength)
				{
					int start = j;
					int end = j + pieceLength;
					int mid = Math.Min((start + end) / 2, sortEnd);
					end = Math.Min(end, sortEnd);

					if (end > mid && mid > start)
					{
						MergeWithHelpList(list, start, mid, end, helpList);
					}
				}
			}

			for (int pieceLength = 2; pieceLength < 2 * sortCount; pieceLength *= 2)
			{
				if (pieceLength == 2 * minimumCount)
				{
					monitor.Watch("Big");
				}

				if (pieceLength >= 2 * minimumCount)
				{
					data.Counts["Big"]++;
				}
				
				MergePieces(pieceLength);
			}
			
			monitor.Stop();
			monitor.SetPerformanceData(data);
			
			Debug.Assert(IsSorted(list, 0, list.Count));
		}

		public static void MergeSort_Iterative_NewList_Smart<T>(IList<T> list) where T : IComparable<T>
			=> MergeSort_Iterative_NewList_Smart(list, 0, list.Count);
		
		public static void MergeSort_Iterative_NewList_Smart<T>(IList<T> list, PerformanceMonitor monitor, PerformanceData data) where T : IComparable<T>
			=> MergeSort_Iterative_NewList_Smart(list, 0, list.Count, monitor, data);
		
		
		private static void MergeSort_Iterative_NewList_Smart<T>(IList<T> list, int sortStart, int sortEnd) where T : IComparable<T>
		{
			//Console.WriteLine(list.ToPrettyString());
			const int minimumCount = 10;
			int sortCount = sortEnd - sortStart;
			var helpList = new T[list.Count];//This is bigger than necessary
			
			void MergePieces(int pieceLength)
			{
				for (int j = sortStart; j < sortEnd; j += pieceLength)
				{
					int start = j;
					int end = j + pieceLength;
					int mid = Math.Min((start + end) / 2, sortEnd);
					end = Math.Min(end, sortEnd);

					if (end > mid && mid > start)
					{
						MergeWithHelpList(list, start, mid, end, helpList);
						//Console.WriteLine(list.ToPrettyString());
					}
				}
			}
			
			void SortPieces(int pieceLength)
			{
				for (int j = sortStart; j < sortEnd; j += pieceLength)
				{
					int start = j;
					int end = j + pieceLength;
					end = Math.Min(end, sortEnd);
					InsertionSort(list, start, end);
					
					Debug.Assert(IsSorted(list, start, end));
					//Console.WriteLine(list.ToPrettyString());
				}
			}

			SortPieces(minimumCount);

			for (int pieceLength = 2*minimumCount; pieceLength < 2 * sortCount; pieceLength *= 2)
			{
				MergePieces(pieceLength);
			}
			
			Debug.Assert(IsSorted(list, 0, list.Count));
		}
		
		private static void MergeSort_Iterative_NewList_Smart<T>(
			IList<T> list,
			int sortStart,
			int sortEnd,
			PerformanceMonitor monitor,
			PerformanceData data) where T : IComparable<T>
		{
			//Console.WriteLine(list.ToPrettyString());
			const int minimumCount = WhenToSwitch_ForPerformanceMeasurements;
			int sortCount = sortEnd - sortStart;
			var helpList = new T[list.Count];//This is bigger than necessary
			
			void MergePieces(int pieceLength)
			{
				for (int j = sortStart; j < sortEnd; j += pieceLength)
				{
					int start = j;
					int end = j + pieceLength;
					int mid = Math.Min((start + end) / 2, sortEnd);
					end = Math.Min(end, sortEnd);

					if (end > mid && mid > start)
					{
						MergeWithHelpList(list, start, mid, end, helpList);
						//Console.WriteLine(list.ToPrettyString());
					}
				}
			}
			
			void SortPieces(int pieceLength)
			{
				for (int j = sortStart; j < sortEnd; j += pieceLength)
				{
					int start = j;
					int end = j + pieceLength;
					end = Math.Min(end, sortEnd);
					ShellSort(list, start, end);
					
					Debug.Assert(IsSorted(list, start, end));
					//Console.WriteLine(list.ToPrettyString());
				}
			}

			monitor.Reset();
			monitor.Watch("Small");
			SortPieces(minimumCount);
			monitor.Watch("Big");
			
			for (int pieceLength = 2*minimumCount; pieceLength < 2 * sortCount; pieceLength *= 2)
			{
				MergePieces(pieceLength);
				data.Counts["Big"]++;
			}
			
			monitor.Stop();
			monitor.SetPerformanceData(data);
			Debug.Assert(IsSorted(list, 0, list.Count));
		}
		
		public static void MergeSort_Iterative_InPlace_Smart<T>(IList<T> list) where T : IComparable<T>
		{
			const int minimumWindowSize = 26;
			int count = list.Count;
			
			void MergePieces(int pieceLength)
			{
				for (int j = 0; j < count; j += pieceLength)
				{
					int start = j;
					int end = j + pieceLength;
					int mid = Math.Min((start + end) / 2, count);
					end = Math.Min(end, count);

					MergeInPlace(list, start, mid, end);
				}
			}
			
			void SortPieces(int pieceLength)
			{
				for (int j = 0; j < count; j += pieceLength)
				{
					int start = j;
					int end = j + pieceLength;
					end = Math.Min(end, count);
					
					ShellSort(list, start, end);
					
					Debug.Assert(IsSorted(list, start, end));
				}
			}

			SortPieces(minimumWindowSize);
			
			for (int pieceLength = minimumWindowSize * 2; pieceLength < 2*count; pieceLength *= 2)
			{
				MergePieces(pieceLength);
			}

			Debug.Assert(IsSorted(list, 0, count));
		}
		
		private static void ThrowIfReached() => Debug.Assert(false, "Unreachable");
		
		private static bool SmallerThan<T>(T item1, T item2) where T : IComparable<T>
			=> item1.CompareTo(item2) < 0;
		
		private static bool SmallerThanOrEquals<T>(T item1, T item2) where T : IComparable<T>
			=> item1.CompareTo(item2) <= 0;
		
		private static void Copy<T>(IList<T> source, IList<T> destination, int start, int end)
		{
			for (int i = start; i < end; i++)
			{
				destination[i] = source[i];
			}
		}
		
		private static IList<T> GetEmptyList<T>(int count) where T : IComparable<T>
		{
			var sortedList = new List<T>(count);

			for (int i = 0; i < count; i++)
			{
				sortedList.Add(default);
			}

			return sortedList;
		}
	}
}