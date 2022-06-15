using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DataStructures.Tiling;

[Obsolete("Use " + nameof(ImmutableStripEnd))]
public class StripEnd
{
	public class EdgeComparer : IEqualityComparer<StripEnd>
	{
		public bool Equals(StripEnd x, StripEnd y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (ReferenceEquals(x, null)) return false;
			if (ReferenceEquals(y, null)) return false;
			if (x.GetType() != y.GetType()) return false;
			
			return x.Eq(y);
		}

		//TODO implement correctly
		public int GetHashCode(StripEnd obj)
		{
			return (obj.rows != null ? obj.rows.GetHashCode() : 0);
		}
	}
	
	private int Width => rows.Length;
	private DoubleEndedList<bool>[] rows;

	private int length;

	public StripEnd(int width)
	{
		rows = new DoubleEndedList<bool>[width];

		for (int i = 0; i < width; i++)
		{
			rows[i] = new DoubleEndedList<bool>(16);
		}

		length = 0;
	}
	
	public Int2 GetFirstEmpty()
	{
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				var point = new Int2(i, j);
				if (!this[point])
				{
					return point;
				}
			}
		}
		
		//TODO: Check whether grid is reduce.
		return Int2.Zero; //The grid has no entries, so the first empty is outside the grid. This assumes the grid is reduced
	}

	public void Reduce()
	{
		int reduceCount = 0;

		bool AllFull() => rows.All(row => row[reduceCount]);
		
		while(AllFull())
		{
			reduceCount++;
		}

		foreach (var row in rows)
		{
			row.RemoveFront(reduceCount);
		}
	}

	public bool Eq(StripEnd other)
	{
		if (Width != other.Width)
		{
			return false;
		}

		for (int i = 0; i < Width; i++)
		{
			if (!Eq(rows[i], other.rows[i]))
			{
				return false;
			}
		}

		return true;
	}

	private bool Eq(DoubleEndedList<bool> list1, DoubleEndedList<bool> list2)
	{
		for (int i = 0; i < Math.Max(list1.Count, list2.Count); i++)
		{
			if (GetItemAt(list1, i) != GetItemAt(list2, i))
			{
				return false;
			}
		}

		return true;
	}

	private static int Compare(DoubleEndedList<bool> list1, DoubleEndedList<bool> list2)
	{
		for (int i = 0; i < Math.Max(list1.Count, list2.Count); i++)
		{
			bool item1 = GetItemAt(list1, i);
			bool item2 = GetItemAt(list2, i);
			
			if (item1 != item2)
			{
				return item1 ? 1 : -1;
			}
		}

		return 0;
	}

	public int Compare(StripEnd other)
	{
		if (Width != other.Width)
		{
			return Width > other.Width ? 1 : -1;
		}
		
		for (int i = 0; i < Width; i++)
		{
			int rowsComparison = Compare(rows[i], other.rows[i]);
			
			if (rowsComparison != 0)
			{
				return rowsComparison;
			}
		}

		return 0;
	}

	private static bool GetItemAt(DoubleEndedList<bool> list, int index) => index < 0 || (index < list.Count && list[index]);

	public bool this[Int2 index]
	{
		get => GetItemAt(rows[index.Y], index.X);
		set
		{
			rows[index.Y][index.X] = value;
			length = Math.Max(length, rows[index.Y].Count);
		}
	}

	public static IEqualityComparer<StripEnd> GetComparer() => new EdgeComparer();

	//Only works if stripEnd is reduced!
	public bool IsStraight() => length == 0;
}
