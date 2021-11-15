using Gamelogic.Extensions;
using NUnit.Framework;

namespace Tests
{
	[Parallelizable(ParallelScope.All), TestOf(nameof(DebugEnumerableExtensions))]
	public class EnumerableExtensionsTests
	{
		private static readonly int[] Empty = System.Array.Empty<int>();
		private static readonly int[] SimpleArray1 = {0, 1, 2};
		private static readonly int[] SimpleArray2 = {3, 4};
		private static readonly int[] Single = {5};
		
		private static readonly int[][] Nested = 
		{
			SimpleArray1,
			SimpleArray2,
			Single
		};

		private static readonly int[][][] DoublyNested =
		{
			Nested,
			new []{ new []{6, 7, 8}}
		};
		
		[Test]
		public void TestEmpty() => Assert.That(Empty.ToPrettyString(), Is.EqualTo("[]"));
		
		[Test]
		public void TestSingle() => Assert.That(Single.ToPrettyString(), Is.EqualTo("[5]"));
		
		[Test]
		public void TestSimple() => Assert.That(SimpleArray1.ToPrettyString(), Is.EqualTo("[0, 1, 2]"));
		
		[Test]
		public void TestNested() => Assert.That(Nested.ToPrettyString(), Is.EqualTo("[[0, 1, 2], [3, 4], [5]]"));
		
		[Test]
		public void TestDoublyNested() => Assert.That(DoublyNested.ToPrettyString(), Is.EqualTo("[[[0, 1, 2], [3, 4], [5]], [[6, 7, 8]]]"));
	}
	
}