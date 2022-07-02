using System;
using System.Drawing;
using System.IO;
using System.Runtime.Versioning;

namespace DataStructures.TileImage;

public class Program
{
	private const string WorkingDirectory = "C:/Work/Files/";
	private const string ImageFile = "11omino5_25x77.gif";// "10omino07_4x5.gif"; //"small.png";
	private const string ImageFilePath = WorkingDirectory + ImageFile;
	
	
	[SupportedOSPlatform("windows")]
	public static void Main1(string[] args)
	{
		try
		{
			if (!PathsExist()) return;

			var image = new Bitmap(ImageFilePath);
			var grid = (Grid<Color>) image.ToGrid().Apply(color => color.ToColor());

			PolyominoScanner.GetTiles(grid);
		}
		catch (ArgumentException e)
		{
			Console.WriteLine(e);
			Console.WriteLine(e.InnerException);
		}
		//catch (Exception e)
		//{
		//	Console.WriteLine(e);
		//}
	}

	private static bool PathsExist()
	{
		if (!Directory.Exists(WorkingDirectory))
		{
			Console.WriteLine(WorkingDirectory + " does not exist.");
			return false;
		}

		if (!File.Exists(ImageFilePath))
		{
			Console.WriteLine(ImageFile + " does not exist.");
			return false;
		}

		return true;
	}
}
