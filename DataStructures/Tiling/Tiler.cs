using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;

namespace DataStructures.Tiling;

public static class Tiler
{
	public const int MaxWidth = 64;
	private const ulong EmptyRow = 0;
	private const ulong FullRow = ulong.MaxValue;
	private static int pageExponent = 2; //page Count == 1 << pageExponent

#pragma warning disable CA2211
	public static int Width;
#pragma warning restore CA2211
	
	private static ulong widthMask;
	private static Func<UlongStripEnd, bool> canRuleOut = _ => false;

	public sealed class UlongStripEnd : IStripEnd<ULongTile>
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
				? FullRow
				: y < Length
					? data[y + offset]
					: EmptyRow;

		public static IStripEnd<ULongTile> New(int _) => new UlongStripEnd();

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
			
			string s = "[";

			for (int i = 0; i < Length; i++)
			{
				s += RowToString(this[i]) + "/";
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

		private UlongStripEnd(UlongStripEnd stripEnd, Int2 position, ULongTile longTile)
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

#if DEBUG
			var empty = FindEmpty();
			Debug.Assert(empty.Y == 0);
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

		public IStripEnd Place(Int2 position, ULongTile longTile)
		{
			var potentialEnd = new UlongStripEnd(this, position, longTile);

			return canRuleOut(potentialEnd) ? null : potentialEnd;
		}

		public Int2 FindEmpty()
		{
			if (IsStraight)
			{
				return Int2.Zero; //This is outside the grid!
			}

			for (int i = 0; i < Length; i++)
			{
				if(this[i] == Tiler.widthMask) continue;

				int x = ULongOps.FindZeroPosition(this[i]);
				return new Int2(x, i);
			}
			
			return new Int2(0, Length);  //This is outside the grid!
		}

		public bool CanPlace(Int2 position, ULongTile tile)
		{
			for (int i = 0; i < tile.Rows.Length; i++)
			{
				ulong row = tile.Rows[i];
				int totalOffset = position.X + tile.XOffset;
				ulong offsetRow = row << totalOffset;

				if (!ULongOps.InRange(row, totalOffset, Width)) return false;
				
				if ((this[position.Y + i + tile.YOffset] & offsetRow) != 0)
				{
					return false;
				}
			}

			return true;
		}

		public static object GetComparer() => new Comparer();
	}

	public static IEnumerable<PositionedTile<TTile>> TileRect<TTile, TStripEnd>(IEnumerable<TTile> tiles, int width, Func<UlongStripEnd, bool> canRuleOut) 
		where TStripEnd : class, IStripEnd<TTile> 
		where TTile : ITile
	{
		if (width is <= 0 or > MaxWidth)
		{
			throw new ArgumentOutOfRangeException(nameof(width), "Must be between 1 and {MaxWidth} (inclusive)");
		}
		
		Width = width;
		widthMask = width == MaxWidth ? ulong.MaxValue : (1ul << width) - 1;
		Tiler.canRuleOut = canRuleOut;
		
		int nodeCount = 0;
		long totalBranches = 0;

		int totalBranchesBatch = 0;
		int countInBatch = 0;
		
		//var openList = new System.Collections.Generic.Queue<TStripEnd>();
		var openList = new System.Collections.Generic.Stack<TStripEnd>();
		openList.Push((TStripEnd) TStripEnd.New(width));
		
		var comparer = (IEqualityComparer<TStripEnd>) TStripEnd.GetComparer();
		var graph = new StripTilings<TTile, TStripEnd>(comparer);

		int count = 0;
		
		while (openList.Any())
		{
			var stripEnd = openList.Pop();

			if (graph.ContainsForward(stripEnd))
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
					
					var newStripEnd = (TStripEnd) stripEnd.Place(empty, tile);

					//if this pair is not already in the graph
					graph.Add(stripEnd, newStripEnd/*, empty, tile*/);

					//newStripEnd can be null if placement results in a tile that cannot be tiled further by some criterion
					if (newStripEnd == null) continue;
					
					if (!graph.ContainsForward(newStripEnd))
					{
						openList.Push(newStripEnd);
					}
					
					if (newStripEnd.IsStraight)
					{
						Console.WriteLine("--------------------------");
						return graph.GetTilingFromBack(newStripEnd);
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
		return null;
	}
}
