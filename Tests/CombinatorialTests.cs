using System;
using Gamelogic.Extensions.Algorithms;
using NUnit.Framework;

namespace Tests
{
	[Parallelizable(ParallelScope.All), TestOf(nameof(Combinatorial))]
	public class CombinatorialTests
	{
		private enum Animal
		{
			Ant,
			Bird, 
			Cat, 
			Dog,
			Elephant
		}
		
		private static readonly TestCaseData[] IntegerTupleTestCases =
		{
			new (new []{1}, new []{new []{0}}),
			
			new (new []{3}, new []
			{
				new []{0},
				new []{1},
				new []{2}
			}),
			
			new (new []{2, 3}, new []
			{
				new[]{0, 0},
				new[]{0, 1},
				new[]{0, 2},
				new[]{1, 0},
				new[]{1, 1},
				new[]{1, 2},
			})
		};
		
		[Test, TestCaseSource(nameof(IntegerTupleTestCases))]
		public void MultiRadixTuples(int[] input, int[][] output)
		{
			Assert.That(Combinatorial.MultiRadixTuples(input), Is.EquivalentTo(output));
		}

		[Test]
		public void TestThrowIfZeroRadix1()
		{
			var input = new [] {0};
			void Test() => Combinatorial.MultiRadixTuples(input);
			
			Assert.That(Test, Throws.TypeOf<ArgumentOutOfRangeException>());
		}
		
		[Test]
		public void TestThrowIfZeroRadix2()
		{
			var input = new [] {1, 0, 1};
			void Test() => Combinatorial.MultiRadixTuples(input);
			
			Assert.That(Test, Throws.TypeOf<ArgumentOutOfRangeException>());
		}
	}
}