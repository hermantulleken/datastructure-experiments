using System.Numerics;

namespace DataStructures.Tiling;

public static class ULongOps
{
	public static int MostSignificantBit(ulong n) => 63 - BitOperations.LeadingZeroCount(n);
	
	public static int LeastSignificantBit(ulong n) => BitOperations.TrailingZeroCount(n);
	
	public static int FindZeroPosition(ulong n)
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
