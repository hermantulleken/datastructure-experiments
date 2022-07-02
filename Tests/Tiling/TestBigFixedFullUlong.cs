using DataStructures;
using NUnit.Framework;

namespace Tests.Tiling;

[TestFixture, Parallelizable]
public class TestBigFixedFullUlong
{
	[Test]
	public void TestEqualsTrue()
	{
		var n1 = new BigFixedFullUlong(new[] { 12345ul, 67890ul });
		var n2 = new BigFixedFullUlong(new[] { 12345ul, 67890ul });

		Assert.That(n1 == n2, Is.True);
	}

	[Test]
	public void TestEqualsFalse1()
	{
		var n1 = new BigFixedFullUlong(new[] { 1234ul, 67890ul });
		var n2 = new BigFixedFullUlong(new[] { 12345ul, 67890ul });

		Assert.That(n1 == n2, Is.False);
	}

	[Test]
	public void TestEqualsFalse2()
	{
		var n1 = new BigFixedFullUlong(new[] { 12345ul, 6789ul });
		var n2 = new BigFixedFullUlong(new[] { 12345ul, 67890ul });

		Assert.That(n1 == n2, Is.False);
	}

	[Test]
	public void TestAnd()
	{
		var n1 = new BigFixedFullUlong(new[] { 0b10111ul, 0b1101011ul });
		var n2 = new BigFixedFullUlong(new[] { 0b01110ul, 0b1010110ul });
		var n3 = new BigFixedFullUlong(new[] { 0b00110ul, 0b1000010ul });

		Assert.That(n1 & n2, Is.EqualTo(n3));
	}

	[Test]
	public void TestOr()
	{
		var n1 = new BigFixedFullUlong(new[] { 0b10111ul, 0b1101011ul });
		var n2 = new BigFixedFullUlong(new[] { 0b01110ul, 0b1010110ul });
		var n3 = new BigFixedFullUlong(new[] { 0b11111ul, 0b1111111ul });

		Assert.That(n1 | n2, Is.EqualTo(n3));
	}

	[Test]
	public void TestXor()
	{
		var n1 = new BigFixedFullUlong(new[] { 0b10111ul, 0b1101011ul });
		var n2 = new BigFixedFullUlong(new[] { 0b01110ul, 0b1010110ul });
		var n3 = new BigFixedFullUlong(new[] { 0b11001ul, 0b0111101ul });

		Assert.That(n1 ^ n2, Is.EqualTo(n3));
	}

	[Test]
	public void LeastSignificantMask()
	{
		ulong actual = BigFixedFullUlong.LeastSignificantMask(3);
		const ulong expected = 7ul;
		
		Assert.That(actual, Is.EqualTo(expected));
	}
	
	[Test]
	public void MostSignificantMask()
	{
		ulong actual = BigFixedFullUlong.MostSignificantMask(3);
		const ulong expected = 7ul << 61;
		
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestToString()
	{
		var n = new BigFixedFullUlong(new[] { 5ul, 9ul });
		string actual = n.ToString();
		const string expected = "00000000000000000000000000000000000000000000000000000000000010010000000000000000000000000000000000000000000000000000000000000101";

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestLeftShift()
	{
		var n = new BigFixedFullUlong(new[] { 5ul | (1ul << 63), 9ul });
		var actual = n << 3;

		var expected = new BigFixedFullUlong(new[] { 40ul, 72ul | 4ul });
		
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestRightShift()
	{
		var n = new BigFixedFullUlong(new[] { 9ul, 5ul + (1ul << 63)});
		var actual = n >> 3;

		var expected = new BigFixedFullUlong(new[] { 1ul | (5ul << 61), 1ul << 60 });
		
		Assert.That(actual, Is.EqualTo(expected));
	}
}
