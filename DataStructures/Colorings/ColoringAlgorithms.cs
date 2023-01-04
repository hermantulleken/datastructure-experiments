using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures.Colorings;

public class ColoringAlgorithms
{
	public struct FlagColoring
	{
		public int ColorCount;
		public int NextRowOffset;
		
		public int GetColor(Int2 point) => GLMath.Mod(point.X + point.Y * NextRowOffset, ColorCount);
		public override string ToString() => $"F_{ColorCount}_{NextRowOffset}";
	}
	
	/*
	public void FindColoring(List<Int2> polyomino)
	{
		
		var colors = new DynamicGrid<int?>();
		var constraints =  new DynamicGrid<List<int>>();
		
		constraints.Fill(_ => new List<int>());
		var constrainMask = GetContraintMask(polyomino);


		while (true)
		{
			Int2 empty = FindEmpty();
			int color = FindColor(empty);

		}

	}

	private int FindColor(IGrid<Int2> constraints, Int2 empty)
	{
		var cellConstraints = constraints[empty];
	}
	*/

	public static IEnumerable<Int2> FindOffsetsOfFlagColoring(Int2 size, FlagColoring coloring)
	{
		var colors = new Grid<int>(size, 0);
		var offsets = new List<Int2>();
		
		foreach (var index in colors.Indices)
		{
			colors[index] = coloring.GetColor(index);
		}

		foreach (var index1 in colors.Indices)
		{
			foreach (var index2 in colors.Indices)
			{
				if (index1 == index2) continue;
				if(colors[index1] != 0) continue;
				if(colors[index2] != 0) continue;

				var offset = NormalizeOffset(index2 - index1);
				
				if (!offsets.Any(o => AreOffsetsEquivalent(o, offset)))
				{
					offsets.Add(offset);
				}
			}
		}

		return offsets;
	}

	private bool IsMultipleOf(int x1, int x2) =>
		(x2 == 0 && x1 == 0) || GLMath.Mod(x1, x2) == 0;

	private static bool IsMultipleOf(Int2 point1, Int2 point2)
	{
		throw new NotImplementedException();
	}

	private static bool IsMultipleOfNonZero(Int2 point1, Int2 point2)
	{
		GLDebug.Assert(point2.X != 0);
		GLDebug.Assert(point2.Y != 0);
		
		return GLMath.Mod(point1.X, point2.X) == 0
		       && point1.Y == point2.Y * point1.X / point2.X;
	}

	private static bool AreOffsetsEquivalent(Int2 point1, Int2 point2) 
		=> IsMultipleOf(point1, point2) || IsMultipleOf(point2, point1);
		
	private static Int2 NormalizeOffset(Int2 point) => 
		Math.Abs(point.X) <= Math.Abs(point.Y) 
			? new Int2(Math.Abs(point.X), Math.Abs(point.Y))
			: new Int2(Math.Abs(point.Y), Math.Abs(point.X));	
}
