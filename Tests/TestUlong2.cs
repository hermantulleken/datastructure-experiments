using DataStructures.Tiling;
using NUnit.Framework;

namespace Tests;

public class TestUlong2
{
	[Test]
	public void TestNewConstruction()
	{
		var expected = new Ulong2<Width65>((1ul << 63) | 1ul, 1ul);
		var actual = Ulong2.FromSetBits<Width65>(0, 63, 64);
		
		Assert.That(actual, Is.EqualTo(expected));
	}
	
	[Test]
	public void TestLeftShift()
	{
		var actual = Ulong2.FromSetBits<Width65>(0, 63, 64) << 1;
		var expected = Ulong2.FromSetBits<Width65>(1, 64);
		
		Assert.That(actual, Is.EqualTo(expected));
	}
	
	[Test]
	public void TestRightShift()
	{
		var actual = Ulong2.FromSetBits<Width65>(0, 63, 64) >> 1;
		var expected = Ulong2.FromSetBits<Width65>(62, 63);
		
		Assert.That(actual, Is.EqualTo(expected));
	}
	
	[Test]
	public void TestLeftShift64()
	{
		var actual = Ulong2.FromSetBits<Width65>(0, 63, 64) << 64;
		var expected = Ulong2.FromSetBits<Width65>(64);
		
		Assert.That(actual, Is.EqualTo(expected));
	}
	
	[Test]
	public void TestRightShift64()
	{
		var actual = Ulong2.FromSetBits<Width65>(0, 63, 64) >> 64;
		var expected = Ulong2.FromSetBits<Width65>(0);
		
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void LeastSignificantZeros1([Values(4, 64)] int expected)
	{
		var n = Ulong2.FromSetBits<Width65>(expected);
		int actual = n.LeastSignificantZeroCount();
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void MostSignificantZeros1([Values(0, 45)] int expected)
	{
		var n = Ulong2.FromSetBits<Width65>(65- 1 - expected);
		int actual = n.MostSignificantZeroCount();
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestMostAndLEastSignificantBit1()
	{
		const int expectedMost = 56;
		const int expectedLeast = 5;

		var n = Ulong2.FromSetBits<Width65>(expectedLeast, expectedMost);
		int actualLeast = n.LeastSignificantBit();
		int actualMost = n.MostSignificantBit();
		
		Assert.That(actualLeast, Is.EqualTo(expectedLeast));
		Assert.That(actualMost, Is.EqualTo(expectedMost));
	}
	
	[Test]
	public void TestMostAndLEastSignificantBit2()
	{
		const int expectedMost = 64;
		const int expectedLeast = 64;

		var n = Ulong2.FromSetBits<Width65>(expectedLeast, expectedMost);
		int actualLeast = n.LeastSignificantBit();
		int actualMost = n.MostSignificantBit();
		
		Assert.That(actualLeast, Is.EqualTo(expectedLeast));
		Assert.That(actualMost, Is.EqualTo(expectedMost));
	}
	
	[Test]
	public void TestLeastSignificantZero1([Values(5, 64)] int expected)
	{
		var n = Ulong2<Width65>.MaxValue;
		n ^= Ulong2<Width65>.One << expected;
		int actual = n.LeastSignificantZero();
		
		Assert.That(actual, Is.EqualTo(expected));
	}
}
