using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.Tiling;

public static class PagedTiler
{
	public class Context
	{
		public readonly int Width;
		public readonly int PageExponent; //page Count == 1 << pageExponent
		public readonly int PageCount;
		public readonly int UsedPageCount;
		public readonly ulong WidthMask;
		public readonly Func<Context, StripEnd, bool> CanRuleOut;
		public readonly int LastRowWidth;

		public int GetWidth(int page) => widths[page];

		public ulong Mask(int page) => page == UsedPageCount - 1 ? ulong.MaxValue : WidthMask;

		private int[] widths;

		public Context(int width, Func<Context, StripEnd, bool> canRuleOut = null)
		{
			Width = width;
			CanRuleOut = canRuleOut ?? ((_, _) => false);
			UsedPageCount = (width + 64 - 1) / 64;
			PageExponent = 0;

			LastRowWidth = width & 63;

			while (1 << PageExponent < UsedPageCount)
			{
				PageExponent++;
			}

			PageCount = 1 << PageExponent;
			widths = new int[PageCount];

			for(int i = 0; i < widths.Length; i++)
			{
				widths[i] = i == UsedPageCount - 1? LastRowWidth : UlongOps.MaxWidth;
			}

			int lastPageWidth = width & 63;
			WidthMask = lastPageWidth == 0 ? ulong.MaxValue : (1ul << lastPageWidth) - 1; 
		}
	}

	public sealed class StripEnd
	{
		public class Comparer : IEqualityComparer<StripEnd>
		{
			public bool Equals(StripEnd strip1, StripEnd strip2)
			{
				//TODO: This assumes that bits out of the width range are set to 0. Is this OK?
				bool ValuesEqual()
				{
					for (int i = 0; i < strip1.pagedLength; i++)
					{
						if (strip1[i] != strip2[i])
						{
							return false;
						}
					}

					return true;
				}
				
				bool ReflectedValuesEqual()
				{
					if (strip1.reflected == null || strip2.reflected == null)
					{
						return false;
					}
					
					for (int i = 0; i < strip1.pagedLength; i++)
					{
						if (strip1.GetReflectedRow(i) != strip2[i])
						{
							return false;
						}

						if (i == strip1.pagedLength - 1)
						{
							//Console.WriteLine("Almost!");
						}
					}
					
					//Console.WriteLine("Found!");

					return true;
				}
				
				if (ReferenceEquals(strip1, strip2)) return true;
				if (ReferenceEquals(strip1, null)) return false;
				if (ReferenceEquals(strip2, null)) return false;

				if (strip1.Length != strip2.Length) return false;
				//if (strip1.width != strip2.width) return false;//Could be potentially be removed since we always work with fixed width

				return ValuesEqual() || ReflectedValuesEqual();
			}

			//TODO implement correctly
			public int GetHashCode(StripEnd obj) => obj.hashCode;
		}

		private ulong[] data;
		private ulong[] reflected;
		private int pagedOffset;
		private readonly int hashCode;
		private int pagedLength;
		
		public int Length { get; private set; }

		public string Pic => Picture();
		
		public bool IsStraight => Length == 0;

		public ulong this[int y] =>
			y < 0
				? UlongOps.FullRow //TODO is this correct 
				: y < pagedLength
					? data[y + pagedOffset]
					: UlongOps.EmptyRow;

		public StripEnd()
		{
			data = null;
			Length = 0;
			pagedOffset = 0;
			hashCode = 0;
			pagedLength = 0;
		}
		
		/// <summary>
		/// This constructor is mainly provided for quick testing.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="data"></param>
		[Private(ExposedFor.Testing)]
		public StripEnd(Context context, ulong[] data)
		{
			GLDebug.Assert(data.Length % context.PageCount == 0);
			this.data = data;
			pagedLength = data.Length;
			Length = pagedLength / context.PageCount;
			pagedOffset = 0;
			
			reflected = GetReflectedData(data, pagedOffset, Length, context);
			
			hashCode = CalculateHashCode();
		}

		private StripEnd(Context context, StripEnd stripEnd, Int2 position, UlongTile tile)
		{
			void CopyFromOldEdge()
			{
				for (int i = 0; i < pagedLength; i++)
				{
					data[i] = stripEnd[i];
				}

				pagedOffset = 0;
			}

			void CreateData()
			{
				int maxY = tile.MaxY + position.Y;
				Length = Math.Max(stripEnd.Length, maxY + 1);
				pagedLength = Length << context.PageExponent;

				data = new ulong[pagedLength];
			}

			void PlaceNewTile()
			{
				for (int tileRowIndex = 0; tileRowIndex < tile.Rows.Length; tileRowIndex++)
				{
					ulong row = tile.Rows[tileRowIndex];
					int totalY = position.Y + tileRowIndex + tile.YOffset;
					
					GLDebug.Assert(totalY >= 0);
					
					int totalX = tile.XOffset + position.X;

					GLDebug.Assert(UlongOps.InRange(row, totalX, context.Width));
					
					//Now we know our tile-row will fit in the allocated pages after the translation
					//This is asserted below
					
					int rowStart = tile.RowStart[tileRowIndex];
					int rowEnd = tile.RowEnd[tileRowIndex];
					
					ulong translatedRow = row << (64 + totalX) % 64; //We add 64 so negative translations work too
					int pageIndex = (rowStart + totalX) / 64;
					
					GLDebug.Assert(pageIndex >= 0);
					GLDebug.Assert(pageIndex < context.UsedPageCount);
					
					int index = totalY * context.PageCount + pageIndex;

					GLDebug.Assert((this[index] & translatedRow) == 0);

					data[pagedOffset + index] |= translatedRow;
					
					//It is possible that the translation move the row over two pages
					//No more, since the tile width is max 64

					int endPageIndex = (rowEnd + totalX) / 64;
					
					if (endPageIndex != pageIndex) //second part of tile is on different page
					{
						GLDebug.Assert(endPageIndex == pageIndex + 1);
						GLDebug.Assert(endPageIndex < context.UsedPageCount);
						
						//totalX can never be negative for tiles that go over two pages.
						GLDebug.Assert(totalX >= 0);
						
						//retrieve the part of the row that was cut off in the previous calculation
						//i.e. the part of the row that goes on the next page
						translatedRow = row >> (64 - totalX);
						index += 1;
						
						GLDebug.Assert((this[index] & translatedRow) == 0);
						data[pagedOffset + index] |= translatedRow;

					}
				}
			}
			
			void ReduceFullRows()
			{
				var empty = FindEmpty(context);

				if (empty.Y != 0)
				{
					pagedOffset = empty.Y << context.PageExponent;
					Length -= empty.Y;
					pagedLength = Length << context.PageExponent;
				}
			}
			
			CreateData();
			CopyFromOldEdge();
			PlaceNewTile();
			ReduceFullRows();

			reflected = GetReflectedData(data, pagedOffset, Length, context);

#if DEBUG
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

		/// <summary>
		/// Prefer to use <see cref="ToString(Context)"/>.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string RowToString(ulong row)
			{
				return new string(Convert.ToString((long) row, 2).Reverse().ToArray());
			}
			
			string s = "[";

			if (data == null)
			{
				return s;
			}

			for (int i = pagedOffset; i < data.Length; i++)
			{
				s += RowToString(data[i]) + ".";
				s += "/";

			}

			return s;
		}
		
		public string ToString(Context context)
		{
			string RowToString(ulong row)
			{
				return new string(Convert.ToString((long) row, 2).Reverse().ToArray());
			}
			
			string s = "[";

			for (int i = 0; i < Length; i++)
			{
				for (int j = 0; j < context.UsedPageCount; j++)
				{
					int rowIndex = pagedOffset + i * context.PageCount + j;
				
					s += RowToString(data[rowIndex]) + ".";
				}

				s += "/";

			}

			return s;
		}
		
		public string ToString2(Context context)
		{
			string RowToString(ulong row)
			{
				return new string(Convert.ToString((long) row, 2).Reverse().ToArray());
			}
			
			string s = "-------------\n";

			for (int i = 0; i < Length; i++)
			{
				for (int j = 0; j < context.UsedPageCount; j++)
				{
					int rowIndex = pagedOffset + i * context.PageCount + j;
				
					s += RowToString(data[rowIndex]) + "|";
				}

				s += "\n";

			}

			return s += "-------------";
		}

		public string Picture()
		{
			string RowToString(ulong row)
			{
				return new string(Convert.ToString((long) row, 2).Reverse().ToArray());
			}
			
			string s = "-------------\n";

			for (int i = 0; i < pagedLength; i++)
			{
				s += RowToString(this[i]) + "\n";
			}

			return s;
		}
		
		public override int GetHashCode() => hashCode;

		public int CalculateHashCode()
		{
			ulong hc = (ulong) pagedLength;
			for (int i = 0; i < pagedLength; i++)
			{
				ulong val = Math.Max(this[i], GetReflectedRow(i)); //This normalizes choosing between a copy and its reflection. 
				hc = unchecked(hc * 314159 + val);
			}

			return hc.GetHashCode();
		}

		//Tile needs to be normalized!
		public StripEnd Place(Context context, Int2 position, UlongTile longTile)
		{
			var potentialEnd = new StripEnd(context, this, position, longTile);

			return context.CanRuleOut(context, potentialEnd) ? null : potentialEnd;
		}

		public Int2 FindEmpty(Context context)
		{
			if (IsStraight)
			{
				return Int2.Zero; //This is outside the grid!
			}

			for (int y = 0; y < Length; y++)
			{
				for (int pageIndex = 0; pageIndex < context.UsedPageCount; pageIndex++)
				{
					int index = y * context.PageCount + pageIndex;
					ulong page = this[index];
					
					if(page == UlongOps.FullRow) continue;

					int xInPage = UlongOps.FindZeroPosition(page);

					if (xInPage == context.LastRowWidth)
					{
						//The 0 is out of bounds, so really the row is full
						continue;
					}

					return new Int2(pageIndex * 64 + xInPage, y);
				}
			}
			
			return new Int2(0, Length);  //This is outside the grid!
		}

		//Tile needs to be normalized!
		public bool CanPlace(Context context, Int2 position, UlongTile tile)
		{
			for (int tileRowIndex = 0; tileRowIndex < tile.Rows.Length; tileRowIndex++)
			{
				ulong row = tile.Rows[tileRowIndex];
				int totalY = position.Y + tileRowIndex + tile.YOffset;

				if (totalY < 0)
				{
					return false;
				}
				
				int totalX = tile.XOffset + position.X;

				if (!UlongOps.InRange(row, totalX, context.Width))
				{
					return false;
				}	
				
				//Now we now our tile-row will fit in the allocated pages after the translation
				//This is asserted below
				
				//If we are off to the right, we are good since there is nothing placed there
				if (totalY >= Length)
				{
					/*	Do y check after the x check, otherwise false positives are returned. 
						We cannot return, as we still need to check whether other rows
						of the tile will fit within the width.
					*/
					continue;
				}
				
				int rowStart = tile.RowStart[tileRowIndex];
				int rowEnd = tile.RowEnd[tileRowIndex];
				
				ulong translatedRow = row << (64 + totalX) % 64; //We add 64 so negative translations work too
				int pageIndex = (rowStart + totalX) / 64;
				
				GLDebug.Assert(pageIndex >= 0);
				GLDebug.Assert(pageIndex < context.UsedPageCount);
				
				int index = totalY * context.PageCount + pageIndex;

				if ((this[index] & translatedRow) != 0)
				{
					//There is already something there, so 
					return false;
				}
				
				//It is possible that the translation move the row over two pages
				//No more, since the tile width is max 64

				int endPageIndex = (rowEnd + totalX) / 64;
				
				if (endPageIndex != pageIndex) //second part of tile is on different page
				{
					GLDebug.Assert(endPageIndex == pageIndex + 1);
					GLDebug.Assert(endPageIndex < context.UsedPageCount);
					
					//totalX can never be negative for tiles that go over two pages.
					GLDebug.Assert(totalX >= 0);
					
					//retrieve the part of the row that was cut off in the previous calculation
					//i.e. the part of the row that goes on the next page
					translatedRow = row >> (64 - totalX);
					index += 1;

					if ((this[index] & translatedRow) != 0)
					{
						//There is already something there, so 
						return false;
					}
				}
			}

			//If nothing is blocking us, we can place
			return true;
		}

		public static IEqualityComparer<StripEnd> GetComparer() => new Comparer();

		public static ulong[] GetReflectedData(ulong[] data, int pagedOffset, int length, Context context)
		{
			ulong[] reflectedData = new ulong[data.Length];

			void ReflectPages()
			{
				for (int i = pagedOffset; i < data.Length; i++)
				{
					if (data[i] != 0)
					{
						reflectedData[i] = UlongOps.ReverseBits(data[i]);
					}
				}
			}

			void ReflectRows()
			{
				if (context.UsedPageCount == 1) return;
				
				for (int i = 0; i < length; i++)
				{
					for (int pageIndex = 0; pageIndex < context.UsedPageCount / 2; pageIndex++)
					{
						int otherIndex = context.UsedPageCount - 1 - pageIndex;
						int offset = pagedOffset + i * context.PageCount + pageIndex;
						int otherOffset = pagedOffset + i * context.PageCount + otherIndex;

						(reflectedData[offset], reflectedData[otherOffset]) = (reflectedData[otherOffset], reflectedData[offset]);
					}
				}
			}

			void CorrectShift()
			{
				if (context.LastRowWidth == 0)
				{
					return;
				}

				int shift = 64 - context.LastRowWidth;
				ulong mask = (1ul << shift) - 1ul;
	
				for (int i = 0; i < length; i++)
				{
					for (int pageIndex = 0; pageIndex < context.UsedPageCount - 1; pageIndex++)
					{
						int index = pagedOffset + i * context.PageCount + pageIndex;
						int nextIndex = index + 1;

						reflectedData[index] >>= shift;
						reflectedData[index] |= reflectedData[nextIndex] & mask;
					}
					
					int index1 = pagedOffset + i * context.PageCount + context.UsedPageCount - 1;
					reflectedData[index1] >>= shift;
				}
			}
			
			ReflectPages();
			ReflectRows();
			CorrectShift();

			return reflectedData;
		}
		
		private ulong GetReflectedRow(int y) =>
			y < 0
				? UlongOps.FullRow //TODO is this correct 
				: y < pagedLength
					? reflected[y + pagedOffset]
					: UlongOps.EmptyRow;
	}
	
	public static IEnumerable<PositionedTile<UlongTile>> TileRect(IEnumerable<UlongTile> tiles, int width, Func<Context, StripEnd, bool> canRuleOut)
	{
		if (width <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(width), "Must be positive");
		}

		var context = new Context(width, canRuleOut);

		int nodeCount = 0;
		long totalBranches = 0;

		int totalBranchesBatch = 0;
		int countInBatch = 0;
		
		//var openList = new System.Collections.Generic.Queue<TStripEnd>();
		var openList = new System.Collections.Generic.Stack<StripEnd>();
		openList.Push(new StripEnd());
		
		var comparer = StripEnd.GetComparer();
		var graph = new Graph<StripEnd, StripEnd>(comparer);

		int count = 0;
		
		while (openList.Any())
		{
			var stripEnd = openList.Pop();

			if (graph.Contains(stripEnd))
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
					if (count % 100000 == 0)
					{
						Console.WriteLine(count + " " + openList.Count + " Branch Factor" + (totalBranches / (float) nodeCount)
						                  + " Branch Factor" + (totalBranchesBatch / (float) countInBatch) + " Graph " + graph.Count );
						totalBranchesBatch = 0;
						countInBatch = 0;
					}
					count++;
					
					var newStripEnd = stripEnd.Place(context, empty, tile);

					//if this pair is not already in the graph
					graph.Add(stripEnd, newStripEnd/*, empty, tile*/);

					//newStripEnd can be null if placement results in a tile that cannot be tiled further by some criterion
					if (newStripEnd == null) continue;
					
					found++;
					
					if (!graph.Contains(newStripEnd))
					{
						openList.Push(newStripEnd);
						
						//Console.WriteLine(newStripEnd.ToString2(context));
					}
					
					if (newStripEnd.IsStraight)
					{
						Console.WriteLine("--------------------------");
						return Array.Empty<PositionedTile<UlongTile>>();
					}
				}
			}

			if (found == 0)
			{
				//Console.WriteLine(stripEnd.ToString());
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
