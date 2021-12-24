﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using Gamelogic.Extensions;
using JetBrains.Annotations;

namespace DataStructures
{
	public struct Int2
	{
		public int X;
		public int Y;

		public static readonly Int2 Zero = new(0, 0);
		public static readonly Int2 One = new(1, 1);
		public static readonly Int2 Up = new(0, 1);
		public static readonly Int2 Right = new(1, 0);
		
		public Int2(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static Int2 operator+(Int2 point1, Int2 point2) => new(point1.X + point2.X, point1.Y + point2.Y);
		public static Int2 operator-(Int2 point1, Int2 point2) => new(point1.X - point2.X, point1.Y - point2.Y);
		public static Int2 operator-(Int2 point1) => new(-point1.X, -point1.Y);
		public static Int2 operator*(int factor, Int2 point) => new(factor * point.X, factor * point.Y);
		public static Int2 operator*(Int2 point, int factor) => new(factor * point.X, factor * point.Y);
		public static Int2 operator/(Int2 point, int divisor) => new(point.X / divisor, point.Y / divisor);
		
		public static implicit operator Vector2(Int2 point) => new(point.X, point.Y);

		public override string ToString() => $"[{X}, {Y}]";
	}

	public readonly struct ColorF
	{

		public static readonly ColorF Red = FromArgb(1, 1, 0, 0); 
		public static readonly ColorF Green = FromArgb(1, 0, 1, 0);
		public static readonly ColorF Blue = FromArgb(1, 0, 0, 1);
		
		public readonly float A;
		public readonly float R;
		public readonly float G;
		public readonly float B;

		private ColorF(float a, float r, float g, float b)
		{
			A = a;
			R = r;
			G = g;
			B = b;
		}
		
		public static ColorF FromArgb(float a, float r, float g, float b) => new ColorF(a, r, g, b);

		public Color ToColor() =>
			Color.FromArgb(
				ToByte(A),
				ToByte(R),
				ToByte(G),
				ToByte(B));
		
		public override string ToString() => $"[{R}, {G}, {B}, ({A})]";
		
		private static int ToByte(float value) => MyMath.RoundToInt(MyMath.Clamp(255 * value, 0, 255));
	}
	
	public static class MyMath
	{
		public static int FloorToInt(float x) => (int)MathF.Floor(x);
		public static int CeilingToInt(float x) => (int)MathF.Ceiling(x);
		public static int RoundToInt(float x) => (int)MathF.Round(x);
		
		public static Int2 FloorToInt2(this Vector2 vector) => new Int2(FloorToInt(vector.X), FloorToInt(vector.Y));
		public static Int2 CeilingToInt2(this Vector2 vector) => new Int2(CeilingToInt(vector.X), CeilingToInt(vector.Y));
		public static float Lerp(float a, float b, float t) => (1 - t) * a + t * b;

		public static float Clamp(float value, float bottom, float top) =>
			value < bottom ? bottom :
			value > top ? top :
			value;
		
		public static int Clamp(int value, int bottom, int top) =>
			value < bottom ? bottom :
			value > top ? top :
			value;

		public static Int2 Clamp(Int2 value, Int2 anchor, Int2 abyss) =>
			new Int2(
				Clamp(value.X, anchor.X, abyss.X - 1), 
				Clamp(value.Y, anchor.Y, abyss.Y - 1));

		public static ColorF Lerp(ColorF a, ColorF b, float t) 
			=> ColorF.FromArgb(
				Lerp(a.A, b.A, t),
				Lerp(a.R, b.R, t),
				Lerp(a.G, b.G, t),
				Lerp(a.B, b.B, t));

		public static float Mod(float x, float divisor)
			=> (x >= 0) ? x % divisor : x % divisor + divisor;
		
		public static float FloorDiv(float x, float divisor)
		{
			float remainder = Mod(x, divisor);
		
			return (x - remainder) / divisor;
		}

	}
	
	public class IdealImage
	{
		private static readonly Func<Vector2, Vector2> Identity = x => x;  
		private Func<Vector2, Vector2> sampler;

		private Vector2 anchor;
		private Vector2 abyss;
		private IPixelGrid pixels;
		public IdealImage(IPixelGrid pixels)
		{
			this.pixels = pixels;
			sampler = Identity;
			anchor = pixels.Anchor;
			abyss = pixels.Abyss;
		}

		public IPixelGrid Rasterize()
		{
			var rasterAnchor = anchor.FloorToInt2();
			var rasterAbyss = abyss.CeilingToInt2();
			var rasterSize = rasterAbyss - rasterAnchor;

			return new CenteredGrid(rasterSize, Sample);
		}

		public ColorF Sample(Vector2 point)
		{
			var sourcePoint = sampler(point);
			var p00 = MyMath.Clamp(sourcePoint.FloorToInt2(), pixels.Anchor, pixels.Abyss);
			var p10 = MyMath.Clamp(p00 + Int2.Right, pixels.Anchor, pixels.Abyss);
			var p01 = MyMath.Clamp(p00 + Int2.Up, pixels.Anchor, pixels.Abyss);
			var p11 = MyMath.Clamp(p10 + Int2.Up, pixels.Anchor, pixels.Abyss);
				
			var c00 = pixels[p00];
			var c10 = pixels[p10];
			var c01 = pixels[p01];
			var c11 = pixels[p11];

			//Bilinear interpolation
			var t = sourcePoint - p00;
			var cx0 = MyMath.Lerp(c00, c10, t.X);
			var cx1 = MyMath.Lerp(c01, c11, t.X);

			return MyMath.Lerp(cx0, cx1, t.Y);
		}
	}

	public static class ArrayExtensions
	{
		public static int GetWidth<T>(this T[,] array) => array.GetLength(0);
		public static int GetHeight<T>(this T[,] array) => array.GetLength(1);
		
		public static void Fill<T>(this T[,] array, T element)
		{
			int width = array.GetWidth();
			int height = array.GetHeight();
				
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					array[i, j] = element;
				}
			}
		}
		
		public static void Fill<T>(this T[,] array, Func<Vector2, T> sampler)
		{
			int width = array.GetWidth();
			int height = array.GetHeight();
				
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					array[i, j] = sampler(new Vector2(i, j));
				}
			}
		}
	}
	
	public interface IPixelGrid : IGrid<ColorF>
	{
		Int2 Anchor { get; }
		Int2 Abyss { get; }
		Bitmap ToBitmap();
	}

	public class PixelGrid : IPixelGrid
	{
		private readonly ColorF[,] pixels;

		public IEnumerable<Int2> Indices { get; }
		public IGrid<T2> CloneStructure<T2>()
		{
			throw new NotImplementedException();
		}

		public int Width => Size.X;
		public int Height => Size.Y;
		public Int2 Size { get; private set; }
		public Int2 Anchor => Int2.Zero;
		public Int2 Abyss => Size;

		public ColorF this[Int2 point]
		{
			get => pixels[point.X, point.Y];
			set => pixels[point.X, point.Y] = value;
		}

		public PixelGrid(Int2 size, ColorF defaultColor)
		{
			Size = size;
			pixels = new ColorF[Width, Height];
			pixels.Fill(defaultColor);
		}

		internal PixelGrid(Int2 size, Func<Vector2, ColorF> sampler)
		{
			Size = size;
			pixels = new ColorF[Width, Height];
			pixels.Fill(sampler);
		}

		public Bitmap ToBitmap()
		{
			var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
			
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					bitmap.SetPixel(i, j, pixels[i,j].ToColor());
				}
			}
			
			return bitmap;
		}

		public override string ToString()
		{
			var result = new ColorF[Width * Height];

			for (int j = 0; j < Height; j++)
			{
				for (int i = 0; i < Width; i++)
				{
					result[j * Width + i] = pixels[i, j];
				}
			}
			
			return result.ToPrettyString();
		}
	}

	public class CenteredGrid : IPixelGrid
	{
		private readonly IPixelGrid baseGrid;
		private readonly Int2 offset;

		public IEnumerable<Int2> Indices { get; }
		
		public int Width => baseGrid.Width;
		public int Height => baseGrid.Height;
		public Int2 Size => baseGrid.Size;

		public Int2 Anchor => -offset;
		public Int2 Abyss => offset;

		public ColorF this[Int2 point]
		{
			get => baseGrid[point + offset];
			set => baseGrid[point + offset] = value;
		}
		
		public CenteredGrid(Int2 size, ColorF defaultColor)
		{
			ValidateSize(size);
			offset = size / 2;
			baseGrid = new PixelGrid(size, defaultColor);
			
		}
		
		public CenteredGrid(Int2 size, Func<Vector2, ColorF> sampler)
		{
			ValidateSize(size);
			offset = size / 2;
			baseGrid = new PixelGrid(size, v => sampler(v - offset));
			
		}

		public CenteredGrid(IPixelGrid baseGrid)
		{
			this.baseGrid = baseGrid;
			offset = baseGrid.Size / 2;
		}

		public void Paint(Func<Int2, ColorF> painter)
		{
			int x0 = Anchor.X;
			int x1 = Abyss.X;
			int y0 = Anchor.Y;
			int y1 = Abyss.Y;
			
			for (int i = x0; i < x1; i++)
			{
				for (int j = y0; j < y1; j++)
				{
					var point = new Int2(i, j);
					this[point] = painter(point);
				}
			}
		}

		public Bitmap ToBitmap() => baseGrid.ToBitmap();
		
		[AssertionMethod]
		private static void ValidateSize(Int2 size)
		{
			if (size.X % 2 != 0 || size.Y % 2 != 0)
			{
				throw new ArgumentException("Dimensions must be even", nameof(size));
			}
		}

		public override string ToString() => baseGrid.ToString();
	}

	public static class GridExtensions
	{
		public static void Paint(this CenteredGrid grid, Func<Int2, int> classifier, IList<ColorF> colors)
			=> grid.Paint(p => colors[classifier(p)]);

		public static IGrid<T1> CloneStructure<T, T1>(this IGrid<T> grid, T1 initialElement = default)
			=> new Grid<T1>(grid.Size, initialElement);
		
		public static void Fill<T>(this IGrid<T> grid, T initialElement = default)
		{
			foreach (var index in grid.Indices)
			{
				grid[index] = initialElement;
			}
		}
		
		public static void Fill<T>(this IGrid<T> grid, Func<Int2, T> filler)
		{
			foreach (var index in grid.Indices)
			{
				grid[index] = filler(index);
			}
		}

		public static IGrid<T1> CloneStructure<T, T1>(this IGrid<T> grid, Func<Int2, T1> filler)
			=> new Grid<T1>(grid.Size, filler);

		public static IGrid<T1> Apply<T, T1>(IGrid<T> grid, Func<T, T1> apply)
		{
			var newGrid = grid.CloneStructure<T, T1>();
			foreach (var index in grid.Indices)
			{
				newGrid[index] = apply(grid[index]);
			}

			return newGrid;
		}
		
		public static IGrid<T1> Apply<T, T1>(IGrid<T> grid, Func<T, Int2, T1> apply)
		{
			var newGrid = grid.CloneStructure<T, T1>();
			foreach (var index in grid.Indices)
			{
				newGrid[index] = apply(grid[index], index);
			}

			return newGrid;
		}
	}

	public class VeinClassifier
	{
		private float mainVeinThickness = 4;
		private float secondaryVeinInterval = 20;
		private float secondaryVeinThickness = 2;
		public bool IsVein(Vector2 point)
		{
			bool IsMainVein(Vector2 p) => MathF.Abs(p.X) <=  mainVeinThickness / 2;
			bool IsSecondaryVein(Vector2 p) => MyMath.Mod(p.Y, secondaryVeinInterval) <= secondaryVeinThickness;
			
			if(IsMainVein(point)) return true;

			if(IsSecondaryVein(point)) return true;

			return false;

		}
	}

	public interface IGrid<T>
	{
		public T this[Int2 index] { get; set; }
		public IEnumerable<Int2> Indices { get; }
		
		int Width { get; }
		int Height { get; }
		Int2 Size { get; }
	}

	public class Grid
	{
		public static IEnumerable<Int2> Rect(Int2 anchor, Int2 size)
		{
			int x0 = anchor.X;
			int y0 = anchor.Y;
			int x1 = anchor.X + size.X;
			int y1 = anchor.Y + size.Y;
			
			for (int j = y0; j < y1; j++)
			{
				for (int i = x0; i < x1; i++)
				{
					yield return new Int2(i, j);
				}
			}
		}

		public static IEnumerable<Int2> Row(int x0, int length)
			=> Rect(new Int2(x0, 0), new Int2(length, 0));
	}

	public class Grid<T> : IGrid<T>
	{
		private readonly T[,] data;

		public T this[Int2 index]
		{
			get => data[index.X, index.Y];
			set => data[index.X, index.Y] = value;
		}

		public IEnumerable<Int2> Indices
		{
			get
			{
				for (int j = 0; j < Height; j++)
				{
					for (int i = 0; i < Width; i++)
					{
						yield return new Int2(i, j);
					}
				}
			}
		}

		public int Width => Size.X;
		public int Height => Size.Y;
		public Int2 Size { get; }

		public Grid(Int2 size, T initialElement = default)
		{
			Size = size;
			data = new T[Width, Height];

			foreach (var index in this.Indices)
			{
				this[index] = initialElement;
			}
		}
		
		public Grid(Int2 size, Func<Int2, T> filler)
		{
			Size = size;
			data = new T[Width, Height];

			foreach (var index in this.Indices)
			{
				this[index] = filler(index);
			}
		}
	}
}