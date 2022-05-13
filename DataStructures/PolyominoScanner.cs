using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace DataStructures;

public class MinDict<TKey>
{
	private IDictionary<TKey, int> dictionary;

	public int this[TKey key] => dictionary[key];

	public MinDict(Func<IDictionary<TKey, int>> factory) => dictionary = factory();

	public void SetIfSmaller(TKey key, int value)
	{
		if(!dictionary.ContainsKey(key) || value < dictionary[key])
		{
			dictionary[key] = value;
		}
	}

	public void AddRange(IDictionary<TKey, int> other)
	{
		foreach (var (key, value) in other)
		{
			SetIfSmaller(key, value);
		}
	}
}

public class PolyominoScanner
{
	public IDictionary<int, int> GetMinRunLengths(IGrid<int> image)
	{
		MinDict<int> GetUniqueRuns(IList<int> runs, int row)
		{
			var minDict = new MinDict<int>(() => new Dictionary<int, int>());

			if (runs.Count == 1)
			{
				minDict.SetIfSmaller(image[0, row], runs[0]);
				return minDict;
			}

			for (int i = 0; i < runs.Count - 1; i++)
			{
				int x0 = runs[i];
				int x1 = runs[i + 1];
				int color = image[x0, row];

				minDict.SetIfSmaller(color, x1 - x0);
			}

			return minDict;
		}

		IList<int> GetRunLengths(int row)
		{
			IList<int> runs = new List<int>();

			for (int i = 0; i < image.Width - 1; i++)
			{
				if (image[i, row] != image[i + 1, row])
				{
					runs.Add(i + 1);
				}
			}

			runs.Add(image.Width);
			return runs;
		}

		IDictionary<int, int> GetMinimumRunLengths_old(int row)
		{
			IDictionary<int, int> runs = new Dictionary<int, int>();
			
			int currentRunColor = image[row, 0];
			int runLength = 1;
			
			for (int i = 0; i < image.Width; i++)
			{
				if (runLength == 1) // we are restarting
				{
					currentRunColor = image[row, i];
					runLength++;
				}
				else if (currentRunColor == image[i, row])
				{
					runLength++;
				}
				else
				{
					runs[currentRunColor] = runLength;
					runLength = 1;
				}
			}

			if (runLength != 1)
			{
				runs[currentRunColor] = runLength;
			}
			//else we already wrote this in the dict

			return runs;
		}
		
		MinDict<int> GetMinimumRunLengths(int row)
		{
			if (image.Width < 2) throw new InvalidOperationException("Width must be at least 2.");
			
			var runs = GetRunLengths(row);
			
			return GetUniqueRuns(runs, row);
		}

		return null; //TODO: combine runlengths of all rows
	}

	public int LineThickNess(IGrid<int> image)
	{
		throw new NotImplementedException();
	}
	
	public int UnitLength(IGrid<int> image) 
	{
		throw new NotImplementedException();
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
}
