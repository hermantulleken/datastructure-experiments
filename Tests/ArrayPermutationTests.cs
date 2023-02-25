using DataStructures.SpatialPartitioning;
using NUnit.Framework;

namespace Tests;

[Parallelizable, TestOf(nameof(ArrayPermutationTests))]
public class ArrayPermutationTests
{
	[DatapointSource] 
	public static readonly ArrayPermutation[] Permutations =
	{
		ArrayPermutation.FromPermutationMap(0, 1, 2, 3),
		ArrayPermutation.FromPermutationMap(1, 0, 2, 3),
		ArrayPermutation.FromPermutationMap(3, 1, 2, 0),
		ArrayPermutation.FromPermutationMap(3, 2, 1, 0)
	};

	public static readonly int[] Ranks = { 0, 6, 21, 23 };
	
	[Test]
	public void TestSet()
	{
		var permutation = ArrayPermutation.FromPermutationMap(3, 2, 1, 0);
		
		AssertElements(permutation, 3, 2, 1, 0);
	}

	[Test]
	public void TestEquals()
	{
		var permutation = ArrayPermutation.FromPermutationMap(3, 2, 1, 0);
		
		var permutation1 = ArrayPermutation.FromPermutationMap(3, 2, 1, 0);
		var permutation2 = ArrayPermutation.FromPermutationMap(3, 2, 1, 0, 4);
		var permutation3 = ArrayPermutation.FromPermutationMap(2, 1, 0);
		var permutation4 = ArrayPermutation.FromPermutationMap(3, 1, 2, 0);
		
		Assert.That(permutation.Equivalent(permutation1));
		Assert.That(!permutation.Equivalent(permutation2));
		Assert.That(!permutation.Equivalent(permutation3));
		Assert.That(!permutation.Equivalent(permutation4));
		Assert.That(!permutation.Equivalent(null));
	}

	[Test]
	public void TestIdentity()
	{
		var permutation = ArrayPermutation.Identity(4);
		
		AssertElements(permutation, 0, 1, 2, 3);
	}

	[Test]
	public void TestInverseInvariant()
	{
		var permutation = ArrayPermutation.FromPermutationMap(3, 2, 1, 0, 4);
		var inverse = permutation.Inverse();
		var result = permutation.Compose(inverse);
		var identity = ArrayPermutation.Identity(permutation.SetCount);
		
		Assert.That(result.Equivalent(identity));
	}

	[Test]
	public void TestMul()
	{
		var permutation1 = ArrayPermutation.FromCycle(5, 1, 2, 3);
		var permutation2 = ArrayPermutation.FromCycle(5, 3, 4);
		var result = permutation1.Compose(permutation2);
		
		AssertElements(result, 0, 2, 4, 1, 3);
	}

	[Test]
	public void TestFluidConstruction()
	{
		var permutation = ArrayPermutation
			.FromCycle(5, 0, 1)
			.MulCycle(1, 2)
			.MulCycle(2, 3);
		
		AssertElements(permutation, 3, 0, 1, 2, 4 );
	}

	[Theory]
	public void TestLexicographicalRankInvariant(ArrayPermutation permutation)
	{
		int rank = permutation.LexicographicalRank();
		Assert.That(permutation.Equivalent(ArrayPermutation.FromLexicographicalRank(permutation.SetCount, rank)));
	}

	[Theory]
	public void TestIdentityInvariant(ArrayPermutation permutation)
	{
		var identity = ArrayPermutation.Identity(permutation.SetCount);
		var permutation1 = permutation.Compose(identity);
		Assert.That(permutation.Equivalent(permutation1));
		
		var permutation2 = identity.Compose(permutation);
		Assert.That(permutation.Equivalent(permutation2));
		
	}

	[Theory]
	public void TestInverseInvariant(ArrayPermutation permutation)
	{
		var permutation1 = permutation.Inverse().Inverse();
		Assert.That(permutation.Equivalent(permutation1));
	}
	
	[Test, Sequential]
	public void TestRank(
		[ValueSource(nameof(Permutations))]ArrayPermutation permutation,
		[ValueSource(nameof(Ranks))] int rank)
	{
		Assert.That(permutation.LexicographicalRank(), Is.EqualTo(rank));
	}

	private static void AssertElements(IPermutation permutation, params int[] elements)
	{
		Assert.That(permutation.SetCount, Is.EqualTo(elements.Length));
		
		for (int i = 0; i < permutation.SetCount; i++)
		{
			Assert.That(permutation[i], Is.EqualTo(elements[i]));
		}
	}
}
