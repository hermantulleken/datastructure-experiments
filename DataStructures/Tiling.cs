using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gamelogic.Extensions;

namespace DataStructures;

public sealed class ImmutableStripEnd
{
	public class Comparer : IEqualityComparer<ImmutableStripEnd>
	{
		public bool Equals(ImmutableStripEnd strip1, ImmutableStripEnd strip2)
		{
			if (ReferenceEquals(strip1, strip2)) return true;
			if (ReferenceEquals(strip1, null)) return false;
			if (ReferenceEquals(strip2, null)) return false;

			return 
				strip1.width == strip2.width
				&& strip1.length == strip2.length 
				&& strip1.Cells.All(cell => strip1[cell] == strip2[cell]);
		}

		//TODO implement correctly
		public int GetHashCode(ImmutableStripEnd obj) => obj.hashCode;
	}
	
	private bool[,] data;

	private int width;
	private int length;
	private int offset;
	private readonly int hashCode;

	public bool IsStraight => length == 0;

	private bool this[Int2 point] => point.Y < 0 || point.Y < length && data[point.X, point.Y + offset];

	IEnumerable<Int2> Cells
	{
		get
		{
			for (int j = 0; j < length; j++)
			{
				for (int i = 0; i < width; i++)
				{
					yield return new Int2(i, j);
				}
			}
		}
	}

	public ImmutableStripEnd(int width)
	{
		data = null;
		this.width = width;
		length = 0;
		offset = 0;
		hashCode = 0;
	}

	public ImmutableStripEnd(ImmutableStripEnd other, bool reflect)
	{
		if (reflect)
		{
			width = other.width;
			offset = other.offset;
			length = other.length;
			
			data = other.data ==  null ? null : new bool[other.data.GetLength(0), other.data.GetLength(1)];

			if (data != null)
			{
				for (int j = 0; j < length; j++)
				{
					for (int i = 0; i < width; i++)
					{
						int newI = width - 1 - i;

						data[newI, j] = other.data[i, j];
					}
				}
			}
			
			
			hashCode = data == null ? 0 : GetRowHash(0); 
		}
	}

	public override string ToString()
	{
		string s = "[";
		foreach (var cell in Cells)
		{
			if (cell.X == 0)
			{
				s += "/";
			}
			
			s += this[cell] ? "1" : "0";
		}

		return s;
	}

	public override int GetHashCode() => hashCode;

	private int GetRowHash(int row)
	{
		int hash = 0;
		int place = 0;
			
		for(int i = 0; i < width; i++)
		{
			hash += (data[i, row] ? 1 : 0) << place;
			place++;
		}

		return hash;
	}
	
	private ImmutableStripEnd(ImmutableStripEnd stripEnd, Int2 position, IEnumerable<Int2> cells)
	{
		
		void InitializeData()
		{
			int maxY = cells.Max(c => c.Y) + position.Y;
			length = Math.Max(stripEnd.length, maxY + 1);
			width = stripEnd.width;
			
			data = new bool[width, length];

			foreach (var cell in stripEnd.Cells)
			{
				data[cell.X, cell.Y] = stripEnd[cell];
			}

			foreach (var cell in cells)
			{
				data[cell.X + position.X, cell.Y + position.Y] = true;
			}
		}
		
		void ReduceFullRows()
		{
			var empty = FindEmpty();

			if (empty.Y != 0)
			{
				offset = empty.Y;
				length -= offset;
			}
		}
		
		InitializeData();
		ReduceFullRows();

		var empty = FindEmpty();
		Debug.Assert(empty.Y == 0);

		if (IsStraight)
		{
			data = null;//no need to hold on to this
			hashCode = 0;
		}
		else
		{
			hashCode = data == null ? 0 : GetRowHash(0);
		}
	}

	public ImmutableStripEnd Place(Int2 position, IEnumerable<Int2> cells) => new(this, position, cells);

	public Int2 FindEmpty()
	{
		if (IsStraight)
		{
			return Int2.Zero; //This is outside the grid!
		}

		foreach (var cell in Cells)
		{
			if (!this[cell])
			{
				return cell;
			}
		}
		
		return new Int2(0, length);
	}

	public bool IsEmpty(Int2 position, IEnumerable<Int2> cells)
	{
		bool InRangeAndEmpty(Int2 point) => point.X >= 0 && point.X < width && !this[point];
		
		return cells.All(cell => InRangeAndEmpty(position + cell));
	}

	public static IEqualityComparer<ImmutableStripEnd> GetComparer() => new Comparer();

	public ImmutableStripEnd ReflectAlongVertical() => new ImmutableStripEnd(this, true);
}

public class DoubleEndedList<T>
{
	
	
	private T[] data;
	private int offset; //where is item 0
	private int rightEmpty;
	private int Capacity => data.Length;
	
	public bool IsReadOnly => false;
	public int Count => rightEmpty - offset;
	private bool FullLeft => offset == 0;
	private bool FullRight => rightEmpty == Capacity - 1;

	public bool CanTakeRight(int count) => rightEmpty + count < Capacity;
	
	
	private bool Any() => offset < rightEmpty;
	
	

	public T this[int index]
	{
		get => data[ValidateInRange(index + offset)];
		set
		{
			if (InRange(index))
			{
				data[index + offset] = value;
				return;
			}

			switch (index)
			{
				case > 0 when offset + index >= Capacity:
					Expand(index + offset - Count);
					break;
				case <= 0 when offset + index < 0:
					Expand(Math.Abs(offset + index));
					break;
			}
			
			data[index + offset] = value;
		}
	}

	private int ValidateInRange(int index) 
		=> InRange(index) ? index : throw new ArgumentOutOfRangeException(nameof(index));
	
	public DoubleEndedList(int capacity)
	{
		data = new T[capacity];
		rightEmpty = offset = capacity/2;
	}

	//Right
	public void Add(T obj, int count)
	{
		if (!CanTakeRight(count))
		{
			Expand(count);
		}

		for (int i = 0; i < count; i++)
		{
			data[rightEmpty] = obj;
			rightEmpty++;
		}
	}
	
	public void AddFront(T obj)
	{
		if (FullLeft)
		{
			Expand(1);
		}

		offset--;
		data[offset] = obj;
	}

	public void Remove()
	{
		if (!Any()) throw new InvalidOperationException();

		rightEmpty--;
		data[rightEmpty] = default;
	}

	public void RemoveFront() => RemoveFront(1);
	
	public void RemoveFront(int count)
	{
		if(Count < count) throw new InvalidOperationException();

		for (int i = 0; i < count; i++)
		{
			data[offset + i] = default;
		}
		
		offset += count;
	}

	public void Clear()
	{
		for (int i = offset; i < rightEmpty; i++)
		{
			data[i] = default;
		}
		
		rightEmpty = offset = Capacity/2;
	}

	private void Expand(int count)
	{
		int newCapacity = 2 * count;

		while (newCapacity < Count + count)
		{
			newCapacity *= 2;
		}
		
		var newData = new T[newCapacity];
		int newOffset = (newCapacity - Capacity) / 2;

		for (int i = 0; i < Count; i++)
		{
			newData[i + newOffset] = data[i + offset];
		}

		data = newData;
		offset = newOffset;
		rightEmpty = offset + Count;
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = offset; i < rightEmpty; i++)
		{
			yield return data[i];
		}
	}

	public bool InRange(int index) => index >= 0 && index < Count;
}

//TODO Immutable?
/*
public class StripEnd
{
	public class EdgeComparer : IEqualityComparer<StripEnd>
	{
		public bool Equals(StripEnd x, StripEnd y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (ReferenceEquals(x, null)) return false;
			if (ReferenceEquals(y, null)) return false;
			if (x.GetType() != y.GetType()) return false;
			
			return x.Eq(y);
		}

		//TODO implement correctly
		public int GetHashCode(StripEnd obj)
		{
			return (obj.rows != null ? obj.rows.GetHashCode() : 0);
		}
	}
	
	private int Width => rows.Length;
	private DoubleEndedList<bool>[] rows;

	private int length;

	public StripEnd(int width)
	{
		rows = new DoubleEndedList<bool>[width];

		for (int i = 0; i < width; i++)
		{
			rows[i] = new DoubleEndedList<bool>(16);
		}

		length = 0;
	}
	
	public Int2 GetFirstEmpty()
	{
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				var point = new Int2(i, j);
				if (!this[point])
				{
					return point;
				}
			}
		}
		
		//TODO: Check whether grid is reduce.
		return Int2.Zero; //The grid has no entries, so the first empty is outside the grid. This assumes the grid is reduced
	}

	public void Reduce()
	{
		int reduceCount = 0;

		bool AllFull() => rows.All(row => row[reduceCount]);
		
		while(AllFull())
		{
			reduceCount++;
		}

		foreach (var row in rows)
		{
			row.RemoveFront(reduceCount);
		}
	}

	public bool Eq(StripEnd other)
	{
		if (Width != other.Width)
		{
			return false;
		}

		for (int i = 0; i < Width; i++)
		{
			if (!Eq(rows[i], other.rows[i]))
			{
				return false;
			}
		}

		return true;
	}

	private bool Eq(DoubleEndedList<bool> list1, DoubleEndedList<bool> list2)
	{
		for (int i = 0; i < Math.Max(list1.Count, list2.Count); i++)
		{
			if (GetItemAt(list1, i) != GetItemAt(list2, i))
			{
				return false;
			}
		}

		return true;
	}

	private static int Compare(DoubleEndedList<bool> list1, DoubleEndedList<bool> list2)
	{
		for (int i = 0; i < Math.Max(list1.Count, list2.Count); i++)
		{
			bool item1 = GetItemAt(list1, i);
			bool item2 = GetItemAt(list2, i);
			
			if (item1 != item2)
			{
				return item1 ? 1 : -1;
			}
		}

		return 0;
	}

	public int Compare(StripEnd other)
	{
		if (Width != other.Width)
		{
			return Width > other.Width ? 1 : -1;
		}
		
		for (int i = 0; i < Width; i++)
		{
			int rowsComparison = Compare(rows[i], other.rows[i]);
			
			if (rowsComparison != 0)
			{
				return rowsComparison;
			}
		}

		return 0;
	}

	private static bool GetItemAt(DoubleEndedList<bool> list, int index) => index < 0 || (index < list.Count && list[index]);

	public bool this[Int2 index]
	{
		get => GetItemAt(rows[index.Y], index.X);
		set
		{
			rows[index.Y][index.X] = value;
			length = Math.Max(length, rows[index.Y].Count);
		}
	}

	public static IEqualityComparer<StripEnd> GetComparer() => new EdgeComparer();

	//Only works if stripEnd is reduced!
	public bool IsStraight() => length == 0;
}
*/

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
	
	private static IComparer<Int2> pointComparer = new LeftMostInLowestRowComparer();
	public static IEnumerable<PositionedTile> TileRect(IEnumerable<IEnumerable<Int2>> tiles, int width)
	{
		var openList = new System.Collections.Generic.Queue<ImmutableStripEnd>();
		openList.Enqueue(new ImmutableStripEnd(width));

		var graph = new StripTilings();

		int count = 0;
		
		while (openList.Any())
		{
			var stripEnd = openList.Dequeue();

			if (graph.ContainsForward(stripEnd))
			{
				//This can happen because when we add nodes to the open list, we don't check the open list itself. 
				continue;
			}
			
			var empty = stripEnd.FindEmpty();

			foreach (var tile in tiles)
			{
				if (stripEnd.IsEmpty(empty, tile))
				{
					if (count % 100 == 0)
					{
						Console.WriteLine(count + " " + openList.Count);
					}
					count++;
					
					var newStripEnd = stripEnd.Place(empty, tile);

					//if this pair is not already in the graph
					graph.Add(stripEnd, newStripEnd, empty, tile);

					if (!graph.ContainsForward(newStripEnd))
					{
						openList.Enqueue(newStripEnd);
					}
					
					if (newStripEnd.IsStraight)
					{
						Console.WriteLine("--------------------------");
						return graph.GetTilingFromBack(newStripEnd);
					}
				}
			}
		}

		//Tiling does not exist
		return null;
	}
	
	public static IEnumerable<Int2> NameToPoly(string cellsStr)
	{
		string[] rows = cellsStr.Split("/");
		rows = rows.Reverse().ToArray();

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
		var min = points.MinBy(x=>x, pointComparer);

		return points.Select(x => x - min);
	}
	
	public static (int order, Int2 size, IEnumerable<PositionedTile> tiling) Summarize(this IEnumerable<PositionedTile> tiling)
	{
		int order = tiling.Count();
		int maxX = tiling.SelectMany(x => x.Points).Max(cell => cell.X);
		int maxY =  tiling.SelectMany(x =>  x.Points).Max(cell => cell.Y);

		Int2 size = new(maxX + 1, maxY + 1);

		return (order, size, tiling);
	}
}

public class Graph<T>
{
	private readonly IDictionary<ImmutableStripEnd, IList<T>> graphEdges = new Dictionary<ImmutableStripEnd, IList<T>>(ImmutableStripEnd.GetComparer());

	public IList<T> this[ImmutableStripEnd stripEnd] => graphEdges[stripEnd];
	
	public void Add(ImmutableStripEnd vertex1, T vertex2)
	{
		if (!graphEdges.ContainsKey(vertex1))
		{
			graphEdges[vertex1] = new List<T>();
		}

		graphEdges[vertex1].Add(vertex2);
	}

	public bool Contains(ImmutableStripEnd vertex) => graphEdges.ContainsKey(vertex);

	public int Count() => graphEdges.Count;
}


/// <summary>
/// A set of these can represent a tiling (to be proper there should be no overlap).
/// </summary>
public struct PositionedTile
{
	public IEnumerable<Int2> Tile;
	public Int2 Position;

	public IEnumerable<Int2> Points
	{
		get
		{
			var position = Position;
			return Tile.Select(point => point + position);
		}
	}

	public override string ToString() => Points.ToPrettyString();
}

/// <summary>
/// A graph that represents strip tilings.
/// </summary>
public class StripTilings
{
	private struct Node
	{
		public Int2 Position;
		public IEnumerable<Int2> Tile;
		public ImmutableStripEnd StripEnd;
	}
	
	private readonly Graph<ImmutableStripEnd> forward = new();
	private readonly Graph<Node> backward = new();

	public void Add(ImmutableStripEnd vertex1, ImmutableStripEnd vertex2, Int2 position, IEnumerable<Int2> tile)
	{
		var node = new Node
		{
			StripEnd = vertex1,
			Position = position,
			Tile = tile
		};
			
		forward.Add(vertex1, vertex2);
		backward.Add(vertex2, node);
		
		//
		//Console.WriteLine(vertex1 + " ---> " + vertex2);
	} 

	public bool ContainsForward(ImmutableStripEnd vertex) => forward.Contains(vertex);

	/// <summary>
	/// Gets a tiling in this set that ends with the given strip end. 
	/// </summary>
	/// <param name="stripEnd"></param>
	/// <returns></returns>
	public IEnumerable<PositionedTile> GetTilingFromBack(ImmutableStripEnd stripEnd)
	{
		Console.WriteLine(backward.Count());
		var list = new List<PositionedTile>();
		int i = 0;
		while (backward.Contains(stripEnd) && i < 10000)
		{
			//Console.WriteLine(stripEnd);
			
			var node = backward[stripEnd].First(); //we assume for now there is only one path
			list.Add(new PositionedTile
			{
				Position = node.Position,
				Tile = node.Tile
			});

			stripEnd = node.StripEnd;

			if(stripEnd.IsStraight) break;
			
			i++;
		}

		return list;
	}
}
