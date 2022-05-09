using DataStructures;
using NUnit.Framework;

namespace Tests;

[TestOf(nameof(SquarePotQuadtree<int>))]
public class PotQuadTreeTests
{
	private const int Size = 1 << 3;

	[Test]
	public void TestIndexesInRange()
	{
		var quadTree = new SquarePotQuadtree<int>(Size, 5);

		foreach (var index in quadTree.Indexes)
		{
			Assert.That(InRange(index.x, 0, Size));
		}
	}
	
	[Test]
	public void TestIndexesInSequence()
	{
		var quadTree = new SquarePotQuadtree<int>(Size, 5);
		(int, int)? previousIndex = null;

		foreach (var index in quadTree.Indexes)
		{
			if (previousIndex.HasValue)
			{
				Assert.That(InSequence(previousIndex.Value, index, Size));
			}
			
			previousIndex = index;
		}
	}

	[Test]
	public void TestStructureStringLeaf()
	{
		var quadTree = new SquarePotQuadtree<int>(2);
		
		Assert.That(quadTree.ToStructureString(), Is.EqualTo("."));
	}
	
	[Test]
	public void TestStructureStringInternal()
	{
		var quadTree = new SquarePotQuadtree<int>(2)
		{
			[0, 0] = 1
		};

		Assert.That(quadTree.ToStructureString(), Is.EqualTo("[....]"));
	}
	
	[Test]
	public void TestStructureStringLevels()
	{
		var quadTree = new SquarePotQuadtree<int>(Size)
		{
			[0, 0] = 1
		};

		Assert.That(quadTree.ToStructureString(), Is.EqualTo("[[[....]...]...]"));
	}
	
	[Test]
	public void TestReconnect()
	{
		var quadTree = new SquarePotQuadtree<int>(Size)
		{
			[0, 0] = 1
		};

		quadTree[0, 0] = 0;
		
		Assert.That(quadTree.ToStructureString(), Is.EqualTo("."));
	}

	private static bool InSequence((int x, int y) index1, (int x, int y) index2, int size)
		=> index1.x == size - 1
			? (index2.x == 0 && index2.y == index1.y + 1)
			: (index2.x == index1.x + 1 && index2.y == index1.y);
	
	private static bool InRange(int value, int min, int max) => value >= min && value < max;
}
