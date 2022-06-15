using System.Collections.Generic;
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
	
	
	public static IEnumerable<Int2> NameToPoly(string cellsStr)
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
}
