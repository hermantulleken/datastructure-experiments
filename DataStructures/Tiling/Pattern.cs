using System;

namespace DataStructures.Tiling;

public class Pattern
{
	public readonly ulong[] Rows;
	public readonly ulong[] Mask;
	public readonly int Width;

	public readonly bool IsFirstRowBlocked;
	public Pattern(UlongTile tile)
	{
		Rows = tile.Rows;
		Mask = new ulong[Rows.Length];
		Width = 0;
		
		for (int rowIndex = 0; rowIndex < Rows.Length; rowIndex++)
		{
			int firstBit = tile.RowStart[rowIndex];
			int lastBit = tile.RowEnd[rowIndex];
			int width = lastBit - firstBit + 1;
			
			Mask[rowIndex] = 
				Width == UlongOps.MaxWidth
					? UlongOps.FullRow 
					: ((1ul << width) - 1ul) << firstBit;
			
			Width = Math.Max(Width, width);
		}

		IsFirstRowBlocked = Mask[0] == Rows[0];
	}
}
