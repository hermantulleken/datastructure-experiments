namespace DataStructures.TileImage;

using System.Collections.Generic;
using System.Numerics;
using System.Linq;

/// <summary>
/// Provides functions for converting tilings into images. 
/// </summary>
public static class Svg 
{
	/// <summary>
	/// HTML colors. 
	/// </summary>
	public static class Color
	{
		public const string Red = "f62d7f";
		public const string Orange = "ff6f00";
		public const string Yellow = "ffff26";
		public const string Green = "40cc0c";
		public const string BlueGreen = "48e0af";
		public const string Blue = "#15ccff";
		public const string Black = "#000000";
		public const string White = "#ffffff";
	}
	
	/// <summary>
	/// Controls aspects of an SVG drawing. 
	/// </summary>
	public record Settings
	{
		public float LineWidth;
		public float Scale;
		public string LineColor;
		public string FillColor;
	}

	/// <summary>
	/// A good set of default settings.
	/// </summary>
	public static readonly Settings DefaultSettings = new Settings()
	{
		LineWidth = 1.323f,
		Scale = 20,
		LineColor = Color.Black,
		FillColor = Color.White
	};

	/// <summary>
	/// Converts a tiling to an SVG string. 
	/// </summary>
	/// <param name="gridSize">The size of the grid that contains the tiling.</param>
	/// <param name="tiles">A list of tiles, each represented as a list of points that are covered by the tile.</param>
	/// <param name="settings">The settings toi use for drawing the image. See for example <see cref="DefaultSettings"/>.</param>
	/// <returns></returns>
	// TODO: calculate the grid size instead.
	public static string ToSvg(Int2 gridSize, IEnumerable<IEnumerable<Int2>> tiles, Settings settings) 
	{
		float lineThickness = settings.LineWidth;
		float scale = settings.Scale;
		
		string color = settings.FillColor;
		string lineColor = settings.LineColor;
		
		var offset = new Vector2(lineThickness / 2, lineThickness / 2);
		float width = gridSize.X * scale + lineThickness;
		float height = gridSize.Y * scale + lineThickness;
		string content = "";
		
		foreach (var tile in tiles) 
		{
			var poly = string.Empty;
			var paths = GetPaths(tile.ToList());

			foreach (var path in paths) 
			{
				var scaledPath = path.Select(point => point * scale + offset).ToList();
				string pathString = PathToSvgPathStr(scaledPath);
				poly += $"<path d=\"{pathString}\" fill=\"{color}\" stroke-width=\"{lineThickness}\" stroke=\"{lineColor}\" />";
			}

			content += poly;
		}
		return $"<svg height=\"{height}\" width=\"{width}\" xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\">{content}</svg>";
	}

	private static (Int2 anchor, Int2 size) FindBoundingBox(IEnumerable<Int2> cells) 
	{
		int maxX = cells.Max(cell => cell.X);
		int maxY = cells.Max(cell => cell.Y);
		int yOffset = cells.Min(cell => cell.Y);
		int xOffset = cells.Min(cell => cell.X);

		int length = maxY + 1 - yOffset;
		int width = maxX + 1 -xOffset;

		return (new Int2(xOffset, yOffset), new Int2(width, length));
	}
  
	private static string PathToSvgPathStr(IEnumerable<Vector2> path)
	{
		var start = path.First();
		string pathStr = $"M {start.X} {start.Y} ";
		
		foreach (var point in path.Skip(1))
		{
			pathStr += $"L {point.X} {point.Y} ";
		}
		pathStr += "Z";
		
		return pathStr;
	}
	
	private static List<List<Int2>> GetPaths(List<Int2> tile)
	{
		var edges = PolyominoToEdgeSoup(tile);
		return EdgeSoupToPaths(edges);
	}
	
	private static IEnumerable<(Int2, Int2)> PolyominoToEdgeSoup(List<Int2> polyomino) 
	{
		var edges = new List<(Int2, Int2)>();
		
		foreach (var point in polyomino) 
		{
			if (!polyomino.Contains(point + Int2.Up)) 
			{
				edges.Add((point + Int2.Up, point + Int2.Up + Int2.Right));
			}
			if (!polyomino.Contains(point + Int2.Left)) 
			{
				edges.Add((point, point + Int2.Up));
			}
			if (!polyomino.Contains(point + Int2.Down)) 
			{
				edges.Add((point, point + Int2.Right));
			}
			if (!polyomino.Contains(point + Int2.Right)) 
			{
				edges.Add((point + Int2.Right, point + Int2.Up + Int2.Right));
			}
		}
		
		return edges;
	}
        
	private static List<List<Int2>> EdgeSoupToPaths(IEnumerable<(Int2, Int2)> edges)
	{
		var edgeList = edges.ToList();

		(List<Int2> path, List<(Int2, Int2)> pathEdges) GetPath((Int2, Int2) edge)
		{
			var start = edge.Item1;
			var end = edge.Item2;
			var path = new List<Int2>{start, end};
			var pathEdges = new List<(Int2, Int2)>();
			
			while (start != end) 
			{
				foreach (var nextEdge in edgeList)
				{
					void AddToPath(Int2 node)
					{
						end = node;
						path.Add(end);
						pathEdges.Add(nextEdge);
					}
					
					if (nextEdge.Item1 == end) 
					{
						AddToPath(nextEdge.Item1);
					} 
					else if (nextEdge.Item2 == end) 
					{
						AddToPath(nextEdge.Item2);
					}
				}
			}

			return (path, pathEdges);
		}

		var paths = new List<List<Int2>>();
		while (edgeList.Count > 0) 
		{
			var edge = edgeList.First();
			edgeList.Remove(edge);
			var (path, pathEdges) = GetPath(edge);

			foreach (var pathEdge in pathEdges)
			{
				edgeList.Remove(pathEdge);
			}
			
			paths.Add(SimplifyPath(path));
		}
		
		return paths;
	}

	private static List<Int2> SimplifyPath(IReadOnlyList<Int2> path)
	{
		var corners = new List<Int2>();
		
		for (int currentIndex = 0; currentIndex < path.Count; currentIndex++)
		{
			int previousIndex = currentIndex == 0 ? path.Count - 1 : currentIndex - 1;
			int nextIndex = currentIndex == path.Count - 1 ? 0 : currentIndex + 1;

			var current = path[currentIndex];
			var previous = path[previousIndex];
			var next = path[nextIndex];

			if (next - current != current - previous)
			{
				corners.Add(current);
			}
		}

		return corners;
	}
}
