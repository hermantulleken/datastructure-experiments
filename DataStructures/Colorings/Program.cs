using System;
using Gamelogic.Extensions;

namespace DataStructures.Colorings;

public static class Program
{
	public static void Main_()
	{
		var coloring = new ColoringAlgorithms.FlagColoring
		{
			ColorCount = 2, 
			NextRowOffset = 1
		};
		
		var offsets = ColoringAlgorithms.FindOffsetsOfFlagColoring(10 * Int2.One, coloring);
		Console.WriteLine($"{coloring}: {offsets.ToPrettyString()}");
	}
}
