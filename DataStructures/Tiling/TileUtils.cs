using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DataStructures.Tiling;

public static class TileUtils
{
	public class LeftMostInLowestRowComparer : IComparer<Int2>
	{
		public int Compare(Int2 point1, Int2 point2)
		{
			int yComparison = point1.Y.CompareTo(point2.Y);
			return yComparison != 0 ? yComparison : point1.X.CompareTo(point2.X);
		}
	}
	
	private static readonly IComparer<Int2> PointComparer = new LeftMostInLowestRowComparer();
	
	
	public static IEnumerable<Int2> NameToPoints(this string cellsStr)
	{
		string[] rows = cellsStr.Split("/");
		rows = rows.ToArray();

		var tile = new List<Int2>();

		for (int j = 0; j < rows.Length; j++)
		{
			string row = rows[j];
			for (int i = 0; i < row.Length; i++)
			{
				if (row[i] == '*')
				{
					tile.Add(new Int2(i, j));
				}
			}
		}

		return tile;
	}

	/// <summary>
	/// Converts a polyomino written in Dahlke notation (see https://github.com/eklhad/trec) to starts and periods notation.
	/// </summary>
	public static string DahlkeToName(string dahlkeStr)
	{
		ArgumentException ArgumentException() => new ArgumentException(null, nameof(dahlkeStr));
		
		string PairToName((string first, string rest) pair) 
			=> DahlkeToName(pair.first) + "/" + DahlkeToName(pair.rest);
	
		string HexToName(string hex)
		{ 
			string binaryPresentation = Convert.ToString(Convert.ToInt32(hex, 16), 2).PadLeft(hex.Length * 4, '0');
			int indexOfLast1 = binaryPresentation.LastIndexOf("1", StringComparison.Ordinal);
			Console.WriteLine(indexOfLast1);
	 
			if(indexOfLast1 != -1)
			{
				binaryPresentation = binaryPresentation[..(indexOfLast1 + 1)];
			}
	 
			return binaryPresentation
				.Replace("0", ".")
				.Replace("1", "*");
		}
		
		
		(string firstToken, string rest) Split(string str)
		{
			//Assume we have two tokens
	
			return str[2] switch
			{
				'+' => (str[0..3], str[3..]),
				'{' when str[5] != '}' => throw ArgumentException(),
				'{' => (str[0..2] + str[3..5], str[6..]),
				_ => (str[0..2], str[2..])
			};
		}
	
		int length = dahlkeStr.Length;
	
		return length switch
		{
			2 => HexToName(dahlkeStr),
			3 when dahlkeStr[2] != '+' => throw ArgumentException(),
			3 => HexToName(dahlkeStr[0..2] + "8"),
			6 when dahlkeStr[2] == '{' && dahlkeStr[5] != '}' => throw ArgumentException(),
			6 when dahlkeStr[2] == '{' => HexToName(dahlkeStr[0..2] + dahlkeStr[3..5]),
			_ => PairToName(Split(dahlkeStr))
		};
	}

	public static IEnumerable<Int2> Rotate90(this IEnumerable<Int2> points) => points.Select(p => p.Rotate90());
	public static IEnumerable<Int2> Rotate180(this IEnumerable<Int2> points) => points.Select(p => p.Rotate180());
	public static IEnumerable<Int2> Rotate270(this IEnumerable<Int2> points) => points.Select(p => p.Rotate270());
	public static IEnumerable<Int2> ReflectX(this IEnumerable<Int2> points) => points.Select(p => p.ReflectX());
	public static IEnumerable<Int2> ReflectXRotate90(this IEnumerable<Int2> points) => points.Select(p => p.ReflectXRotate90());
	public static IEnumerable<Int2> ReflectXRotate180(this IEnumerable<Int2> points) => points.Select(p => p.ReflectXRotate180());
	public static IEnumerable<Int2> ReflectXRotate270(this IEnumerable<Int2> points) => points.Select(p => p.ReflectXRotate270());

	public static IEnumerable<Int2> Normalize(this IEnumerable<Int2> points)
	{
		var min = points.MinBy(x=>x, PointComparer);

		return points.Select(x => x - min);
	}
	
	public static (int order, Int2 size, IEnumerable<PositionedTile<TTile>> tiling) Summarize<TTile>(this IEnumerable<PositionedTile<TTile>> tiling)
		where TTile : ITile
	{
		int order = tiling.Count();
		int maxX = tiling.SelectMany(x => x.Points).Max(cell => cell.X);
		int maxY =  tiling.SelectMany(x =>  x.Points).Max(cell => cell.Y);

		Int2 size = new(maxX + 1, maxY + 1);

		return (order, size, tiling);
	}
	
	public static UlongTile ToUlongTile(this IEnumerable<Int2> points) => new UlongTile(points);

	public static IEnumerable<Int2>[] GetAllSymmetriesNormalized(this IEnumerable<Int2> tile)
	{
		var tileArray = new[]
		{
			tile.Normalize(), 
			tile.ReflectX().Normalize(),
			tile.Rotate180().Normalize(), 
			tile.ReflectXRotate180().Normalize(),
				
			tile.Rotate90().Normalize(),
			tile.Rotate270().Normalize(),
			tile.ReflectXRotate90().Normalize(),
			tile.ReflectXRotate270().Normalize()
		};

		return tileArray;
	}
	
	/// <summary>
	/// Gets all symmetries of a tile that is distinct from the given tile. 
	/// </summary>
	public static IEnumerable<Int2>[] GetDistinctSymmetriesNormalized(this IEnumerable<Int2> tile)
	{
		var tileArray = new[]
		{
			tile.Normalize(), 
			tile.ReflectX().Normalize(),
			tile.Rotate180().Normalize(), 
			tile.ReflectXRotate180().Normalize(),
				
			tile.Rotate90().Normalize(),
			tile.Rotate270().Normalize(),
			tile.ReflectXRotate90().Normalize(),
			tile.ReflectXRotate270().Normalize()
		};

		var neededOrientations = new List<IEnumerable<Int2>>()
		{
			tileArray[0]
		};
		
		neededOrientations.AddRange(tileArray.Skip(1).Where(tile2 => !tile2.IsSame(tileArray[0])));

		return neededOrientations.ToArray();
	}

	private static bool IsSame(this IEnumerable<Int2> tile1, IEnumerable<Int2> tile2)
	{
		if (tile1.Count() != tile2.Count())
		{
			return false;
		}
		
		tile1.AssertIsDistinct();
		tile2.AssertIsDistinct();

		return tile1.All(tile2.Contains);
	}
	
	[Conditional(GLDebug.UseAsserts)]
	private static void AssertIsDistinct(this IEnumerable<Int2> list)
	{
		var tile1Distinct = list.Distinct();
		GLDebug.Assert(tile1Distinct.Count() == list.Count());
	}
	
	public static bool HasPattern(PagedTiler.Context context, PagedTiler.StripEnd stripEnd, Pattern pattern)
	{
		bool Match(int page, int offsetX, int offsetY)
		{
			for (int i = 0; i < pattern.Rows.Length; i++)
			{
				int rowIndex = i + offsetY;
				ulong row = stripEnd[rowIndex * context.PageCount + page];
				ulong mask = pattern.Mask[i] << offsetX;
				ulong patternRow = pattern.Rows[i] << offsetX;

				if ((row & mask) != patternRow)
				{
					return false;
				}
			}
			
			return true;
		}

		int yStart = pattern.IsFirstRowBlocked ? -1 : 0;

		for (int page = 0; page < context.UsedPageCount; page++)
		{
			int pageWidth = context.GetWidth(page);
			
			for (int patternYOffset = yStart; patternYOffset < stripEnd.Length - pattern.Rows.Length + 1; patternYOffset++)
			{
				for (int patternXOffset = 0; patternXOffset < pageWidth - pattern.Width + 1; patternXOffset++)
				{
					if (Match(page, patternXOffset, patternYOffset))
					{
						return true;
					}
				}
			}
		}
		
		return false;
	}
	
	public static bool HasPattern<TWidth, TTile>(PagedTiler.StripEnd stripEnd, Pattern pattern)
		where TWidth : IWidth
		where TTile : ITile<TWidth, TTile>
	{
		bool Match(int offsetX, int offsetY)
		{
			for (int i = 0; i < pattern.Rows.Length; i++)
			{
				int rowIndex = i + offsetY;
				ulong row = stripEnd[rowIndex];
				ulong mask = pattern.Mask[i] << offsetX;
				ulong patternRow = pattern.Rows[i] << offsetX;

				if ((row & mask) != patternRow)
				{
					return false;
				}
			}
			
			return true;
		}

		int yStart = pattern.IsFirstRowBlocked ? -1 : 0;

		for (int patternYOffset = yStart; patternYOffset < stripEnd.Length - pattern.Rows.Length + 1; patternYOffset++)
		{
			for (int patternXOffset = 0; patternXOffset < TWidth.Width - pattern.Width + 1; patternXOffset++)
			{
				if (Match(patternXOffset, patternYOffset))
				{
					return true;
				}
			}
		}
		
		return false;
	}

	//TODO These methods are not correct; the part after the first row should check against the inverted pattern.
	[Obsolete]
	public static bool HasPattern(ulong pattern, ulong mask, int patternWidth, PagedTiler.StripEnd potentialEnd, PagedTiler.Context context)
	{
		for (int i = 0; i < 1; i++)
		{
			int rowOffset = i * context.PageCount;

			for (int pageIndex = 0; pageIndex < context.UsedPageCount; pageIndex++)
			{
				ulong shiftedPattern = pattern;
				ulong shiftedMask = mask;
				int width = context.GetWidth(pageIndex);
				ulong page = potentialEnd[rowOffset + pageIndex];
				
				for (int j = 0; j <= width - patternWidth; j++)
				{
					if ((page & shiftedMask) == shiftedPattern)
					{
						return true;
					}

					shiftedPattern <<= 1;
					shiftedMask <<= 1;
				}
			}
		}
		
		for (int i = 1; i < potentialEnd.Length; i++)
		{
			int rowOffset = i * context.PageCount;
			int previousRowOffset = rowOffset - context.PageCount;

			for (int pageIndex = 0; pageIndex < context.UsedPageCount; pageIndex++)
			{
				ulong shiftedPattern = pattern;
				ulong shiftedMask = mask;
				int width = context.GetWidth(pageIndex);
				ulong page = potentialEnd[rowOffset + pageIndex];
				ulong previous = potentialEnd[previousRowOffset + pageIndex];
				
				for (int j = 0; j <= width - patternWidth; j++)
				{
					if ((page & shiftedMask) == shiftedPattern)
					{
						return true;
					}

					shiftedPattern <<= 1;
					shiftedMask <<= 1;
				}
			}
		}
		
		return false;
	}
	
	[Obsolete]
	public static bool HasPattern(ulong pattern, ulong mask, int patternWidth, Tiler.UlongStripEnd potentialEnd, Tiler.Context context)
	{
		for (int i = 0; i < 1; i++)
		{
			ulong shiftedPattern = pattern;
			ulong shiftedMask = mask;
			ulong row = potentialEnd[i];
			
			for (int j = 0; j <= context.Width - patternWidth; j++)
			{
				if ((row & shiftedMask) == shiftedPattern)
				{
					return true;
				}

				shiftedPattern <<= 1;
				shiftedMask <<= 1;
			}
		}
		
		for (int i = 1; i < potentialEnd.Length; i++)
		{
			ulong shiftedPattern = pattern;
			ulong shiftedMask = mask;
			ulong row = potentialEnd[i];
			ulong previous = potentialEnd[i - 1];
			
			for (int j = 0; j <= context.Width - patternWidth; j++)
			{
				if ((row & shiftedMask) == shiftedPattern && (previous & shiftedMask) == shiftedMask)
				{
					return true;
				}

				shiftedPattern <<= 1;
				shiftedMask <<= 1;
			}
		}

		return false;
	}
}
