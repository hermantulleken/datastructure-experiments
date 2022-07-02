using System;
using System.Collections.Generic;
#if DEBUG
#endif
using System.Linq;

namespace DataStructures.Tiling;



public readonly struct Ulong2Tile<TWidth> : ITile
	where TWidth : IWidth
{
	public readonly Ulong2<TWidth>[] Rows;
	public readonly int YOffset;
	public readonly int XOffset;
	public readonly int MaxY;
	public readonly int MaxX;

	public readonly int[] RowStart;
	public readonly int[] RowEnd;

	public readonly int Width;
	public readonly int Length;

	public Ulong2Tile(IEnumerable<Int2> cells)
	{
		Cells = cells;
		MaxX = cells.Max(cell => cell.X);
		MaxY = cells.Max(cell => cell.Y);
		YOffset = cells.Min(cell => cell.Y);
		XOffset = cells.Min(cell => cell.X);
		
		Length = MaxY + 1 - YOffset;
		Width = MaxX + 1 -XOffset;
		
		Rows = new Ulong2<TWidth>[Length];
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
			
			Rows[y] |= Ulong2<TWidth>.One << x;
		}

		for (int rowIndex = 0; rowIndex < Length; rowIndex++)
		{
			var row = Rows[rowIndex];
			RowStart[rowIndex] = row.LeastSignificantBit();
			RowEnd[rowIndex] = row.MostSignificantBit();
		}
	}

	public IEnumerable<Int2> Cells { get; }
	public static ITile New(IEnumerable<Int2> cells) => new UlongTile(cells);
	
	public override string ToString()
	{
		return Rows.Aggregate("[", (current, t) => current + (t + "/"));
	}
}

public interface ITile<TWidth, TTile> 
	where TWidth : IWidth
	where TTile : ITile<TWidth, TTile>
{
#pragma warning disable CA2252
	public static abstract bool CanRuleOut(FixedTiler<TWidth, TTile>.StripEnd stripEnd);
#pragma warning restore CA2252
}

public static class FixedTiler<TWidth, TTile> 
	where TWidth : IWidth 
	where TTile : ITile<TWidth, TTile>
{
	public sealed class StripEnd
	{
		public class Comparer : IEqualityComparer<StripEnd>
		{
			public bool Equals(StripEnd strip1, StripEnd strip2)
			{
				if (ReferenceEquals(strip1, strip2)) return true;
				if (ReferenceEquals(strip1, null)) return false;
				if (ReferenceEquals(strip2, null)) return false;

				if (strip1.Length != strip2.Length) return false;
				//if (strip1.width != strip2.width) return false;//Could be potentially be removed since we always work with fixed width

				for (int i = 0; i < strip1.Length; i++)
				{
					if (strip1[i] != strip2[i])
					{
						return false;
					}
				}

				return true;
			}

			//TODO implement correctly
			public int GetHashCode(StripEnd obj) => obj.hashCode;
		}

		private Ulong2<TWidth>[] data;
		private int offset;
		private readonly int hashCode;
		
		public int Length { get; private set; }
		public bool IsStraight => Length == 0;

		public Ulong2<TWidth> this[int y] =>
			y < 0
				? Ulong2<TWidth>.MaxValue
				: y < Length
					? data[y + offset]
					: Ulong2<TWidth>.Zero;

		public StripEnd()
		{
			data = null;
			Length = 0;
			offset = 0;
			hashCode = 0;
		}

		public override string ToString()
		{
			string s = "[";

			for (int i = 0; i < Length; i++)
			{
				s += this[i] + "/";
			}

			return s;
		}

		public override int GetHashCode() => hashCode;

		//private int GetRowHash(int row) => data[row].GetHashCode();

		public int CalculateHashCode()
		{
			int hash = data.Length;
			unchecked
			{
				foreach (Ulong2<TWidth> val in data)
				{
					hash = unchecked(hash * 314159 + val.GetHashCode());
				}
			}
			

			return hash.GetHashCode();
		}

		private StripEnd(StripEnd stripEnd, Int2 position, Ulong2Tile<TWidth> longTile)
		{
			void InitializeData()
			{
				int maxY = longTile.MaxY + position.Y;
				Length = Math.Max(stripEnd.Length, maxY + 1);

				data = new Ulong2<TWidth>[Length];

				for(int i = 0; i < Length; i++)
				{
					data[i] = stripEnd[i];
				}

				for(int i = 0; i < longTile.Rows.Length; i++)
				{
					var row = longTile.Rows[i];
					data[position.Y + i + longTile.YOffset] |= row << (longTile.XOffset + position.X);
				}
			}
			
			void ReduceFullRows()
			{
				var empty = FindEmpty();

				if (empty.Y != 0)
				{
					offset = empty.Y;
					Length -= offset;
				}
			}
			
			InitializeData();
			ReduceFullRows();

#if DEBUG
			var empty = FindEmpty();
			GLDebug.Assert(empty.Y == 0);
#endif

			if (IsStraight)
			{
				data = null;//no need to hold on to this
				hashCode = 0;
			}
			else
			{
				hashCode = data == null ? 0 : CalculateHashCode();
			}
		}

		public StripEnd Place(Int2 position, Ulong2Tile<TWidth> longTile)
		{
			var potentialEnd = new StripEnd(this, position, longTile);

			return TTile.CanRuleOut(potentialEnd) ? null : potentialEnd;
		}

		public Int2 FindEmpty()
		{
			if (IsStraight)
			{
				return Int2.Zero; //This is outside the grid!
			}

			for (int i = 0; i < Length; i++)
			{
				if(this[i] == Ulong2<TWidth>.MaxValue) continue;

				int x = this[i].LeastSignificantZero();
				return new Int2(x, i);
			}
			
			return new Int2(0, Length);  //This is outside the grid!
		}

		public bool CanPlace(Int2 position, Ulong2Tile<TWidth> tile)
		{
			for (int i = 0; i < tile.Rows.Length; i++)
			{
				var row = tile.Rows[i];
				int totalOffset = position.X + tile.XOffset;
				var offsetRow = row << totalOffset;

				if (!InRange(row, totalOffset, TWidth.Width)) return false;
				
				if ((this[position.Y + i + tile.YOffset] & offsetRow) != Ulong2<TWidth>.Zero)
				{
					return false;
				}
			}

			return true;
		}

		public static object GetComparer() => new Comparer();
	}
	
	
	private static bool InRange(Ulong2<TWidth> row, int offset, int width) => 
		(64 - row.MostSignificantZeroCount() + offset <= width)
		&& row.LeastSignificantZeroCount() + offset >= 0;

	public static bool TileRect(IEnumerable<Ulong2Tile<TWidth>> tiles, int width, Func<StripEnd, bool> canRuleOut)
	{
		if (width is <= 0 or > UlongOps.MaxWidth)
		{
			throw new ArgumentOutOfRangeException(nameof(width), "Must be between 1 and {MaxWidth} (inclusive)");
		}

		int nodeCount = 0;
		long totalBranches = 0;

		int totalBranchesBatch = 0;
		int countInBatch = 0;
		
		//var openList = new System.Collections.Generic.Queue<TStripEnd>();
		var openList = new System.Collections.Generic.Stack<StripEnd>();
		openList.Push(new StripEnd());
		
		var graph = new Graph<StripEnd, StripEnd>(new StripEnd.Comparer());

		int count = 0;
		
		while (openList.Any())
		{
			var stripEnd = openList.Pop();

			if (graph.Contains(stripEnd))
			{
				//This can happen because when we add nodes to the open list, we don't check the open list itself. 
				continue;
			}
			
			var empty = stripEnd.FindEmpty();

			int found = 0;
			foreach (var tile in tiles)
			{
				if (stripEnd.CanPlace(empty, tile))
				{
					found++;
					if (count % 100000 == 0)
					{
						Console.WriteLine(count + " " + openList.Count + " Branch Factor" + (totalBranches / (float) nodeCount)
						                  + " Branch Factor" + (totalBranchesBatch / (float) countInBatch) + " Graph " + graph.Count );
						totalBranchesBatch = 0;
						countInBatch = 0;
					}
					count++;
					
					var newStripEnd = stripEnd.Place(empty, tile);

					//if this pair is not already in the graph
					graph.Add(stripEnd, newStripEnd/*, empty, tile*/);

					//newStripEnd can be null if placement results in a tile that cannot be tiled further by some criterion
					if (newStripEnd == null) continue;
					
					if (!graph.Contains(newStripEnd))
					{
						openList.Push(newStripEnd);
					}
					
					if (newStripEnd.IsStraight)
					{
						Console.WriteLine("--------------------------");
						return true;
					}
				}
			}

			if (found == 0)
			{
				graph.Add(stripEnd, null/*, Int2.Zero, null*/);
			}

			totalBranches += found;
			totalBranchesBatch += found;
			countInBatch++;
			nodeCount++;
		}

		//Tiling does not exist
		return false;
	}
}
