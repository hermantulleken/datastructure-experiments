using DataStructures;
using NUnit.Framework;

namespace Tests
{
	[Parallelizable, TestOf(nameof(BinarySearchTree<int>))]
	public class BinaryTreeTests
	{
		[Test]
		public void AddContainsInvariant()
		{
			var tree = new BinarySearchTree<string>
			{
				[3] = "three", 
				[5] = "five", 
				[1] = "one"
			};

			Assert.That(tree, Contains.Key(3));
		}
	}
}