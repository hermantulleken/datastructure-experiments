using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DataStructures.Tiling;

public static class PagedTiler
{
	public const int MaxWidth = 64;
	private const ulong EmptyRow = 0;
	private const ulong FullRow = ulong.MaxValue;
	private static int pageExponent = 2; //page Count == 1 << pageExponent
	public static int pageCount;
	private static int width;
	public static int usedPageCount;

#pragma warning disable CA2211
	public static int Width
	{
		get => width;
		set
		{
			width = value;
			usedPageCount = width / 64;
			pageExponent = 0;

			while (1 << pageExponent < usedPageCount)
			{
				pageExponent++;
			}

			pageCount = 1 << pageExponent;
			
			widthMask = width == MaxWidth ? ulong.MaxValue : (1ul << width) - 1; 
		}
	}
#pragma warning restore CA2211
	
	private static ulong widthMask;
	private static Func<PagedUlongStripEnd, bool> canRuleOut = _ => false;
	
	public sealed class PagedUlongStripEnd : IStripEnd<ULongTile>
	{
		public class Comparer : IEqualityComparer<PagedUlongStripEnd>
		{
			public bool Equals(PagedUlongStripEnd strip1, PagedUlongStripEnd strip2)
			{
				if (ReferenceEquals(strip1, strip2)) return true;
				if (ReferenceEquals(strip1, null)) return false;
				if (ReferenceEquals(strip2, null)) return false;

				if (strip1.Length != strip2.Length) return false;
				//if (strip1.width != strip2.width) return false;//Could be potentially be removed since we always work with fixed width

				for (int i = 0; i < strip1.pagedLength; i++)
				{
					if (strip1[i] != strip2[i])
					{
						return false;
					}
				}

				return true;
			}

			//TODO implement correctly
			public int GetHashCode(PagedUlongStripEnd obj) => obj.hashCode;
		}

		private ulong[] data;
		private int pagedOffset;
		private readonly int hashCode;
		
		public int Length { get; private set; }

		public int pagedLength;
		public bool IsStraight => Length == 0;

		public ulong this[int y] =>
			y < 0
				? FullRow
				: y < pagedLength
					? data[y + pagedOffset]
					: EmptyRow;

		public static IStripEnd<ULongTile> New(int _) => new PagedUlongStripEnd();

		public PagedUlongStripEnd()
		{
			data = null;
			Length = 0;
			pagedOffset = 0;
			hashCode = 0;
			pagedLength = 0;
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
				for (int j = 0; j < usedPageCount; j++)
				{
					int rowIndex = i * pageCount + j;
				
					s += RowToString(data[rowIndex]) + ".";
				}

				s += "/";

			}

			return s;
		}

		public override int GetHashCode() => hashCode;

		//private int GetRowHash(int row) => data[row].GetHashCode();

		public int CalculateHashCode()
		{
			ulong hc = (ulong) pagedLength;
			for (int i = 0; i < pagedLength; i++)
			{
				ulong val = this[i];
				hc = unchecked(hc * 314159 + val);
			}

			return hc.GetHashCode();
		}

		private PagedUlongStripEnd(PagedUlongStripEnd stripEnd, Int2 position, ULongTile longTile)
		{
			void InitializeData()
			{
				int maxY = longTile.MaxY + position.Y;
				Length = Math.Max(stripEnd.Length, maxY + 1);
				pagedLength = Length << pageExponent;

				data = new ulong[pagedLength];

				for(int i = 0; i < pagedLength; i++)
				{
					data[i] = stripEnd[i];
				}

				for(int i = 0; i < longTile.Rows.Length; i++)
				{
					ulong row = longTile.Rows[i];
					int y = position.Y + i + longTile.YOffset;
					int x = longTile.XOffset + position.X;

					if (x >= 0)
					{
						int xx = x & 63; //x % 64
						ulong movedRow = row << xx;

						if (movedRow != 0)
						{
							int yy = (y << pageExponent) + (x >> 6); // x >> 6 is x / 64
							data[yy + pagedOffset] |= movedRow;
						}
					
						if(xx == 0) continue; //In this case, the expression below becomes row >> 64, which does nothing
					
						//if row == 0b10000, and xx == 62, then movedRow above will be 0
						//to get the points, we move -2 = 64 - 62, or (64 - xx) to the other side
						movedRow = row >> (64 - xx);

						if (movedRow != 0)
						{
							int yy = (y << pageExponent) + (x >> 6) + 1; // x >> 6 is x / 64
							data[yy + pagedOffset] |= movedRow;
						}
					}
					else
					{
						throw new NotImplementedException();
					}
				}
			}
			
			void ReduceFullRows()
			{
				var empty = FindEmpty();

				if (empty.Y != 0)
				{
					pagedOffset = empty.Y << pageExponent;
					Length -= empty.Y;
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
			var potentialEnd = new PagedUlongStripEnd(this, position, longTile);

			return canRuleOut(potentialEnd) ? null : potentialEnd;
		}

		public Int2 FindEmpty()
		{
			if (IsStraight)
			{
				return Int2.Zero; //This is outside the grid!
			}
			
			int x;

			for (int i = 0; i < Length; i++)
			{
				for (int j = 0; j < usedPageCount - 1; j++)
				{
					int ii = i << pageExponent + j;
					if (this[ii] == MaxWidth)
					{
						continue;
					}
					
					x = ULongOps.FindZeroPosition(this[ii]);
					return new Int2(x + j * pageCount, i);
				}
				
				int ii2 = i << pageExponent + usedPageCount - 1;
				if(this[ii2] == widthMask) continue;

				x = ULongOps.FindZeroPosition(this[ii2]);
				return new Int2(x + (usedPageCount - 1) * pageCount, i);
			}
			
			return new Int2(0, Length);  //This is outside the grid!
		}

		public bool CanPlace(Int2 position, ULongTile tile)
		{
			for (int i = 0; i < tile.Rows.Length; i++)
			{
				ulong row = tile.Rows[i];
				int y = position.Y + i + tile.YOffset;

				if (y < 0) return false;
				

				int x = tile.XOffset + position.X;
				if (!ULongOps.InRange(row, x, Width)) return false;	
				
				if (y >= Length) continue; //do y check after the x check, otherwise false positives are returned. 
				
				if (x >= 0)
				{
					int xx = x & 63; //x % 64
					ulong movedRow = row << xx;

					if (movedRow != 0)
					{
						int yy = (y << pageExponent) + (x >> 6); // x >> 6 is x / 64
						if ((this[pagedOffset] & movedRow) != 0ul)
						{
							return false;
						}
					}
				
					if (xx == 0) continue;//In this case, the expression below becomes row >> 64, which does nothing
					
					//if row == 0b10000, and xx == 62, then movedRow above will be 0
					//to get the points, we move -2 = 64 - 62, or (64 - xx) to the other side
					movedRow = row >> (64 - xx);

					if (movedRow != 0)
					{
						int yy = (y << pageExponent) + (x >> 6) + 1; // x >> 6 is x / 64
						if((data[yy] & movedRow) != 0)
						{
							return false;
						}
					}
				}
				else
				{
					throw new NotImplementedException();
				}
			}

			return true;
		}

		public static object GetComparer() => new Comparer();
	}
	
	public static IEnumerable<PositionedTile<TTile>> TileRect<TTile, TStripEnd>(IEnumerable<TTile> tiles, int width, Func<PagedUlongStripEnd, bool> canRuleOut) 
		where TStripEnd : class, IStripEnd<TTile> 
		where TTile : ITile
	{
		if (width <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(width), "Must be positive");
		}
		
		Width = width;
		widthMask = width == MaxWidth ? ulong.MaxValue : (1ul << width) - 1;
		PagedTiler.canRuleOut = canRuleOut;
		
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
