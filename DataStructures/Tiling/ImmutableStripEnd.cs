using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DataStructures.Tiling;

public sealed class ImmutableStripEnd : IStripEnd<ListTile, object>
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
				GLDebug.Assert(other.data != null, "other.data != null");
				
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
	
	public static IStripEnd<ListTile, object> New(int width) => new ImmutableStripEnd(width);

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
	
	private ImmutableStripEnd(ImmutableStripEnd stripEnd, Int2 position, ListTile tile)
	{
		
		void InitializeData()
		{
			int maxY = tile.Cells.Max(c => c.Y) + position.Y;
			length = Math.Max(stripEnd.length, maxY + 1);
			width = stripEnd.width;
			
			data = new bool[width, length];

			foreach (var cell in stripEnd.Cells)
			{
				data[cell.X, cell.Y] = stripEnd[cell];
			}

			foreach (var cell in tile.Cells)
			{
				data[cell.X + position.X, cell.Y + position.Y] = true;
			}
		}
		
		void ReduceFullRows()
		{
			var empty = FindEmpty(null);

			if (empty.Y != 0)
			{
				offset = empty.Y;
				length -= offset;
			}
		}
		
		InitializeData();
		ReduceFullRows();

		var empty = FindEmpty(null);
		GLDebug.Assert(empty.Y == 0);

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

	public IStripEnd Place(Object _, Int2 position, ListTile cells) => new ImmutableStripEnd(this, position, cells);

	public Int2 FindEmpty(object _)
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

	public bool CanPlace(object _, Int2 position, ListTile tile)
	{
		bool InRangeAndEmpty(Int2 point) => point.X >= 0 && point.X < width && !this[point];
		
		return tile.Cells.All(cell => InRangeAndEmpty(position + cell));
	}

	public static object GetComparer() => new Comparer();

	public ImmutableStripEnd ReflectAlongVertical() => new ImmutableStripEnd(this, true);
}
