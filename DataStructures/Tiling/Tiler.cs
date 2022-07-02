using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DataStructures.Tiling;

public static class Tiler
{
	public class Context
	{
		public int Width { get; }
		public ulong WidthMask { get; }
		public Func<Context, UlongStripEnd, bool> CanRuleOut { get; }

		public Context(int width, Func<Context, UlongStripEnd, bool> canRuleOut = null)
		{
			Width = width;
			WidthMask = width == UlongOps.MaxWidth ? ulong.MaxValue : (1ul << width) - 1;
			CanRuleOut = canRuleOut ?? ((_, _) => false);
		}
	}
	
	
	
	public sealed class UlongStripEnd : IStripEnd<UlongTile, Context>
	{
		public class Comparer : IEqualityComparer<UlongStripEnd>
		{
			public bool Equals(UlongStripEnd strip1, UlongStripEnd strip2)
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
			public int GetHashCode(UlongStripEnd obj) => obj.hashCode;
		}

		private ulong[] data;
		private int offset;
		private readonly int hashCode;
		
		public int Length { get; private set; }
		public bool IsStraight => Length == 0;

		public ulong this[int y] =>
			y < 0
				? UlongOps.FullRow
				: y < Length
					? data[y + offset]
					: UlongOps.EmptyRow;

		public static IStripEnd<UlongTile, Context> New(int _) => new UlongStripEnd();

		public UlongStripEnd()
		{
			data = null;
			Length = 0;
			offset = 0;
			hashCode = 0;
		}

		public override string ToString()
		{
			string RowToString(ulong row)
			{
				return new string(Convert.ToString((long) row, 2).Reverse().ToArray());
			}
			
			string s = "[" + Length + " " + offset + " > ";

			for (int i = 0; i < Length; i++)
			{
				s += this[i] + ",";
			}

			return s;
		}

		public override int GetHashCode() => hashCode;

		//private int GetRowHash(int row) => data[row].GetHashCode();

		public int CalculateHashCode()
		{
			ulong hc = (ulong) data.Length;
			foreach (ulong val in data)
			{
				hc = unchecked(hc * 314159 + val);
			}

			return hc.GetHashCode();
		}

		private UlongStripEnd(Context context, UlongStripEnd stripEnd, Int2 position, UlongTile longTile)
		{
			void InitializeData()
			{
				int maxY = longTile.MaxY + position.Y;
				Length = Math.Max(stripEnd.Length, maxY + 1);

				data = new ulong[Length];

				for(int i = 0; i < Length; i++)
				{
					data[i] = stripEnd[i];
				}

				for(int i = 0; i < longTile.Rows.Length; i++)
				{
					ulong row = longTile.Rows[i];
					data[position.Y + i + longTile.YOffset] |= row << (longTile.XOffset + position.X);
				}
			}
			
			void ReduceFullRows()
			{
				var empty = FindEmpty(context);

				if (empty.Y != 0)
				{
					offset = empty.Y;
					Length -= offset;
				}
			}
			
			InitializeData();
			ReduceFullRows();

#if USE_ASSERTS
			var empty = FindEmpty(context);
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

		public IStripEnd Place(Context context, Int2 position, UlongTile longTile)
		{
			var potentialEnd = new UlongStripEnd(context, this, position, longTile);

			return context.CanRuleOut(context, potentialEnd) ? null : potentialEnd;
		}

		public Int2 FindEmpty(Context context)
		{
			if (IsStraight)
			{
				return Int2.Zero; //This is outside the grid!
			}

			for (int i = 0; i < Length; i++)
			{
				if(this[i] == context.WidthMask) continue;

				int x = UlongOps.FindZeroPosition(this[i]);
				return new Int2(x, i);
			}
			
			return new Int2(0, Length);  //This is outside the grid!
		}

		public bool CanPlace(Context context, Int2 position, UlongTile tile)
		{
			for (int i = 0; i < tile.Rows.Length; i++)
			{
				ulong row = tile.Rows[i];
				int totalOffset = position.X + tile.XOffset;
				ulong offsetRow = row << totalOffset;

				if (!UlongOps.InRange(row, totalOffset, context.Width)) return false;
				
				if ((this[position.Y + i + tile.YOffset] & offsetRow) != 0)
				{
					return false;
				}
			}

			return true;
		}

		public static object GetComparer() => new Comparer();
	}

	public static bool TileRect<TTile, TStripEnd>(IEnumerable<TTile> tiles, int width, Func<Context, UlongStripEnd, bool> canRuleOut) 
		where TStripEnd : class, IStripEnd<TTile, Context> 
		where TTile : ITile
	{
		if (width is <= 0 or > UlongOps.MaxWidth)
		{
			throw new ArgumentOutOfRangeException(nameof(width), "Must be between 1 and {MaxWidth} (inclusive)");
		}

		Context context = new(width, canRuleOut);
		
		Console.WriteLine(context.WidthMask);
		
		int nodeCount = 0;
		long totalBranches = 0;

		int totalBranchesBatch = 0;
		int countInBatch = 0;
		
		//var openList = new System.Collections.Generic.Queue<TStripEnd>();
		var openList = new System.Collections.Generic.Stack<TStripEnd>();
		openList.Push((TStripEnd) TStripEnd.New(width));
		
		var comparer = (IEqualityComparer<TStripEnd>) TStripEnd.GetComparer();
		//var graph = new Graph<TStripEnd, TStripEnd>(comparer);
		var set = new HashSet<TStripEnd>(comparer);
		int count = 0;
		
		while (openList.Any())
		{
			var stripEnd = openList.Pop();

			if (set.Contains(stripEnd))
			{
				//This can happen because when we add nodes to the open list, we don't check the open list itself. 
				continue;
			}
			
			var empty = stripEnd.FindEmpty(context);

			int found = 0;
			foreach (var tile in tiles)
			{
				if (stripEnd.CanPlace(context, empty, tile))
				{
					found++;
					if (count % 100000 == 0)
					{
						Console.WriteLine(count + " " + openList.Count + " Branch Factor" + (totalBranches / (float) nodeCount)
						                  + " Branch Factor" + (totalBranchesBatch / (float) countInBatch) + " Graph " + set.Count );
						totalBranchesBatch = 0;
						countInBatch = 0;
					}
					count++;
					
					var newStripEnd = (TStripEnd) stripEnd.Place(context, empty, tile);

					//if this pair is not already in the graph
					set.Add(stripEnd/*, empty, tile*/);
					//Console.WriteLine(stripEnd + " " + newStripEnd);

					//newStripEnd can be null if placement results in a tile that cannot be tiled further by some criterion
					if (newStripEnd == null) continue;
					
					if (!set.Contains(newStripEnd))
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
				set.Add(stripEnd/*, Int2.Zero, null*/);
				
				//Console.WriteLine(stripEnd + " " + "null");
			}

			totalBranches += found;
			totalBranchesBatch += found;
			countInBatch++;
			nodeCount++;
		}
		
		Console.WriteLine(set.Count);

		//Tiling does not exist
		return false;
	}
}

public interface ILongTile<TWidth, TTile> 
	where TWidth : IWidth
	where TTile : ILongTile<TWidth, TTile>
{
#pragma warning disable CA2252
	static abstract bool CanRuleOut(Tiler<TWidth, TTile>.StripEnd stripEnd);
#pragma warning restore CA2252
}

public class DefaultLongTile<TWidth> : ILongTile<TWidth, DefaultLongTile<TWidth>>
	where TWidth : IWidth
{
	public static bool CanRuleOut(Tiler<TWidth, DefaultLongTile<TWidth>>.StripEnd stripEnd) => false;
}

public readonly struct UlongTile<TWidth> : ITile
	where TWidth : IWidth
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

			if (x < 0 || x >= TWidth.Width)
			{
				throw new ArgumentOutOfRangeException(nameof(cells));
			}

			int y = cell.Y - YOffset;
			
			Rows[y] |= 1ul << x;
		}

		for (int rowIndex = 0; rowIndex < Length; rowIndex++)
		{
			var row = Rows[rowIndex];
			RowStart[rowIndex] = UlongOps.LeastSignificantBit(row);
			RowEnd[rowIndex] = UlongOps.MostSignificantBit(row);
		}
	}

	public IEnumerable<Int2> Cells { get; }
	public static ITile New(IEnumerable<Int2> cells) => new UlongTile(cells);
	
	public override string ToString()
	{
		return Rows.Aggregate("[", (current, t) => current + (t + "/"));
	}
}

public static class Tiler<TWidth, TTile>
	where TWidth : IWidth
	where TTile : ILongTile<TWidth, TTile>
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

		private ulong[] data;
		private int offset;
		private readonly int hashCode;
		
		public int Length { get; private set; }
		public bool IsStraight => Length == 0;

		public ulong this[int y] =>
			y < 0
				? TWidth.LastRowMask
				: y < Length
					? data[y + offset]
					: UlongOps.EmptyRow;

		public StripEnd()
		{
			data = null;
			Length = 0;
			offset = 0;
			hashCode = 0;
		}

		public override string ToString()
		{
			string RowToString(ulong row)
			{
				return new string(Convert.ToString((long) row, 2).Reverse().ToArray());
			}
			
			string s = "[" + Length + " " + offset + " > ";

			for (int i = 0; i < Length; i++)
			{
				s += (this[i]) + ",";
			}

			return s;
		}

		public override int GetHashCode() => hashCode;

		//private int GetRowHash(int row) => data[row].GetHashCode();

		public int CalculateHashCode()
		{
			ulong hc = (ulong) data.Length;
			foreach (ulong val in data)
			{
				hc = unchecked(hc * 314159 + val);
			}

			return hc.GetHashCode();
		}

		private StripEnd(StripEnd stripEnd, Int2 position, UlongTile longTile)
		{
			void InitializeData()
			{
				int maxY = longTile.MaxY + position.Y;
				Length = Math.Max(stripEnd.Length, maxY + 1);

				data = new ulong[Length];

				for(int i = 0; i < Length; i++)
				{
					data[i] = stripEnd[i];
				}

				for(int i = 0; i < longTile.Rows.Length; i++)
				{
					ulong row = longTile.Rows[i];
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

#if USE_ASSERTS
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

		public StripEnd Place(Int2 position, UlongTile longTile)
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
				if(this[i] == TWidth.LastRowMask) continue;

				int x = UlongOps.FindZeroPosition(this[i]);
				return new Int2(x, i);
			}
			
			return new Int2(0, Length);  //This is outside the grid!
		}

		public bool CanPlace(Int2 position, UlongTile tile)
		{
			for (int i = 0; i < tile.Rows.Length; i++)
			{
				ulong row = tile.Rows[i];
				int totalOffset = position.X + tile.XOffset;
				ulong offsetRow = row << totalOffset;

				if (!UlongOps.InRange(row, totalOffset, TWidth.Width)) return false;
				
				if ((this[position.Y + i + tile.YOffset] & offsetRow) != 0)
				{
					return false;
				}
			}

			return true;
		}

		public static object GetComparer() => new Comparer();
	}

	public static bool TileRect(IEnumerable<UlongTile> tiles)
	{
		Console.WriteLine(TWidth.LastRowMask);
		int nodeCount = 0;
		long totalBranches = 0;

		int totalBranchesBatch = 0;
		int countInBatch = 0;
		
		//var openList = new System.Collections.Generic.Queue<TStripEnd>();
		var openList = new System.Collections.Generic.Stack<StripEnd>();
		openList.Push(new StripEnd());
		
		//var graph = new Graph<StripEnd, StripEnd>(new StripEnd.Comparer());
		var set = new HashSet<StripEnd>(new StripEnd.Comparer());
		int count = 0;
		
		while (openList.Any())
		{
			var stripEnd = openList.Pop();

			if (set.Contains(stripEnd))
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
						                  + " Branch Factor" + (totalBranchesBatch / (float) countInBatch) + " Graph " + set.Count );
						totalBranchesBatch = 0;
						countInBatch = 0;
					}
					count++;
					
					var newStripEnd = stripEnd.Place(empty, tile);

					//if this pair is not already in the graph
					set.Add(stripEnd/*, empty, tile*/);
					
					//Console.WriteLine(stripEnd + " " + newStripEnd);

					//newStripEnd can be null if placement results in a tile that cannot be tiled further by some criterion
					if (newStripEnd == null) continue;
					
					if (!set.Contains(newStripEnd))
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
				set.Add(stripEnd/*, Int2.Zero, null*/);
				
				//Console.WriteLine(stripEnd + " " + "null");
			}

			totalBranches += found;
			totalBranchesBatch += found;
			countInBatch++;
			nodeCount++;
		}

		//Tiling does not exist
		Console.WriteLine(set.Count);
		return false;
	}
}
