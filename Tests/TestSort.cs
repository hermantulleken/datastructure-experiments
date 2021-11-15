using System;
using System.Collections.Generic;
using System.Linq;
using DataStructures;
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
	public class TestSort
	{
		private static List<int>[] sortedLists = 
		{
			new(),
			new() {0, 1, 2, 3, 4},
			new() {0, 0, 1, 2, 3},
			new() {0, 0, 0, 0, 0},
			new() {0, 11, 23, 45, 67, 89},
			new() {0, 11, 23, 23, 67, 67, 67, 99}
		};

		private static List<int>[] unsortedLists =
		{
			new(){1, 0},
			new(){0, 1, 2, 3, 4, 5, 6, 7, 6}
		};

		//private static List<int> randomList = new() {6, 6, 8, 6, 9, 1, 0, 4, 7, 2, 5, 3, 6};
		
		private static readonly List<int> RandomList = DataStructures.RandomList.New(1000).Take(1000).ToList();
		
		//private static List<int> randomList = new() {6, 8, 6};
		
		[DatapointSource]
		private static Action<IList<int>>[] algorithms =
		{
			Sort.SelectionSort,
			Sort.SelectionSort_Inline,
			Sort.InsertionSort,
			Sort.ShellSort,
			Sort.MergeSort_Queues,
			Sort.MergeSort_Iterative_InPlace,
			Sort.MergeSort_Iterative_InPlace_Smart,
			Sort.MergeSort_Iterative_NewList,
			Sort.MergeSort_Iterative_NewList_Smart,
			Sort.MergeSort_Recursive_NewList,
			Sort.MergeSort_Recursive_NewList_Smart,
		};

		[Test]
		[TestCaseSource(nameof(sortedLists))]
		public static void TestIsSortedOnSortedList(IList<int> list)
		{
			TestIsSorted(list, true);
		}
		
		[Test]
		[TestCaseSource(nameof(unsortedLists))]
		public static void TestIsSortedOnUnsortedList(IList<int> list)
		{
			TestIsSorted(list, false);
		}

		[Theory]
		public static void TestSortAlgorithms(Action<IList<int>> algorithm)
		{
			var copiedList = RandomList.ToList();

			algorithm(copiedList);
			
			Assert.That(Sort.IsSorted(RandomList), Is.False);
			Assert.That(Sort.IsSorted(copiedList), Is.True);
		}

		[Theory]
		public static void TestSortAlgorithmsAreEquivalent(Action<IList<int>> algorithm)
		{
			var list = RandomList.ToList();
			var benchmark = RandomList.ToList();
			
			//make sure the objects are different
			Assert.That(list != RandomList);
			Assert.That(benchmark != RandomList);

			algorithm(list);
			Sort.SelectionSort(benchmark);
			
			Assert.IsTrue(list.SequenceEqual(benchmark));
			//Assert.IsFalse(copiedList1.SequenceEqual(randomList));
		}

		[Test]
		public static void Test1()
		{
			TestSortAlgorithmsAreEquivalent(Sort.MergeSort_Queues);
		}

		[Test]
		public static void TestShellSortRange()
		{
			var list1 = RandomList.ToList();
			var list2 = RandomList.ToList();

			list1 = Algorithms.Range(list1, 33, 56).ToList();
			
			Sort.ShellSort(list1);
			Sort.ShellSort(list2, 33, 56);
			list2 = Algorithms.Range(list2, 33, 56).ToList();
			
			Assert.IsTrue(list1.SequenceEqual(list2));

		}

		private static void TestIsSorted(IList<int> list, bool isSorted)
		{
			Assert.That(Sort.IsSorted(list), Is.EqualTo(isSorted));
		}
	}
}