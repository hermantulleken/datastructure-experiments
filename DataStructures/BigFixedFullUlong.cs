using System;
using System.Diagnostics;
using DataStructures.Tiling;
using JetBrains.Annotations;

namespace DataStructures;

public sealed record BigFixedFullUlong
{
	public static readonly BigFixedFullUlong Zero1 = GetZero(1);
	public static readonly BigFixedFullUlong Zero2 = GetZero(2);
	public static readonly BigFixedFullUlong Zero3 = GetZero(3);
	public static readonly BigFixedFullUlong Zero4 = GetZero(4);
	public static readonly BigFixedFullUlong Zero5 = GetZero(5);
	public static readonly BigFixedFullUlong Zero6 = GetZero(6);
	public static readonly BigFixedFullUlong Zero7 = GetZero(7);
	public static readonly BigFixedFullUlong Zero8 = GetZero(8);
	
	public static readonly BigFixedFullUlong Ones1 = GetOnes(1);
	public static readonly BigFixedFullUlong Ones2 = GetOnes(2);
	public static readonly BigFixedFullUlong Ones3 = GetOnes(3);
	public static readonly BigFixedFullUlong Ones4 = GetOnes(4);
	public static readonly BigFixedFullUlong Ones5 = GetOnes(5);
	public static readonly BigFixedFullUlong Ones6 = GetOnes(6);
	public static readonly BigFixedFullUlong Ones7 = GetOnes(7);
	public static readonly BigFixedFullUlong Ones8 = GetOnes(8);
	
	private const ulong FullRow = ulong.MaxValue;

	private readonly ulong[] value;
	
	public BigFixedFullUlong(ulong[] value)
	{
		Size = value.Length;
		this.value = value; //should we copy?
	}

	public int Size { get; }

	public bool Equals(BigFixedFullUlong other)
	{
		AssertSizesAreEqual(this, other);
		for (int i = 0; i < Size; i++)
		{
			if (value[i] != other.value[i])
			{
				return false;
			}
		}
		
		return true;
	}
	
	public override int GetHashCode()
	{
		ulong hc = (ulong) Size;
		for (int index = 0; index < value.Length; index++)
		{
			ulong val = value[index];
			hc = unchecked(hc * 314159 + val);
		}

		return hc.GetHashCode();
	}

	private BigFixedFullUlong(int size)
	{
		this.Size = size;
		value = new ulong[size];
	}

	public bool IsZero()
	{
		for (int i = 0; i < Size; i++)
		{
			if (value[i] != 0)
			{
				return false;
			}
		}

		return true;
	}
	
	public bool IsOnes()
	{
		for (int i = 0; i < Size; i++)
		{
			if (value[i] != FullRow)
			{
				return false;
			}
		}

		return true;
	}
	
	public BigFixedFullUlong Reverse()
	{
		ulong[] newValue = new ulong[Size];

		for (int i = 0; i < Size; i++)
		{
			newValue[i] = Reverse(value[i]);
		}

		return new BigFixedFullUlong(newValue);
	}

	public int MostSignificantBit()
	{
		Debug.Assert(!IsZero());

		int i = Size - 1;

		while (i >= 0)
		{
			if (value[i] != 0)
			{
				return UlongOps.MostSignificantBit(value[i]);
			}

			i--;
		}

		return -1; // Should not be reached for non-zero values. 
	}
	
	public int LeastSignificantBit()
	{
		Debug.Assert(!IsZero());

		int i = 0;

		while (i < Size)
		{
			if (value[i] != 0)
			{
				return UlongOps.LeastSignificantBit(value[i]);
			}

			i++;
		}

		return -1; // Should not be reached for non-zero values. 
	}

	public int LeastSignificantZero() => (~this).LeastSignificantBit();

	//Consider putting this in an array

	public static BigFixedFullUlong GetZero(int size) => new BigFixedFullUlong(size);

	public static BigFixedFullUlong GetOnes(int size)
	{
		ulong[] newValue = new ulong[size];

		for (int i = 0; i < newValue.Length; i++)
		{
			newValue[i] = FullRow;
		}

		return new BigFixedFullUlong(newValue);
	}

	public static ulong LeastSignificantMask(int n)
	{
		AssertIsSize(n);
		return n == 64 ? FullRow : (1ul << n) - 1ul;
	}

	[AssertionMethod]
	private static void AssertIsSize(int n) => Debug.Assert(n is >= 0 and <= 64);
	
	[AssertionMethod]
	private static void AssertSizesAreEqual(BigFixedFullUlong bigNum1, BigFixedFullUlong bigNum2) => Debug.Assert(bigNum1.Size == bigNum2.Size);

	public static ulong MostSignificantMask(int n) => LeastSignificantMask(n) << (64 - n);

	public static BigFixedFullUlong operator>> (BigFixedFullUlong bigNum, int n)
	{
		AssertIsSize(n);
		var result = new BigFixedFullUlong(bigNum.Size);

		ulong mask = LeastSignificantMask(n);
		int lastIndex = bigNum.Size - 1;
		
		for (int i = 0; i < lastIndex; i++)
		{
			result.value[i] = bigNum.value[i] >> n;
			result.value[i] |= (bigNum.value[i + 1] & mask) << (64 - n);
		}
		
		result.value[lastIndex] = bigNum.value[lastIndex] >> n;

		return result;
	}

	public static BigFixedFullUlong operator<< (BigFixedFullUlong bigNum, int n)
	{
		AssertIsSize(n);
		var result = new BigFixedFullUlong(bigNum.Size);

		ulong mask = MostSignificantMask(n);
		
		for (int i = bigNum.Size - 1; i > 0; i--)
		{
			result.value[i] = bigNum.value[i] << n;
			result.value[i] |= (bigNum.value[i - 1] & mask) >> (64 - n);
		}
		
		result.value[0] = bigNum.value[0] << n;

		return result;
	}

	public static BigFixedFullUlong operator& (BigFixedFullUlong bigNum1, BigFixedFullUlong bigNum2)
	{
		AssertSizesAreEqual(bigNum1, bigNum2);

		ulong[] newValue = new ulong[bigNum1.Size];
		
		for (int i = 0; i < bigNum1.Size; i++)
		{
			newValue[i] = bigNum1.value[i] & bigNum2.value[i];
		}

		return new BigFixedFullUlong(newValue);
	}

	public static BigFixedFullUlong operator| (BigFixedFullUlong bigNum1, BigFixedFullUlong bigNum2)
	{
		AssertSizesAreEqual(bigNum1, bigNum2);

		ulong[] newValue = new ulong[bigNum1.Size];
		
		for (int i = 0; i < bigNum1.Size; i++)
		{
			newValue[i] = bigNum1.value[i] | bigNum2.value[i];
		}

		return new BigFixedFullUlong(newValue);
	}
	
	public static BigFixedFullUlong operator^ (BigFixedFullUlong bigNum1, BigFixedFullUlong bigNum2)
	{
		AssertSizesAreEqual(bigNum1, bigNum2);

		ulong[] newValue = new ulong[bigNum1.Size];
		
		for (int i = 0; i < bigNum1.Size; i++)
		{
			newValue[i] = bigNum1.value[i] ^ bigNum2.value[i];
		}

		return new BigFixedFullUlong(newValue);
	}
	
	public static BigFixedFullUlong operator~ (BigFixedFullUlong bigNum)
	{
		ulong[] newValue = new ulong[bigNum.Size];
		
		for (int i = 0; i < bigNum.Size; i++)
		{
			newValue[i] = ~bigNum.value[i];
		}

		return new BigFixedFullUlong(newValue);
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
		
		for (int i = Size - 1; i >= 0; i--)
		{
			result += Convert.ToString((long) value[i], 2).PadLeft(64, '0');
		}

		return result;
	}

	public BigFixedFullUlong MaskLast(ulong mask)
	{
		var newValue = new ulong[Size];
		for (int i = 0; i < Size; i++)
		{
			newValue[i] = value[i];
		}
		
		newValue[^1] &= mask;

		return new BigFixedFullUlong(newValue);
	}
}
