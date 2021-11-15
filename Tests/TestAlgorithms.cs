using System.Collections.Generic;
using DataStructures;
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
	public class TestAlgorithms
	{
		[Test]
		public void TestSwapInt()
		{
			int a = 4;
			int b = 3;
			
			Algorithms.Swap(ref a, ref b);
			
			Assert.That(a == 3);
			Assert.That(b == 4);
		}
		
		[Test]
		public void TestSwapObject()
		{
			var a = new object();
			var a0 = a;
			
			var b = new object();
			var b0 = b;
			
			Algorithms.Swap(ref a, ref b);
			
			Assert.That(a == b0);
			Assert.That(b == a0);
		}

		[Test]
		public void TestSwapAt()
		{
			var list = new List<int> {0, 1, 2, 3, 4};
			
			Algorithms.SwapAt(list, 3, 4);
			Assert.That(list[3] == 4);
			Assert.That(list[4] == 3);
		}
	}
}