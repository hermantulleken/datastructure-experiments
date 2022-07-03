namespace DataStructures.TileImage;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Gamelogic.Extensions;

using Int2Stack = System.Collections.Generic.Stack<Int2>;

public static class PolyominoScanner
{
	public record Settings
	{
		/// <summary>
		/// True of tile borders overlap in an image.  
		/// </summary>
		/// <remarks>
		/// When tile borders overlap, all the borders are equally thin.
		/// When they don't overlap, borders that coincide with the edge
		/// of the tiling will be half as thin as borders in the interior
		/// of the tiling. <see langword="true"/> by default.
		/// </remarks>
		public bool BordersOverlap = true;
		
		/// <summary>
		/// If lines are pure, they are all the exact same color and thickness.
		/// </summary>
		/// <remarks> If line are not pure, extra steps are taken to find all
		/// the lines, and it may be more difficult to interpret images where
		/// the tiles are small in comparison to the line thickness. 
		/// </remarks>
		public bool LinesArePure = true;
	}
	
	/// <summary>
	/// Represents a line.
	/// </summary>
	private struct Line
	{
		/// <summary>
		/// The index of the color of the line.
		/// </summary>
		public int Colorindex;
	
		/// <summary>
		/// The thickness of the line.
		/// </summary>
		public int Thickness;

		public override string ToString() => $"Color Index: {Colorindex}, Thickness: {Thickness}";
	}

	private struct Rect
	{
		public Int2 Anchor;
		public Int2 Size;

		public override string ToString() => $"Anchor: {Anchor}, Size: {Size}";
	}
	
	private sealed class ConnectivityGrid
	{
		private readonly IGrid<bool> horizontal;
		private readonly IGrid<bool> vertical;
		
		/*
			Checks how cells are connected to their right and upper neighbors. 
			
			The grid size is the tiling grid size, i.e. how many cells that grid is 
			wide and high. 
			
			If horizontallyConnected[index] is true, then index and index + Int2.Right does not have a vertical line between them.
			If horizontallyConnected[index] is true, then index and index + Int2.Up does not have a horizontal line between them.
		*/
		public ConnectivityGrid(IGrid<int> image, Rect tilingRect, Int2 gridSize, int cellSize, Line line)
		{
			var centerAnchor = tilingRect.Anchor  + Int2.One * (line.Thickness + cellSize / 2);

			IGrid<bool> GetConnectivityInDirection(Int2 direction)
			{
				var isConnected = new Grid<bool>(gridSize - Int2.Right);
			
				foreach (var gridIndex in isConnected.Indices)
				{
					var start = centerAnchor + gridIndex * cellSize ;
					isConnected[gridIndex] = IsConnected(image, start,  direction, cellSize, line);
				}

				return isConnected;
			}

			horizontal = GetConnectivityInDirection(Int2.Right);
			vertical = GetConnectivityInDirection(Int2.Up);
		}
		
		/*
			Checks whether two neighboring cells are connected.
			
			start: Roughly in the center of the cell to check (in pixel units, not cell units). 
			direction: Where the neighbor lies. The center of the neighbor is start + cellSize * direction.
		*/
		private static bool IsConnected(IGrid<int> image, Int2 start, Int2 direction, int cellSize, Line line)
		{
			Debug.Assert(image[start] != line.Colorindex);
			Debug.Assert(image[start + cellSize * direction] != line.Colorindex);
			
			for (int i = 1; i < cellSize - 1; i++)
			{
				var index = start + i * direction;
				
				if (image[index] == line.Colorindex)
				{
					return false;
				}
			}

			return true;
		}
		
		private IEnumerable<Int2> GetConnectedNeighbors(Int2 point)
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

		public IEnumerable<Int2> GetConnectedCells(Int2 point)
		{
			var cellsWhoseNeighborsAreAlreadyAdded = new List<Int2>();
			var cellsWithUnaddedNeighbors = new Int2Stack();
			cellsWithUnaddedNeighbors.Push(point);
			
			var tile = new List<Int2>{point};

			while (cellsWithUnaddedNeighbors.Any())
			{
				var newPoint = cellsWithUnaddedNeighbors.Pop();
				var connectedNeighbors = GetConnectedNeighbors(newPoint);

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
	}

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


	public static (Int2 size, IEnumerable<IEnumerable<Int2>> tiles) GetTiles(Grid<Color> image, Settings settings)
	{
		var (indexedImage, colorCount) = ConvertToIndexedImage(image);
		
		Console.WriteLine($"Color count: {colorCount}");
		
		var runLengths = GetMinRunLengths(indexedImage);

		Console.WriteLine($"Minimum run lengths: {runLengths.ToPrettyString()}");
		
		var line = GetLine(runLengths);

		if (!settings.LinesArePure)
		{
			RecolorLines(indexedImage, runLengths, line);
			//Update run lengths and line properties after finding more line colors.
			runLengths = GetMinRunLengths(indexedImage);
			Console.WriteLine($"Minimum run lengths: {runLengths.ToPrettyString()}");
			line = GetLine(runLengths);
		}
		
		Console.WriteLine($"Line: {line}");

		//Remove the run lengths of the line
		runLengths = RemoveLinesFromRunLength(runLengths, line);

		Console.WriteLine($"Clean minimum run lengths: {runLengths.ToPrettyString()}");

		int cellSize = GetCellSize(runLengths, line, settings.BordersOverlap);
		
		Console.WriteLine($"Cell size: {cellSize}");

		var tilingRect = InterestingPartRect(indexedImage, line);
		var gridSize = GetGridSize(tilingRect, cellSize, line);

		Console.WriteLine($"Tiling Rect: {tilingRect}");

		var connectivityGrid = new ConnectivityGrid(indexedImage, tilingRect, gridSize, cellSize, line);
		
		Console.WriteLine($"Grid size: {gridSize}");

		var tiles = GetTiles(gridSize, connectivityGrid);
		
		Console.WriteLine($"Tiles {tiles.ToPrettyString()}");
		Console.WriteLine($"Tile count {tiles.Count()}");
		
		return (gridSize, tiles);
	}

	private static Int2 GetGridSize(Rect tilingRect, int cellSize, Line line)
	{
		//Remove one line size from size so we can find a more reliable cell size below
		var sizeWithoutOutline = tilingRect.Size - line.Thickness * Int2.One;
		var gridSize = GLMath.RoundDiv(sizeWithoutOutline, cellSize);
		return gridSize;
	}

	/*
		Assigns an index to each color in the given image, and returns a grid where the
		value in a position is the index of the color at the position in the given grid.
		Assumption: color values are exact.
	*/
	private static (IGrid<int>, int colorCount) ConvertToIndexedImage(IGrid<Color> image)
	{
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
	
	/*
		For each color, find the minimum run length (horizontally and vertically).
		A run is a contiguous set of pixels in the same row or column with the same 
		color. The length of the run is how many pixels are in it.  
		
		Assumption: there is no noise in the image.
	*/
	private static IReadOnlyDictionary<int, int> GetMinRunLengths(IGrid<int> image)
	{
		var runLengths = new MinDict<int>(() => new Dictionary<int, int>());

		void GetMinVerticalRunLengths()
		{
			for (int i = 0; i < image.Width; i++)
			{
				int runStart = 0;
				int runColor = image[i, 0];

				for (int j = 1; j < image.Height; j++)
				{
					int color = image[i, j];

					//If the colors are different, we are at the end of the run.
					if (color != runColor)
					{
						int length = j - runStart;
						runLengths.SetIfSmaller(runColor, length);
						runStart = j;
						runColor = color;
					}

					// If we are at the end of the column, we are also at the end of the current run. 
					// This can also happen if the colors changed at this pixel, in which case
					// the new run is only one pixel long. 
					if (j == image.Height - 1)
					{
						int length = j - runStart + 1;
						runLengths.SetIfSmaller(runColor, length);
					}
				}
			}
		}

		//The same as the method above, with x and y swapped. 
		void GetMinHorizontalRunLengths()
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
		
		GetMinHorizontalRunLengths();
		GetMinVerticalRunLengths();

		return runLengths;
	}

	/*
		From the set of minimum run lengths for each colors, returns the
		color and run length of the smallest one, which is interpreted as
		the line color in the image and the thickness of the line. 
		
		Assumption: color with minimum run length is line, and that run length 
		is the line thickness.
	*/
	private static Line GetLine(IReadOnlyDictionary<int, int> minRunLengths)
	{
		(int colorIndex, int thickness) = minRunLengths.MinBy(pair => pair.Value);

		return new Line
		{
			Colorindex = colorIndex, 
			Thickness = thickness
		};
	}
	
	/*
		Returns the size of the cells in the grid implicit in the image from the run lengths and line properties.
	*/
	//TODO: This is not completely correct; we really need to take the GDC. 
	// For example, this will report incorrect results if all the tiles are 3x2 rectangles. 
	private static int GetCellSize(IReadOnlyDictionary<int, int> runLengths, Line line, bool bordersOverlap) 
	{
		Debug.Assert(!runLengths.ContainsKey(line.Colorindex));

		return bordersOverlap ? 
			runLengths.Values.Min() + line.Thickness:
			runLengths.Values.Min() + 2 * line.Thickness;
	}

	/*
		The interesting part of the image is within the outermost border. 
		This, however, may not work if the image is surrounded by a thin 
		background that is not part of the image, as it may be interpreted
		as a line. 
		
		It is better to crop images, so that anchor is always Int2.Zero
		int size below corresponds with the grid size. 
	*/
	private static Rect InterestingPartRect(IGrid<int> image, Line line)
	{
		bool IsLinePixel(Int2 index) => image[index] == line.Colorindex;
		var firstIndex = image.Indices.First(IsLinePixel);
		var lastIndex = image.Indices.Last(IsLinePixel);
		
		Console.WriteLine(image[lastIndex]);

		return new Rect{Anchor = firstIndex, Size = lastIndex - firstIndex + Int2.One};
	}

	
	
	

	private static IEnumerable<IEnumerable<Int2>> GetTiles(Int2 gridSize, ConnectivityGrid connectivityGrid)
	{
		var tiles = new List<IEnumerable<Int2>>();
		var inSomeTile = new Grid<bool>(gridSize);
		bool HasCellsNotInSomeTile() => inSomeTile.Indices.Any(index => !inSomeTile[index]);
		Int2 GetCellNotInSomeTile() => inSomeTile.Indices.First(index => !inSomeTile[index]);
		
		while (HasCellsNotInSomeTile())
		{
			var cellNotInTileYet = GetCellNotInSomeTile();
			var tile = connectivityGrid.GetConnectedCells(cellNotInTileYet);
			
			tiles.Add(tile);
			foreach (var cell in tile)
			{
				inSomeTile[cell] = true;
			}
		}

		return tiles;
	}
	
	private static IReadOnlyDictionary<int, int> RemoveLinesFromRunLength(IReadOnlyDictionary<int, int> runLengths, Line line)
	{
		var runLengthClusters = new List<List<int>>();
		var newRunLengths = new Dictionary<int, int>();

		foreach (var (colorIndex, runLength) in runLengths)
		{
			if(colorIndex == line.Colorindex) continue;
			
			var cluster = runLengthClusters.FirstOrDefault(cluster => cluster.Any(item => Math.Abs(item - runLength) <= line.Thickness));

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

		foreach (var (colorIndex, runLength) in runLengths)
		{
			if(colorIndex == line.Colorindex) continue;

			int newRunLength = validRunLengths.MinBy(x => MathF.Abs(x - runLength));

			newRunLengths[colorIndex] = newRunLength;
		}

		return newRunLengths;
	}

	

	private static void RecolorLines(IGrid<int> image, IReadOnlyDictionary<int, int> runLengths, Line line)
	{
		foreach (var (colorIndex, length) in runLengths)
		{
			if (length <= 2 * line.Thickness && colorIndex != line.Colorindex)
			{
				ReplaceColor(image, colorIndex, line.Colorindex);
			}
		}
	}

	private static void ReplaceColor(IGrid<int> image, int originalColor, int newColor)
	{
		foreach (var index in image.Indices)
		{
			if(image[index] == originalColor)
			{
				image[index] = newColor;
			}
		}
	}
}
