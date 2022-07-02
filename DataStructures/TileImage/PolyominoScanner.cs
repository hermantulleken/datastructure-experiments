using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DataStructures.Tiling;
using Gamelogic.Extensions;

namespace DataStructures.TileImage;

using Int2Stack = System.Collections.Generic.Stack<Int2>;

public class MinDict<TKey> : IReadOnlyDictionary<TKey, int>
{
	private readonly IDictionary<TKey, int> dictionary;

	public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

	public bool TryGetValue(TKey key, out int value) => dictionary.TryGetValue(key, out value);

	public int this[TKey key] => dictionary[key];
	public IEnumerable<TKey> Keys => dictionary.Keys;
	public IEnumerable<int> Values => dictionary.Values;

	public MinDict(Func<IDictionary<TKey, int>> factory) => dictionary = factory();

	public void SetIfSmaller(TKey key, int value)
	{
		if(!dictionary.ContainsKey(key) || value < dictionary[key])
		{
			dictionary[key] = value;
		}
	}
	
	public IEnumerator<KeyValuePair<TKey, int>> GetEnumerator() => dictionary.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public int Count => dictionary.Count;
}

public class PolyominoScanner
{

	public static (IGrid<int>, int colorCount) Indexify(IGrid<Color> image)
	{
		//Assumption: color values are exact
		
		var colors = new Dictionary<Color, int>();
		int nextColorIndex = 0;
		var indexedImage = new Grid<int>(image.Size);
		
		foreach(var index in image.Indices)
		{
			var color = image[index];

			if (!colors.ContainsKey(color))
			{
				colors[color] = nextColorIndex;
				nextColorIndex++;
			}

			indexedImage[index] = colors[color];
		}

		return (indexedImage, nextColorIndex);
	}

	//Assumption: there is no noise in the image
	public static IReadOnlyDictionary<int, int> GetMinRunLengths(IGrid<int> image)
	{
		var runLengths = new MinDict<int>(() => new Dictionary<int, int>());

		void GetMinYRunLengths()
		{
			for (int i = 0; i < image.Width; i++)
			{
				int runStart = 0;
				int runColor = image[i, 0];

				for (int j = 1; j < image.Height; j++)
				{
					int color = image[i, j];

					if (color != runColor)
					{
						int length = j - runStart;
						runLengths.SetIfSmaller(runColor, length);
						runStart = j;
						runColor = color;
					}

					if (j == image.Height - 1)
					{
						int length = j - runStart + 1;
						runLengths.SetIfSmaller(runColor, length);
					}
				}
			}
		}

		void GetMinXRunLengths()
		{
			for (int j = 0; j < image.Height; j++)
			{
				int runStart = 0;
				int runColor = image[0, j];

				for (int i = 1; i < image.Width; i++)
				{
					int color = image[i, j];

					if (color != runColor)
					{
						int length = i - runStart;
						runLengths.SetIfSmaller(runColor, length);
						runStart = i;
						runColor = color;
					}

					if (i == image.Width - 1)
					{
						int length = i - runStart + 1;
						runLengths.SetIfSmaller(runColor, length);
					}
				}
			}
		}
		
		GetMinXRunLengths();
		GetMinYRunLengths();

		return runLengths;
	}

	public static (int color, int thickNess) GetLine(IReadOnlyDictionary<int, int> runLengths)
	{
		//Assumption: color with minimum run length is line, and that run length is the line thickness

		var minPair = runLengths.MinBy(pair => pair.Value);

		return (minPair.Key, minPair.Value);
	}
	
	
	public static int GetUnitLength(IReadOnlyDictionary<int, int> runLengths, int lineColorIndex, int lineWidth) 
	{
		// r_i = cellSize * n_i - delta_i
		// 17, 25, 
		
		Debug.Assert(!runLengths.ContainsKey(lineColorIndex));

		return runLengths.Values.Min() + lineWidth; //Not completely correct; we relly need to take the GDC
	}

	public static (Int2 anchor, Int2 size) InterestingPartSize(IGrid<int> image, int lineColorIndex)
	{
		bool IsLinePixel(Int2 index) => image[index] == lineColorIndex;
		var firstIndex = image.Indices.First(IsLinePixel);
		var lastIndex = image.Indices.Last(IsLinePixel);
		
		Console.WriteLine(image[lastIndex]);

		return (firstIndex, lastIndex - firstIndex + Int2.One);
	}

	public static (Int2 gridSize, IGrid<bool>, IGrid<bool>) GetConnectivity(IGrid<int> image, Int2 anchor, Int2 size, int unit, int lineWidth, int lineColorIndex)
	{
		//Remove one line size from size so we can find a more reliable cell size below
		var sizeWithoutOutline = size - lineWidth * Int2.One;
		var gridSize = GLMath.RoundDiv(sizeWithoutOutline, unit);
		var centerAnchor = anchor + Int2.One * (unit / 2);

		var horizontal = new Grid<bool>(gridSize - Int2.Right);
		foreach (var gridIndex in horizontal.Indices)
		{
			var start = centerAnchor + gridIndex * unit ;
			horizontal[gridIndex] = IsXConnected(image, start, unit, lineColorIndex);
		}
		
		var vertical = new Grid<bool>(gridSize - Int2.Up);
		foreach (var gridIndex in vertical.Indices)
		{
			var start = centerAnchor + gridIndex * unit;
			vertical[gridIndex] = IsYConnected(image, start, unit, lineColorIndex);
		}

		return (gridSize, horizontal, vertical);
	}
	
	private static bool IsConnected(IGrid<int> image, Int2 start, Int2 direction, int length, int lineColorIndex)
	{
		Debug.Assert(image[start] != lineColorIndex);
		Debug.Assert(image[start + length * direction] != lineColorIndex);
		
		for (int i = 1; i < length - 1; i++)
		{
			var index = start + i * direction;
			
			if (image[index] == lineColorIndex)
			{
				return false;
			}
		}

		return true;
	}

	private static bool IsYConnected(IGrid<int> image, Int2 start, int length, int lineColorIndex) 
		=> IsConnected(image, start, Int2.Up, length, lineColorIndex);
	
	private static bool IsXConnected(IGrid<int> image, Int2 start, int length, int lineColorIndex)
		=> IsConnected(image, start, Int2.Right, length, lineColorIndex);
	

	private static IEnumerable<IEnumerable<Int2>> GetTiles(Int2 gridSize, IGrid<bool> horizontal, IGrid<bool> vertical)
	{
		var tiles = new List<IEnumerable<Int2>>();
		var inSomeTile = new Grid<bool>(gridSize, false);
		bool HasCellsNotInSomeTile() => inSomeTile.Indices.Any(index => !inSomeTile[index]);
		Int2 GetCellNotInSomeTile() => inSomeTile.Indices.First(index => !inSomeTile[index]);
		
		while (HasCellsNotInSomeTile())
		{
			var cellNotInTileYet = GetCellNotInSomeTile();
			var tile = GetConnectedCells(horizontal, vertical, cellNotInTileYet);
			
			tiles.Add(tile);
			foreach (var cell in tile)
			{
				inSomeTile[cell] = true;
			}
		}

		return tiles;
	}
	
	private static IReadOnlyDictionary<int, int> CleanRunLengths(IReadOnlyDictionary<int, int> runLengths, int lineColor, int lineWidth)
	{
		var runLengthClusters = new List<List<int>>();
		var newRunLengths = new Dictionary<int, int>();

		foreach (var (color, runLength) in runLengths)
		{
			if(color == lineColor) continue;
			
			var cluster = runLengthClusters.FirstOrDefault(cluster => cluster.Any(item => Math.Abs(item - runLength) <= lineWidth));

			if (cluster != null)
			{
				if (!cluster.Contains(runLength))
				{
					cluster.Add(runLength);
				}
			}
			else
			{
				runLengthClusters.Add(new List<int>{runLength});
			}
		}

		var validRunLengths = runLengthClusters.Select(cluster => cluster.Max());

		foreach (var (color, runLength) in runLengths)
		{
			if(color == lineColor) continue;

			int newRunLength = validRunLengths.MinBy(x => MathF.Abs(x - runLength));

			newRunLengths[color] = newRunLength;
		}

		return newRunLengths;
	}

	private static IEnumerable<Int2> GetConnectedNeighbors(IGrid<bool> horizontal, IGrid<bool> vertical, Int2 point)
	{
		var neighbors = new List<Int2>();

		void AddNeighbors(IGrid<bool> connectivity, Int2 direction)
		{
			if (connectivity.ContainsIndex(point))
			{
				if (connectivity[point])
				{
					neighbors.Add(point + direction);
				}
			}

			var otherNeighbor = point - direction;
			
			if (connectivity.ContainsIndex(otherNeighbor))
			{
				if (connectivity[otherNeighbor])
				{
					neighbors.Add(otherNeighbor);
				}
			}
		}
		
		AddNeighbors(horizontal, Int2.Right);
		AddNeighbors(vertical, Int2.Up);

		return neighbors;
	}

	private static IEnumerable<Int2> GetConnectedCells(IGrid<bool> horizontal, IGrid<bool> vertical, Int2 point)
	{
		var cellsWhoseNeighborsAreAlreadyAdded = new List<Int2>();
		var cellsWithUnaddedNeighbors = new Int2Stack();
		cellsWithUnaddedNeighbors.Push(point);
		
		var tile = new List<Int2>{point};

		while (cellsWithUnaddedNeighbors.Any())
		{
			var newPoint = cellsWithUnaddedNeighbors.Pop();
			var connectedNeighbors = GetConnectedNeighbors(horizontal, vertical, newPoint);

			foreach (var neighbor in connectedNeighbors)
			{
				if (tile.Contains(neighbor)) continue; //Already in tile, don't add again.
				tile.Add(neighbor);
					
				if (cellsWhoseNeighborsAreAlreadyAdded.Contains(neighbor)) continue; //Already processed, don't process again
					
				if (cellsWithUnaddedNeighbors.Contains(neighbor)) continue; //Already in list, don't add again.
				cellsWithUnaddedNeighbors.Push(neighbor);
			}

			if (cellsWhoseNeighborsAreAlreadyAdded.Contains(newPoint)) continue; //Already in this list, don't add again
			cellsWhoseNeighborsAreAlreadyAdded.Add(newPoint);
		}

		return tile;
	}

	public static IEnumerable<IEnumerable<Int2>> GetTiles(Grid<Color> image)
	{
		var (indexedImage, colorCount) = Indexify(image);
		
		Console.WriteLine($"Color count: {colorCount}");
		
		var runLengths = GetMinRunLengths(indexedImage);

		Console.WriteLine($"Minimum run lengths: {runLengths.ToPrettyString()}");
		
		var (lineColorIndex, lineWidth) = GetLine(runLengths);
		
		Console.WriteLine($"Line color index: {lineColorIndex}");
		Console.WriteLine($"lineWidth: {lineWidth}");
		
		//Remove the run lengths of the line
		runLengths = CleanRunLengths(runLengths, lineColorIndex, lineWidth);

		Console.WriteLine($"Clean minimum run lengths: {runLengths.ToPrettyString()}");

		int unit = GetUnitLength(runLengths, lineColorIndex, lineWidth);
		
		Console.WriteLine($"Unit length: {unit}");

		var (anchor, size) = InterestingPartSize(indexedImage, lineColorIndex);
		
		Console.WriteLine($"Point: {anchor}");
		Console.WriteLine($"Size: {size}");

		var (gridSize, horizontal, vertical) = GetConnectivity(indexedImage, anchor, size, unit, lineWidth, lineColorIndex);
		
		Console.WriteLine($"Grid size: {gridSize}");

		var tiles = GetTiles(gridSize, horizontal, vertical);
		
		Console.WriteLine($"Tiles {tiles.ToPrettyString()}");
		Console.WriteLine($"Tile count {tiles.Count()}");
		
		return tiles;
	}
	

	
}
