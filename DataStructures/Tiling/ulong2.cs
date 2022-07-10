using System.Numerics;

namespace DataStructures.Tiling;

using System;
using JetBrains.Annotations;

public readonly struct Ulong2
{
	/// <summary>
	/// This provides a more convenient way to construct ulongs2 for testing purposes. It is slow.
	/// </summary>
	/// <returns></returns>
	public static Ulong2<TWidth> FromSetBits<TWidth>(params int[] setBits)
		where TWidth:IWidth
	{
		GLDebug.Assert(Ulong2<TWidth>.IsWidthSuitable());
		
		ulong value0 = 0;
		ulong value1 = 0;
		
		foreach (int setBit in setBits)
		{
			GLDebug.Assert(setBit >= 0 || setBit < TWidth.Width, "Out of range");
			
			if (setBit < 64)
			{
				value0 |= (1ul << setBit);
			}
			else
			{
				value1 |= (1ul << (setBit - 64));
			}
		}

		return new Ulong2<TWidth>(value0, value1);
	}
}

public readonly struct Ulong2<TWidth> : IEquatable<Ulong2<TWidth>>
	where TWidth : IWidth
{
	public static readonly Ulong2<TWidth> MaxValue = new(FullRow, TWidth.LastRowMask);
	public static readonly Ulong2<TWidth> Zero = new(0, 0);
	public static readonly Ulong2<TWidth> One = new(1, 0);

	public const int Size = 2;
	
	private const ulong FullRow = ulong.MaxValue;
	private readonly ulong value0, value1;
	
	//TODO Not all operators will work with this constructor
	private Ulong2(ulong value)
	{
		GLDebug.Assert(TWidth.Width is > 0 and <= 64);

		value0 = value;
		value1 = 0;
	}

	public Ulong2(params ulong[] value)
	{
		GLDebug.Assert(value.Length == Size);
		GLDebug.Assert(TWidth.Width is > 64 and <= Size * 64);

		value0 = value[0];
		value1 = value[1];
	}
	
	public bool IsZero() => value0 == 0 && value1 == 0;

	public bool IsOnes() =>
		value0 == ulong.MaxValue &&
		value1 == TWidth.LastRowMask;

	public Ulong2<TWidth> Reverse()
	{
		ulong newValue0 = (Reverse(value1) >> TWidth.EmptyBitCount);
		ulong newValue1 = (Reverse(value0) >> TWidth.EmptyBitCount) | (value0 & TWidth.EmptyBitMask);
		
		return new Ulong2<TWidth>(newValue0, newValue1);
	}

	public int MostSignificantBit()
	{
		GLDebug.Assert(!IsZero());

		if (value1 != 0)
		{
			return 64 + UlongOps.MostSignificantBit(value1);
		}

		if (value0 != 0)
		{
			return UlongOps.MostSignificantBit(value0);
		}

		return -1; // Should not be reached for non-zero values. 
	}
	
	public int LeastSignificantBit()
	{
		GLDebug.Assert(!IsZero());

		if (value0 != 0)
		{
			return UlongOps.LeastSignificantBit(value0);
		}
		
		if (value1 != 0)
		{
			return 64 + UlongOps.LeastSignificantBit(value1);
		}

		return -1; // Should not be reached for non-zero values. 
	}

	public int LeastSignificantZero() => (~this).LeastSignificantBit();

	//Consider putting this in an array
	//Need one value for each width

	public static Ulong2<TWidth> GetZero() => new();

	public static Ulong2<TWidth> GetOnes() => new(ulong.MaxValue, TWidth.LastRowMask);

	public static ulong LeastSignificantMask(int n)
	{
		AssertIsSize(n);
		return n == 64 ? FullRow : (1ul << n) - 1ul;
	}

	[AssertionMethod]
	private static void AssertIsSize(int n) => GLDebug.Assert(n is >= 0 and <= 64);
	

	public static ulong MostSignificantMask(int n) => LeastSignificantMask(n) << (64 - n);

	public static Ulong2<TWidth> operator>> (Ulong2<TWidth> bigNum, int n)
	{
		AssertIsSize(n);

		if (n == 64)
		{
			const ulong newValue1 = 0;
			ulong newValue0 = bigNum.value1;
		
			return new Ulong2<TWidth>(newValue0, newValue1);
		}
		else
		{
			ulong newValue1 = (bigNum.value1 >> n);
			ulong newValue0 = (bigNum.value0 >> n) | (bigNum.value1 << (64 - n));
		
			return new Ulong2<TWidth>(newValue0, newValue1);
		}
	}

	public static Ulong2<TWidth> operator<< (Ulong2<TWidth> bigNum, int n)
	{
		AssertIsSize(n);

		if (n == 64)
		{
			const ulong newValue0 = 0;
			ulong newValue1 = (bigNum.value0) & TWidth.LastRowMask;
			
			return new Ulong2<TWidth>(newValue0, newValue1);
			
		}
		else
		{
			ulong newValue0 = (bigNum.value0 << n);
			ulong newValue1 = ((bigNum.value1 << n) | (bigNum.value0 >> (64 - n))) & TWidth.LastRowMask;

			return new Ulong2<TWidth>(newValue0, newValue1);
		}
	}

	public static Ulong2<TWidth> operator& (Ulong2<TWidth> bigNum1, Ulong2<TWidth> bigNum2)
	{
		ulong newValue0 = bigNum1.value0 & bigNum2.value0;
		ulong newValue1 = bigNum1.value1 & bigNum2.value1;
		
		return new Ulong2<TWidth>(newValue0, newValue1);
	}

	public static Ulong2<TWidth> operator| (Ulong2<TWidth> bigNum1, Ulong2<TWidth> bigNum2)
	{
		ulong newValue0 = bigNum1.value0 | bigNum2.value0;
		ulong newValue1 = bigNum1.value1 | bigNum2.value1;
		
		return new Ulong2<TWidth>(newValue0, newValue1);
	}
	
	public static Ulong2<TWidth> operator^ (Ulong2<TWidth> bigNum1, Ulong2<TWidth> bigNum2)
	{
		ulong newValue0 = bigNum1.value0 ^ bigNum2.value0;
		ulong newValue1 = bigNum1.value1 ^ bigNum2.value1;
		
		return new Ulong2<TWidth>(newValue0, newValue1);
	}
	
	public static Ulong2<TWidth> operator~ (Ulong2<TWidth> bigNum)
	{
		ulong newValue0 = ~bigNum.value0;
		ulong newValue1 = (~bigNum.value1) & TWidth.LastRowMask;
		
		return new Ulong2<TWidth>(newValue0, newValue1);
	}
	
	//Implement a faster version using a lookup
	private static ulong Reverse(ulong n)
	{
		ulong result = 0;

		for (int i = 0; i < 64; i++)
		{
			result |= n & 1;
			n >>= 1;
			result <<= 1;
		}

		return result;
	}

	public override string ToString()
	{
		string result = string.Empty;
		
		result += Convert.ToString((long) value1, 2).PadLeft(64, '0');
		result += Convert.ToString((long) value0, 2).PadLeft(64, '0');
		
		return Reverse(result);
	}

	public int LeastSignificantZeroCount() =>
		value0 == 0 
			? value1 == 0 
				? TWidth.Width 
				: BitOperations.TrailingZeroCount(value1) + 64 
			: BitOperations.TrailingZeroCount(value0);

	private static string Reverse( string s )
	{
		char[] charArray = s.ToCharArray();
		Array.Reverse( charArray );
		return new string( charArray );
	}

	public bool Equals(Ulong2<TWidth> other) => value0 == other.value0 && value1 == other.value1;

	public override bool Equals(object obj) => obj is Ulong2<TWidth> other && Equals(other);

	public static bool operator ==(Ulong2<TWidth> left, Ulong2<TWidth> right) => left.Equals(right);

	public static bool operator !=(Ulong2<TWidth> left, Ulong2<TWidth> right) => !left.Equals(right);
	
	public override int GetHashCode() => HashCode.Combine(value0, value1);

	public int MostSignificantZeroCount()
	{
		if (value1 == 0)
		{
			if (value0 == 0)
			{
				return TWidth.Width;
			}

			return BitOperations.LeadingZeroCount(value0) + 64 - TWidth.EmptyBitCount;
		}

		return BitOperations.LeadingZeroCount(value1) - TWidth.EmptyBitCount;
	}

	public static bool IsWidthSuitable() => TWidth.Width is >= 64 and < 64 * 2;
	public static bool IsLengthSuitable(int length)=> length == 2;
}
