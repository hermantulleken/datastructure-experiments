using DataStructures.Tiling;
using NUnit.Framework;

namespace Tests.Tiling;

[TestFixture]
public class TestULongOps
{
	[Test]
	public void TestFindZeroPosition()
	{
		int pos = 5;
		var n = ulong.MaxValue ^ (1ul << pos);
		
		Assert.That(ULongOps.FindZeroPosition(n), Is.EqualTo(pos));
	}
	
	[DatapointSource]
	private static readonly (ulong row, int offset , int width, bool inRange)[] RangeTestData =
	{
		(7ul << 3, -3, 3, true),
		(7ul << 3, -4, 3, false),
		(7ul << 3, -3, 2, false),
		(7ul << 3, 58, 64, true),
		(7ul << 3, 59, 64, false),
		(7ul << 3, 3, 9, true),
		(7ul << 3, 4, 9, false),
		(7ul << 3, 0, 6, true),
	};
	
	[Theory]
	public void TestInRange((ulong row, int offset , int width, bool inRange) source)
	{
		bool inRange = ULongOps.InRange(source.row, source.offset, source.width);
		Assert.That(inRange, Is.EqualTo(source.inRange));
	}
}
