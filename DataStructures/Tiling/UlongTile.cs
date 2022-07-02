using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.Tiling;

public readonly struct UlongTile : ITile
{
	public readonly ulong[] Rows;
	public readonly int YOffset;
	public readonly int XOffset;
	public readonly int MaxY;
	public readonly int MaxX;

	public readonly int[] RowStart;
	public readonly int[] RowEnd;

	public readonly int Width;
	public readonly int Length;

	public UlongTile(IEnumerable<Int2> cells)
	{
		Cells = cells;
		MaxX = cells.Max(cell => cell.X);
		MaxY = cells.Max(cell => cell.Y);
		YOffset = cells.Min(cell => cell.Y);
		XOffset = cells.Min(cell => cell.X);
		
		Length = MaxY + 1 - YOffset;
		Width = MaxX + 1 -XOffset;
		
		Rows = new ulong[Length];
		RowStart = new int[Length];
		RowEnd = new int[Length];
			
		foreach (var cell in cells)
		{
			int x = cell.X - XOffset;

			if (x is < 0 or >= UlongOps.MaxWidth)
			{
				throw new ArgumentOutOfRangeException(nameof(cells));
			}

			int y = cell.Y - YOffset;
			
			Rows[y] |= 1ul << x;
		}

		for (int rowIndex = 0; rowIndex < Length; rowIndex++)
		{
			ulong row = Rows[rowIndex];
			RowStart[rowIndex] = UlongOps.LeastSignificantBit(row);
			RowEnd[rowIndex] = UlongOps.MostSignificantBit(row);
		}
	}

	public IEnumerable<Int2> Cells { get; }
	public static ITile New(IEnumerable<Int2> cells) => new UlongTile(cells);
	
	public override string ToString()
	{
		string RowToString(ulong row)
		{
			return new string(Convert.ToString((long) row, 2).Reverse().ToArray());
		}

		return Rows.Aggregate("[", (current, t) => current + (RowToString(t) + "/"));
	}
}
