using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.Colorings;

public class DynamicGrid
{
	//Prefer odd so the center is in the middle, prefer one more than power of two (compare with CalculateNewSize below)
	protected static readonly Int2 DefaultSize = 17 * Int2.One; 
	
	protected DynamicGrid(){}
}

/// <summary>
/// A grid that grows dynamically to accomodate any index used for a set operation. 
/// </summary>
/// <typeparam name="T">The type contained in the grid.</typeparam>
public sealed class DynamicGrid<T> : DynamicGrid, IGrid<T>
{
	private Grid<T> grid;
	private Int2 anchor;

	public IEnumerable<Int2> Indices => grid.Indices.Select(index => index + anchor);
	public int Width => grid.Width;
	public int Height => grid.Height;
	public Int2 Size => grid.Size;
	private Int2 Center => anchor + Size / 2;

	public T this[Int2 index]
	{
		//We do not resize on get
		get => grid[index - anchor];
		
		set
		{
			if (!ContainsIndex(index))
			{
				/* Modifying what these methods return changes the resize strategy. */
				var size = CalculateNewSize(index);
				Resize(size);
			}
			grid[index - anchor] = value;
		}
	}

	public T this[int x, int y]
	{
		get => grid[new Int2(x, y)];
		set => grid[new Int2(x, y)] = value;
	}

	public DynamicGrid() : this(DefaultSize) { }

	public DynamicGrid(Int2 initialSize) : this(initialSize, Int2.Zero) { }

	public DynamicGrid(Int2 initialSize, Int2 center) => CreateGrid(initialSize, center);
	
	public bool ContainsIndex(Int2 index) => grid.ContainsIndex(index - anchor);

	private void CreateGrid(Int2 size, Int2 center)
	{
		anchor = center - size/2;
		grid = new Grid<T>(size);
	}
	
	private void Resize(Int2 size)
	{
		var center = anchor + size / 2;
		var oldGrid = grid;
		var oldAnchor = anchor;
		
		CreateGrid(size, center);

		foreach (var oldIndex in oldGrid.Indices)
		{
			var newIndex = oldAnchor + oldIndex;
			this[newIndex] = oldGrid[oldIndex];
		}
	}

	private Int2 CalculateNewSize(Int2 index)
	{
		var normalizedIndex = index - Center;
		int maxIndex = Math.Max(Math.Abs(normalizedIndex.X), Math.Abs(normalizedIndex.Y));
		int power = 2 + (int) Math.Floor(Math.Log2(maxIndex));
		
		/*
			This below is one more than next-smallest power of 2 greater than maxIndex.
			
			For example, if the index is 5, the next smallest power of two is 8, and
			the next-smallest is 16.
			
			Assuming the center is at 0, the new grid will then be size 17, which will 
			accomodate indices -8 and 8 (inclusively).
		*/
		int newSize = 1 << power + 1; 

		return newSize * Int2.One;
	}
}
