using System;

namespace DataStructures
{
	public static class Inker
	{
		public static float Average(ColorF color)
			=> (color.R + color.G + color.B) / 3;

		public static ColorF ToColor(float intensity)
			=> ColorF.FromArgb(1, intensity, intensity, intensity);

		public static float AverageRow(IGrid<float> grid, Int2 start, int length)
		{
			float sum = 0;

			foreach (var index in Grid.Row(start, length))
			{
				sum += grid[index];
			}

			return sum / length;
		}

		public static float MedianRow(IGrid<float> grid, Int2 start, int length)
		{
			float[] values = new float[length];

			int i = 0;
			
			foreach (var index in Grid.Row(start, length))
			{
				values[i] = grid[index];
				i++;
			}
			
			Array.Sort(values);
			float median = values[length / 2];
			
			return median;
		}
		
		public static IGrid<ColorF> Ink(IGrid<ColorF> image, int width)
			=> image
				.Desaturate(Average)
				.Collect(AverageRow, width)
				.Render(width)
				.Colorize(ToColor);

		public static IGrid<float> Desaturate(this IGrid<ColorF> image, Func<ColorF, float> desaturate)
			=> image.Apply(desaturate);
		
		public static IGrid<ColorF> Colorize(this IGrid<float> image, Func<float, ColorF> colorize)
			=> image.Apply(colorize);
		
		public static IGrid<float> Collect(this IGrid<float> image, Func<IGrid<float>, Int2, int, float> collect, int lineWidth)
		{
			var newGrid = new Grid<float>(new Int2(image.Width / lineWidth, image.Height));

			foreach (var index in newGrid.Indices)
			{
				var sourceIndex = new Int2(index.X * lineWidth, index.Y);
				newGrid[index] = collect(image, sourceIndex, lineWidth);
			}

			return newGrid;
		}

		public static IGrid<float> Render(this IGrid<float> intensity, int lineWidth)
		{
			var lineCounts = intensity.CloneStructure<int>();
			var excesses = intensity.CloneStructure();
			
			foreach (var index in intensity.Indices)
			{
				float scaledValue = intensity[index] * lineWidth;
				lineCounts[index] = scaledValue < 1 ? 0 : MyMath.FloorToInt((scaledValue + 1) / 2) * 2 - 1;
				excesses[index] = scaledValue - lineCounts[index];
			}

			var newGrid = new Grid<float>(new Int2(intensity.Width * lineWidth, intensity.Height));

			foreach (var index in intensity.Indices)
			{
				var newGridIndex = new Int2(index.X * lineWidth, index.Y);
				var center = newGridIndex + Int2.Right * lineWidth / 2;
				int lineCount = lineCounts[index];
				
				if (lineCount == 0)
				{
					newGrid[center] = excesses[index];
				}
				else if (lineCount == lineWidth)
				{
					foreach (var rowIndex in Grid.Row(newGridIndex, lineWidth))
					{
						newGrid[rowIndex] = 1;
					}
				}
				else
				{
					var solidStartIndex = center - Int2.Right*lineCount/2;

					foreach (var rowIndex in Grid.Row(solidStartIndex, lineCount))
					{
						newGrid[rowIndex] = 1;
					}

					float halfExcess = excesses[index] / 2;
					newGrid[solidStartIndex - 1 * Int2.Right] = halfExcess;
					newGrid[solidStartIndex + lineCount * Int2.Right] = halfExcess;
				}
			}

			return newGrid;
		}
	}
}