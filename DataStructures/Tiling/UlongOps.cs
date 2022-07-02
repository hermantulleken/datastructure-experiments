using System.Numerics;

namespace DataStructures.Tiling;

/*
	Not that BitOperations.LeadingZeroCount and 
	BitOperations.TrailingZeroCount seem to have 
	their names swapped. 
 */
public static class UlongOps
{
	public const int MaxWidth = 64;
	public const ulong EmptyRow = 0;
	public const ulong FullRow = ulong.MaxValue;
	
	
	public static int MostSignificantBit(ulong n) => 63 - BitOperations.LeadingZeroCount(n);
	
	public static int LeastSignificantBit(ulong n) => BitOperations.TrailingZeroCount(n);
	
	public static int FindZeroPosition(ulong n) => BitOperations.TrailingZeroCount(~n);
	
	// From https://www.techiedelight.com/reverse-bits-of-given-integer/
	public static ulong ReverseBits(ulong n)
	{
		int pos = MaxWidth - 1;     // maintains shift
 
		// store reversed bits of `n`. Initially, all bits are set to 0
		ulong reverse = 0;
 
		// do till all bits are processed
		while (pos >= 0 && n != 0)
		{
			// if the current bit is 1, then set the corresponding bit in the result
			if ((n & 1ul) != 0) 
			{
				reverse |= (1ul << pos);
			}
 
			n >>= 1;                // drop current bit (divide by 2)
			pos--;                  // decrement shift by 1
		}
 
		return reverse;
	}
	
	public static int FindZeroPosition_Slow(ulong n)
	{
		int position = 0;
			
		while ((n & 1) == 1)
		{
			n >>= 1;
			position++;
		}

		return position;
	}

	//Checks whether if we move the row by the offset, it still fits within the width
	public static bool InRange(ulong row, int offset, int width) => 
		(64 - BitOperations.LeadingZeroCount(row) + offset <= width)
		&& BitOperations.TrailingZeroCount(row) + offset >= 0;
}
