using System;
using System.Collections.Generic;
using DataStructures;
using NUnit.Framework;

namespace Tests;

[TestFixture, TestOf(typeof(MergeSort))]
public class TestMergeSort
{
	private readonly record struct SortableItemTracker(int Key, int OriginalOrder);

	private class KeyComparer : IComparer<SortableItemTracker>
	{
		public int Compare(SortableItemTracker x, SortableItemTracker y) => x.Key - y.Key;
	}

	private class KeyOrderComparer : IComparer<SortableItemTracker>
	{
		public int Compare(SortableItemTracker x, SortableItemTracker y) =>
			x.Key == y.Key
				? x.OriginalOrder - y.OriginalOrder
				: x.Key - y.Key;
	}

	private static List<int>[] sortedLists = 
	{
		new(),
		new() {7},
		new() {7, 7, 7, 7},
		new() {0, 1, 2, 3, 4},
		new() {0, 0, 1, 2, 3},
		new() {0, 11, 23, 45, 67, 89},
		new() {0, 11, 23, 23, 67, 67, 67, 99}
	};

	private static List<int>[] unsortedLists =
	{
		new(){1, 0},
		new(){0, 1, 2, 3, 4, 5, 6, 7, 6}
	};
	
	[Test]
	[TestCaseSource(nameof(sortedLists))]
	public static void TestIsSortedOnSortedList(IList<int> list) => TestIsSorted(list, true);

	[Test]
	[TestCaseSource(nameof(unsortedLists))]
	public static void TestIsSortedOnUnsortedList(IList<int> list) => TestIsSorted(list, false);

	[Test]
	public static void TestNullThrows()
	{
		Assert.Throws<ArgumentNullException>(() => MergeSort.IsSorted<int>(null!));
	}
	
	private static void TestIsSorted(IList<int> list, bool isSorted) => Assert.That(MergeSort.IsSorted(list), Is.EqualTo(isSorted));
}
