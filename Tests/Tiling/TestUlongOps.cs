using System;
using DataStructures.Tiling;
using NUnit.Framework;

namespace Tests.Tiling;

[TestFixture]
public class TestUlongOps
{
	[Test]
	public void TestMostSignificantBit()
	{
		int pos = 5;
		ulong n = 7ul << pos;
		int leastSignificantBit = UlongOps.MostSignificantBit(n);
		Assert.That(leastSignificantBit, Is.EqualTo(pos + 3 - 1));//3, since 7ul has three bits set in sequence
	}
	
	[Test]
	public void TestLeasSignificantBit()
	{
		int pos = 5;
		ulong n = 7ul << pos;
		int leastSignificantBit = UlongOps.LeastSignificantBit(n);
		Assert.That(leastSignificantBit, Is.EqualTo(pos));
	}
	
	[Test]
	public void TestFindZeroPosition()
	{
		int pos = 5;
		ulong n = ulong.MaxValue ^ (1ul << pos);
		/*
		string str = Convert.ToString((long) n, 2);
		ulong nn = ~n;
		string s2 = Convert.ToString((long) nn, 2);
		int leadingZeros = BitOperations.LeadingZeroCount(nn);
		int trailingZero = BitOperations.TrailingZeroCount(nn);

		int m1 = UlongOps.MostSignificantBit(nn);
		int m2 = UlongOps.LeastSignificantBit(nn);
		*/
		Assert.That(UlongOps.FindZeroPosition(n), Is.EqualTo(pos));
	}
	
	[Test]
	public void TestFindZeroPosition2()
	{
		int pos = 5;
		ulong n = ulong.MaxValue ^ (5ul << pos);
		Assert.That(UlongOps.FindZeroPosition(n), Is.EqualTo(pos));
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
		bool inRange = UlongOps.InRange(source.row, source.offset, source.width);
		Assert.That(inRange, Is.EqualTo(source.inRange));
	}

	[Test]
	public void TestReverseBits()
	{
		ulong bb = 0b0000000000000000000000000000000000000000000000000000000000000000;
		ulong nn = 0b1111111111111111111111111111111111111111111111111111111111111111;
		ulong n1 = 0b1010111101;
		ulong n2 = 0b1011110101000000000000000000000000000000000000000000000000000000;

		string s1 = Convert.ToString((long) n1, 2);
		string s2 = Convert.ToString((long) n2, 2);

		ulong reverse = UlongOps.ReverseBits(n1);
		string sr =  Convert.ToString((long) reverse, 2);
		
		Assert.That(reverse, Is.EqualTo(n2));
	}
}
