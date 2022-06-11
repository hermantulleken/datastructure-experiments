using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DataStructures;

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

			indexedImage[index] = nextColorIndex;
		}

		return (indexedImage, nextColorIndex);
	}
	
	public static IReadOnlyDictionary<int, int> GetMinRunLengths(IGrid<int> image)
	{
		//Assumption: there is no noise in the image
		
		var runLengths = new MinDict<int>(() => new Dictionary<int, int>());
		
		for (int j = 0; j < image.Height; j++)
		{
			int runStart = 0;
			int runColor = image[0, j];
			
			for (int i = 1; i < image.Width; i++)
			{
				int color = image[i, j];

				if (i == image.Width - 1)
				{
					int length = i - runStart + 1;
					runLengths.SetIfSmaller(runColor, length);
				}
				if (color != runColor)
				{
					int length = i - runStart;
					runLengths.SetIfSmaller(runColor, length);
					runStart = i;
				}
			}
		}

		return runLengths;
	}

	public (int color, int thickNess) GetLine(IReadOnlyDictionary<int, int> runLengths)
	{
		//Assumption: color with minimum run length is line, and that run length is the line thickness

		var minPair = runLengths.MinBy(pair => pair.Value);

		return (minPair.Key, minPair.Value);
	}
	
	public int UnitLength(IReadOnlyDictionary<int, int> runLengths, int lineColor) 
	{
		// r_i = cellSize * n_i - delta_i
		// 17, 25, 


		return 0;

	}

	public (int, int) GridSize(IGrid<int> image)
	{
		throw new NotImplementedException();
	}

	public (IGrid<bool>, IGrid<bool>) GetConnectivity(IGrid<int> image)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<IEnumerable<(int x, int y)>> GetTiles()
	{
		throw new NotImplementedException();
	}

	public IEnumerable<IEnumerable<(int x, int y)>> GetTiles(Grid<Color> image)
	{
		var (indexedImage, colorCount) = Indexify(image);
		var runLengths = GetMinRunLengths(indexedImage);
		var (lineColor, lineThickness) = GetLine(runLengths);
		runLengths = CleanRunLengths(runLengths, lineColor, lineThickness);

		
		
		throw new NotImplementedException();
	}
	

	private IReadOnlyDictionary<int, int> CleanRunLengths(IReadOnlyDictionary<int, int> runLengths, int lineColor, int lineThickness)
	{
		List<List<int>> runLengthClusters = new List<List<int>>();

		var newRunLengths = new Dictionary<int, int>();

		foreach (var (color, runLength) in runLengths)
		{
			if(color == lineColor) continue;
			
			var cluster = runLengthClusters.FirstOrDefault(cluster => cluster.Any(item => Math.Abs(item - runLength) <= lineThickness));

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
}
