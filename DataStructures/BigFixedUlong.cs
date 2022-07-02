using System.Diagnostics;
using JetBrains.Annotations;

namespace DataStructures;

public sealed record BigFixedUlong
{
	private const ulong FullRow = ulong.MaxValue;

	private readonly BigFixedFullUlong value;
	private readonly int width;

	private readonly int emptyBits;

	private readonly ulong lastRowMask;
	
	public BigFixedUlong(ulong[] value, int width)
	{
		int size = GetSize(width);
		Debug.Assert(size == value.Length);
		lastRowMask = GetLastRowMask(width);
		this.value = new BigFixedFullUlong(value).MaskLast(lastRowMask); //should we copy?
		this.width = width;
		int excess = width & 63;
		emptyBits = excess == 0 ? 0 : 64 - excess;
	}
	
	public BigFixedUlong(BigFixedFullUlong value, int width)
	{
		int size = GetSize(width);
		Debug.Assert(size == value.Size);
		lastRowMask = GetLastRowMask(width);
		this.value = value.MaskLast(lastRowMask); //should we copy?
		this.width = width;
		
		int excess = width & 63;
		emptyBits = excess == 0 ? 0 : 64 - excess;
	}
	
	private BigFixedUlong(int width)
	{
		int size = GetSize(width);
		value = BigFixedFullUlong.GetZero(size);
		this.width = width;
		lastRowMask = GetLastRowMask(width);
		int excess = width & 63;
	}

	public bool Equals(BigFixedUlong other) => other != null && value == other.value; //Valid, since empty bits are guaranteed zeroed out. 

	public override int GetHashCode() => value.GetHashCode();

	public bool IsZero()
	{
		return value.IsZero();
	}
	
	//TODO: not efficient
	public bool IsOnes() => (~this).IsZero();

	public BigFixedFullUlong Reverse() => value.Reverse() >> emptyBits;

	public int MostSignificantBit() => value.MostSignificantBit(); //Works since extra bits zeroed out.
	public int LeastSignificantBit() => value.LeastSignificantBit();
	public int LeastSignificantZero() => value.LeastSignificantZero();

	//Consider putting this in an array

	public static BigFixedUlong GetZero(int width) 
		=> new BigFixedUlong(width);

	public static BigFixedUlong GetOnes(int width)
	{
		int size = GetSize(width);
		ulong[] newValue = new ulong[size];

		for (int i = 0; i < newValue.Length - 1; i++)
		{
			newValue[i] = FullRow;
		}

		newValue[^1] = GetLastRowMask(width);

		return new BigFixedUlong(newValue, width);
	}

	public static ulong LeastSignificantMask(int n)
	{
		AssertIsSize(n);
		return n == 64 ? FullRow : (1ul << n) - 1ul;
	}

	[AssertionMethod]
	private static void AssertIsSize(int n) => Debug.Assert(n is >= 0 and <= 64);
	
	[AssertionMethod]
	private static void AssertSizesAreEqual(BigFixedUlong bigNum1, BigFixedUlong bigNum2) => Debug.Assert(bigNum1.width == bigNum2.width);

	public static ulong MostSignificantMask(int n) => LeastSignificantMask(n) << (64 - n);

	public static BigFixedUlong operator >> (BigFixedUlong bigNum, int n) 
		=> new(bigNum.value >> n, bigNum.width);

	public static BigFixedUlong operator<< (BigFixedUlong bigNum, int n) 
		=> new(bigNum.value << n, bigNum.width); //The constructor will wipe out the empty bits.

	public static BigFixedUlong operator& (BigFixedUlong bigNum1, BigFixedUlong bigNum2) 
		=> new BigFixedUlong(bigNum1.value & bigNum2.value, bigNum1.width);

	public static BigFixedUlong operator| (BigFixedUlong bigNum1, BigFixedUlong bigNum2)
		=> new BigFixedUlong(bigNum1.value | bigNum2.value, bigNum1.width);
	
	public static BigFixedUlong operator^ (BigFixedUlong bigNum1, BigFixedUlong bigNum2)
		=> new BigFixedUlong(bigNum1.value ^ bigNum2.value, bigNum1.width);
	
	public static BigFixedUlong operator~ (BigFixedUlong bigNum)
		=> new BigFixedUlong(~bigNum.value, bigNum.width);
	
	public override string ToString() => value.ToString().Substring(0, width);

	private static int GetSize(int width) => GLMath.CeilDiv(width, 64);

	private static ulong GetLastRowMask(int width)
	{
		var lastRowSize = width & 63; // % 64
		
		if (lastRowSize == 0)
		{
			return ulong.MaxValue;
		}

		return (1ul << lastRowSize) - 1;
	}
}
