using System;
using System.Drawing;
using System.IO;
using System.Runtime.Versioning;

namespace DataStructures.TileImage;

[SupportedOSPlatform("windows")]
public static class Program
{
	private const string WorkingDirectory = "C:/Work/Files/";
	private const string ImageFile = "Y5 Prime 12x50.PNG"; 
	
	private static readonly string Basename = Path.GetFileNameWithoutExtension(ImageFile);
	private const string ImageFilePath = WorkingDirectory + ImageFile;
	private static readonly string SvgFile = Basename + ".svg";
	private static readonly string SvgFilePath = WorkingDirectory + SvgFile;

	public static void Main1(string[] args) => ConvertTilingImageToSvg();

	private static void ConvertTilingImageToSvg() =>ConvertTilingImageToSvg(ImageFilePath, SvgFilePath);

	private static void ConvertTilingImageToSvg(string imagePath, string svgPath)
	{
		try
		{
			if (!PathsExist(imagePath)) return;
			
			var scanSettings = new PolyominoScanner.Settings
			{
				BordersOverlap = true, 
				LinesArePure = true
			};
			
			var image = new Bitmap(imagePath);
			var grid = (Grid<Color>) image.ToGrid().Apply(color => color.ToColor());
			var (gridSize, tiles) = PolyominoScanner.GetTiles(grid, scanSettings);
			var settings = Svg.DefaultSettings with { FillColor = Svg.Color.Blue, LineWidth = 1.323f/2, Scale = 10};
			string svg = Svg.ToSvg(gridSize, tiles, settings);
			
			Console.WriteLine("SVG string created.");
			
			File.WriteAllText(svgPath, svg);
		}
		catch (ArgumentException e)
		{
			Console.WriteLine(e);
		}
		//catch (Exception e)
		//{
		//	Console.WriteLine(e);
		//}
	}

	private static bool PathsExist(string imagePath)
	{
		if (!File.Exists(imagePath))
		{
			Console.WriteLine(imagePath + " does not exist.");
			return false;
		}

		return true;
	}
}
