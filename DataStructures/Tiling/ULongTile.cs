using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.Tiling;

public struct ULongTile : ITile
{
	public readonly ulong[] Rows;
	public readonly int YOffset;
	public readonly int XOffset;
	public readonly int MaxY;

	public ULongTile(IEnumerable<Int2> cells)
	{
		Cells = cells;
		MaxY = cells.Max(cell => cell.Y);
		YOffset = cells.Min(cell => cell.Y);
		XOffset = cells.Min(cell => cell.X);
		
		int length = MaxY + 1 - YOffset;
		
		Rows = new ulong[length];

		foreach (var cell in cells)
		{
			int x = cell.X - XOffset;

			if (x is < 0 or >= Tiler.MaxWidth)
			{
				throw new ArgumentOutOfRangeException(nameof(cells));
			}

			int y = cell.Y - YOffset;
			
			Rows[y] |= 1ul << x;
		}
	}

	public IEnumerable<Int2> Cells { get; }
	public static ITile New(IEnumerable<Int2> cells) => new ULongTile(cells);
	
	public override string ToString()
	{
		string RowToString(ulong row)
		{
			return new string(Convert.ToString((long) row, 2).Reverse().ToArray());
		}

		return Rows.Aggregate("[", (current, t) => current + (RowToString(t) + "/"));
	}
}
