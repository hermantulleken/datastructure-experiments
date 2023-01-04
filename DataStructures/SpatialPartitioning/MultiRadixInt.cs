using System;

namespace DataStructures.SpatialPartitioning;

public static class Combinatorics
{
	private static readonly ulong[] FactorialTable =
	{
		1ul, 
		1ul, 
		2ul, 
		6ul, 
		24ul, 
		120ul, 
		720ul, 
		5040ul, 
		40320ul, 
		362880ul, 
		3628800ul, 
		39916800ul, 
		479001600ul,
		6227020800ul, 
		87178291200ul, 
		1307674368000ul, 
		20922789888000ul, 
		355687428096000ul,
		6402373705728000ul, 
		121645100408832000ul, 
		2432902008176640000ul 
	};
	
	[Private(ExposedFor.Testing)]
	public static ulong CalculateFactorial(int n)
	{
		switch (n)
		{
			case < 0:
				throw new ArgumentOutOfRangeException(nameof(n));
			case 0 or 1:
				return 1;
		}

		ulong product = 2;

		for (ulong i = 3; i <= (ulong) n; i++)
		{
			product *= i;
		}

		return product;
	}

	public static ulong Factorial(int n)
	{
		if (n is < 0 or > 21) throw new ArgumentOutOfRangeException(nameof(n));

		return FactorialTable[n];
	}
}

public class MultiRadixInt
{
	private int[] digits;
	
	public int DigitCount => digits.Length;

	//0 denotes the first digit
	private static int GetDigitSize(int digit) => (int) Combinatorics.Factorial(digit);

	private static int GetDigitCount(int n)
	{
		int digitSize = 1;
		int digitCount = 0;

		while (digitSize < n)
		{
			digitCount++;
			digitSize = GetDigitSize(digitCount);
		}

		return digitCount;
	}

	
}
