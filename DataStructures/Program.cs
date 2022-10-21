#define USE_PAGED

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Gamelogic.Extensions;
using Gamelogic.Extensions.Algorithms;
using DataStructures.Tiling;

namespace DataStructures
{

	class TTetromino<TWidth> : ILongTile<TWidth, TTetromino<TWidth>>
		where TWidth : IWidth
	{
		public static bool CanRuleOut(Tiler<TWidth, TTetromino<TWidth>>.StripEnd stripEnd)
		{
			return false;
		}
	}
	
	[SuppressMessage("ReSharper", "IdentifierTypo")]
	public static class Program
	{
		public static readonly IDictionary<string, string[]> PagedTileHeuristicFunctions =
			new Dictionary<string, string[]>()
			{
				{
					".***/*******", 
					new[]
					{
						".*/*.*/.*", 
						"******/*....*", 
						"*******/*.....*", 
						"********/*......*"
					}
				},

				{
					".*/****", 
					new[]
					{
						"*****/*...*", 
						".*/*.*/.*",
						".**/*..*/.*",
						//".*/*.*/*.*/.*"
					}
				},

				{
					".*/*****", 
					new string[]{ }
				},
			};
		
		public static readonly IDictionary<string, Func<Tiler.Context, Tiler.UlongStripEnd, bool>> TileHeuristicFunctions =
			new Dictionary<string, Func<Tiler.Context, Tiler.UlongStripEnd, bool>>()
			{
				{
					".***/*******", 
					RuleOut_oxxx_xxxxx
				},

				{
					".*/****", 
					RuleOut_ox_xxxx
				},
				
				{
					".*/*****", 
					RuleOut_ox_xxxxx
				}
			};

		private static bool RuleOutNothing(PagedTiler.Context _, PagedTiler.StripEnd __) => false;
		private static bool RuleOutNothing(Tiler.Context _, Tiler.UlongStripEnd __) => false;
		
		private static bool RuleOut_ox_xxxx(PagedTiler.Context context, PagedTiler.StripEnd potentialEnd)
			=> TileUtils.HasPattern(0b10001, 0b11111, 5, potentialEnd, context);

		private static bool RuleOut_ox_xxxxx(PagedTiler.Context context, PagedTiler.StripEnd potentialEnd)
			=> TileUtils.HasPattern(0b10001, 0b11111, 5, potentialEnd, context)
			   || TileUtils.HasPattern(0b100001, 0b111111, 6, potentialEnd, context)
			   || TileUtils.HasPattern(0b1001001, 0b1111111, 7, potentialEnd, context)
			   ;


		private static bool RuleOut_oxxx_xxxxx(PagedTiler.Context context, PagedTiler.StripEnd potentialEnd) 
			=> TileUtils.HasPattern(0b100001, 0b111111, 6, potentialEnd, context) 
			   || TileUtils.HasPattern(0b1000001, 0b1111111, 7, potentialEnd, context)
			   || TileUtils.HasPattern(0b10000001, 0b11111111, 8, potentialEnd, context);
		
		private static bool RuleOut_ox_xxxx(Tiler.Context context, Tiler.UlongStripEnd potentialEnd)
			=> TileUtils.HasPattern(0b10001, 0b11111, 5, potentialEnd, context);
		
		private static bool RuleOut_ox_xxxxx(Tiler.Context context, Tiler.UlongStripEnd potentialEnd)
			=> TileUtils.HasPattern(0b10001, 0b11111, 5, potentialEnd, context)
			   || TileUtils.HasPattern(0b100001, 0b111111, 6, potentialEnd, context)
			   || TileUtils.HasPattern(0b1001001, 0b1111111, 7, potentialEnd, context)
		;

		private static bool RuleOut_oxxx_xxxxx(Tiler.Context context, Tiler.UlongStripEnd potentialEnd) 
			=> TileUtils.HasPattern(0b100001, 0b111111, 6, potentialEnd, context) 
			   || TileUtils.HasPattern(0b1000001, 0b1111111, 7, potentialEnd, context)
			   || TileUtils.HasPattern(0b10000001, 0b11111111, 8, potentialEnd, context);

		public static void TestOctree()
		{
			var rand = new Random();
			const int maxSize = 129;
			
			(int x, int y, int z) Rand() => (rand.Next(maxSize), rand.Next(maxSize), rand.Next(maxSize)) ;

			float GetNodeCount((int, int, int) size, Octree<int>.GridStrategy strategy)
			{
				(int width, int height, int depth) = size;
				var tree = new Octree<int>(width, height, depth, strategy:strategy);
				int value = 0;

				foreach (var index in tree.Indexes)
				{
					tree[index] = value;
					value++;
				}

				return tree.NodeCount / ((float) width * height * depth);
			}

			float sum = 0;

			int repeatCount = 10;
			
			for (int i = 0; i < repeatCount; i++)
			{
				var size = Rand();
				float nodeCount = GetNodeCount(size, Octree<int>.GridStrategy.MaxCellSize) 
				                - GetNodeCount(size, Octree<int>.GridStrategy.MinCellSize);
				Console.WriteLine(nodeCount);
				sum += nodeCount;
			}

			Console.WriteLine(sum/repeatCount);
		}

		public static void TestTiling()
		{
			//string tileName = ".***/*******";
			//int width = 70;

			string tileName = ".***/*******";
			int width = 35;
			
			var tiles = TileUtils
				.NameToPoints(tileName)
				.GetAllSymmetriesNormalized()
				.Select(points => points.ToUlongTile())
				.ToArray();

#if USE_PAGED
			Pattern MakePattern(string name) => new(new UlongTile(TileUtils.NameToPoints(name)));

			var patterns = PagedTileHeuristicFunctions[tileName].Select(MakePattern).ToArray();

			bool CanRuleOut(PagedTiler.Context context, PagedTiler.StripEnd stripEnd)
				//=> false;
				=> TileUtils.HasPattern(context, stripEnd, patterns[1])
					|| TileUtils.HasPattern(context, stripEnd, patterns[3]) 
					|| TileUtils.HasPattern(context, stripEnd, patterns[0]) 
				    || TileUtils.HasPattern(context, stripEnd, patterns[2])
				   //false
				   ;
			
			var res = PagedTiler.TileRect(tiles, width, CanRuleOut);
#else
			var canRuleOut = TileHeuristicFunctions.ContainsKey(tileName) ? TileHeuristicFunctions[tileName] : RuleOutNothing;
			var res = Tiler.TileRect<UlongTile, Tiler.UlongStripEnd>(tiles, width, canRuleOut);
#endif		
			if (res != null)
			{
				/*
				var (order, size, tiling) = res.Summarize();
				
				Console.WriteLine(order);
				Console.WriteLine(size);
				Console.WriteLine(tiling.ToPrettyString());
				*/
				
				Console.WriteLine("Found a tiling!");
			}
			else
			{
				Console.WriteLine("No tiling found");
			}
		}
		
		public static void TestUlongTiler()
		{
			string tileName = ".***/*******";
			int width = 35;
			
			var tiles = TileUtils
				.NameToPoints(tileName)
				.GetAllSymmetriesNormalized()
				.Select(points => points.ToUlongTile())
				.ToArray();
			
			var res = Tiler.TileRect<UlongTile, Tiler.UlongStripEnd>(tiles, width, RuleOutNothing);

			Console.WriteLine(res ? "Found a tiling!" : "No tiling found");
		}
		
		
		public static void TestUlong2Tiler()
		{
			string tileName = ".***/*******";
			
			var tiles = TileUtils
				.NameToPoints(tileName)
				.GetAllSymmetriesNormalized()
				.Select(points => points.ToUlongTile())
				.ToArray();

			var res = Tiler<Width35, DefaultLongTile<Width35>>.TileRect(tiles);

			Console.WriteLine(res ? "Found a tiling!" : "No tiling found");
		}
		
		[SupportedOSPlatform("windows")]
		public static void Main(string[] args)
		{
			//TileImage.Program.Main1(args);
			
			TestUlongTiler();

			//TestTiling<ListTile, ImmutableStripEnd>();
			//Console.WriteLine("Took: " + Measure(TestTiling)/1000.0f);
			//Console.WriteLine("Took: " + Measure(TestTiling<ListTile, ImmutableStripEnd>)/1000.0f);
			//Console.WriteLine("Took: " + Measure(TestTiling<UlongTile, UlongStripEnd>)/1000.0f);
			//Console.WriteLine("Took: " + Measure(TestTiling<ListTile, ImmutableStripEnd>)/1000.0f);
			//Console.WriteLine("Took: " + Measure(TestTiling<UlongTile, UlongStripEnd>)/1000.0f);
			//Console.WriteLine("Took: " + Measure(TestTiling<ListTile, ImmutableStripEnd>)/1000.0f);

			//Console.WriteLine("Took: " + Measure(TestUlongTiler)/1000.0f); //45.12
			//Console.WriteLine("Took: " + Measure(TestUlongTiler2)/1000.0f); //45.12

			//Measure(TestTiling<ListTile, ImmutableStripEnd>);
			//TestOctree();
			//TestInker();

			//TimeAlgorithms2();

			//TestRandomBag();
			//TestMerge();
			//BinarySearchTreeTest();
			//CombinatorialTest();
			//BasicTest();
			//ProfileStringBuilding();

			//Console.ReadKey();

			var d = BindingDirection.OneWay;

			switch (d)
			{
				case BindingDirection.OneWay:
					Console.Write("a");
					break;
				case BindingDirection.TwoWay:
					Console.Write("b");
					break;
				default:
					throw new InvalidEnumArgumentException(nameof(d), (int) d, d.GetType());
			}
		}

		private static void TestInker()
		{
			var image = new Bitmap("image.jpeg");
			var grid = image.ToGrid();
			var inkedGrid = Inker.Ink(grid, 11);
			var inkedImage = inkedGrid.ToBitmap();
			inkedImage.Save("inkedImage.png");

		}

		public static void TestImage()
		{
			int InCircle(Int2 p, int radius)
				=> p.X * p.X + p.Y * p.Y < radius*radius ? 1 : 0;

			ColorF Fill(Int2 p) => ColorF.Blue;

			ColorF HalfPlane(Int2 p) => p.X >= 0 ? ColorF.Blue : ColorF.Green;

			VeinClassifier vein = new VeinClassifier();

			var colors = new ColorF[]
			{
				ColorF.Red, ColorF.Blue,
			};

			int width = 100;
			var size = new Int2(width, width);
			var pixels = new CenteredGrid(size, ColorF.Red);
			pixels.Paint(p => vein.IsVein(p) ? 1 : 0, colors);

			//Console.WriteLine(pixels.ToString());

			var image = new IdealImage(pixels);
			var raster = image.Rasterize();

			//Console.WriteLine(raster.ToString());

			var bitmap = raster.ToBitmap();

			bitmap.Save("Red.png");
		}

		private static void TimeAlgorithms2()
		{
			const int range = 1000;
			const int experimentCount = 1000;
			const int count = 100000;

			var algorithms = new Action<IList<int>, PerformanceMonitor, PerformanceData>[]
			{
				//Sort.SelectionSort,
				//Sort.SelectionSort_Inline,
				//Sort.InsertionSort,
				//Sort.ShellSort,
				//Sort.MergeSort_Queues,
				//Sort.MergeSort_Iterative_InPlace,
				//Sort.MergeSort_Iterative_InPlace_Smart,
				Sort.MergeSort_Iterative_NewList,
				Sort.MergeSort_Iterative_NewList_Smart,
				//Sort.MergeSort_Recursive_NewList,
				//Sort.MergeSort_Recursive_NewList_Smart,
			};

			var names = new string[]
			{
				//nameof(Sort.SelectionSort),
				//nameof(Sort.SelectionSort_Inline),
				//nameof(Sort.InsertionSort),
				//nameof(Sort.ShellSort),
				//nameof(Sort.MergeSort_Queues),
				//nameof(Sort.MergeSort_Iterative_InPlace),
				//nameof(Sort.MergeSort_Iterative_InPlace_Smart),
				nameof(Sort.MergeSort_Iterative_NewList),
				nameof(Sort.MergeSort_Iterative_NewList_Smart),
				//nameof(Sort.MergeSort_Recursive_NewList),
				//nameof(Sort.MergeSort_Recursive_NewList_Smart),
			};


			var countLabels = new [] { "Big" };
			var tickCountLabels = new [] { "Small", "Big" };
			var data = new  Dictionary<string, PerformanceData[]>();

			foreach (string name in names)
			{
				var dataItem = new PerformanceData[experimentCount];

				for (int i = 0; i < experimentCount; i++)
				{
					dataItem[i] = new PerformanceData(countLabels, tickCountLabels);
				}

				data[name] = dataItem;
			}

			var monitor = new PerformanceMonitor(tickCountLabels);

			long[][] times = new long[algorithms.Length][];

			for (int i = 0; i < algorithms.Length; i++)
			{
				times[i] = new long[experimentCount];
			}

			for (int i = 0; i < experimentCount; i++)
			{
				var list = RandomList.New(range).Take(count);

				for (int j = 0; j < algorithms.Length; j++)
				{
					var copy = list.ToList();
					algorithms[j](copy, monitor, data[names[j]][j]);
				}
			}

			foreach (string name in names)
			{
				Console.WriteLine(name);
				Console.Write(data[name].Average().ReportString());
			}
		}

		private static void TimeAlgorithms()
		{
			const int range = 1000;
			const int experimentCount = 100;
			const int interval = 10000;
			const int intervalCount = 5;

			var algorithms = new Action<IList<int>>[]
			{
				Sort.SelectionSort,
				Sort.SelectionSort_Inline,
				Sort.InsertionSort,
				Sort.ShellSort,
				Sort.MergeSort_Queues,
				Sort.MergeSort_Iterative_InPlace,
				Sort.MergeSort_Iterative_InPlace_Smart,
				Sort.MergeSort_Iterative_NewList,
				Sort.MergeSort_Iterative_NewList_Smart,
				Sort.MergeSort_Recursive_NewList,
				Sort.MergeSort_Recursive_NewList_Smart,
			};

			var names = new string[]
			{
				nameof(Sort.SelectionSort),
				nameof(Sort.SelectionSort_Inline),
				nameof(Sort.InsertionSort),
				nameof(Sort.ShellSort),
				nameof(Sort.MergeSort_Queues),
				nameof(Sort.MergeSort_Iterative_InPlace),
				nameof(Sort.MergeSort_Iterative_InPlace_Smart),
				nameof(Sort.MergeSort_Iterative_NewList),
				nameof(Sort.MergeSort_Iterative_NewList_Smart),
				nameof(Sort.MergeSort_Recursive_NewList),
				nameof(Sort.MergeSort_Recursive_NewList_Smart),
			};

			Console.Write("n");
			Console.Write("\t");

			for (int i = 0; i < algorithms.Length; i++)
			{
				Console.Write(names[i]);
				Console.Write("\t");
			}

			Console.WriteLine();

			for (int k = 1; k <= intervalCount; k++)
			{
				int count = interval * k;
				long[][] times = new long[algorithms.Length][];

				for (int i = 0; i < algorithms.Length; i++)
				{
					times[i] = new long[experimentCount];
				}

				for (int i = 0; i < experimentCount; i++)
				{
					var list = RandomList.New(range).Take(count);

					for (int j = 0; j < algorithms.Length; j++)
					{
						var copy = list.ToList();
						int j1 = j;
						times[j][i] = Measure(() => algorithms[j1](copy));
					}
				}

				Console.Write(count);
				Console.Write("\t");

				for (int j = 0; j < algorithms.Length; j++)
				{
					Console.Write(times[j].Average());
					Console.Write("\t");
				}

				Console.WriteLine();
			}
		}

		private static long Measure(Action action)
		{
			var watch = new Stopwatch();
			watch.Start();
			action();
			watch.Stop();

			return watch.ElapsedMilliseconds;
		}

		private static void TestRandomBag()
		{
			var bag = new RandomBag2<int>
			{
				0, 1, 2, 3, 4, 5, 6, 7, 8, 9
			};

			Console.WriteLine(bag.ToPrettyString());
			Console.WriteLine(bag.ToPrettyString());
			Console.WriteLine(bag.ToPrettyString());
		}

		private static void TestMerge()
		{
			int[] list = {
				0, 6, 3, 6,
				1, 8, 3, 2
			};

			Sort.MergeSort_Iterative_InPlace(list);
			Console.WriteLine(list.ToPrettyString());
		}

		private static void ProfileStringBuilding()
		{
			const int elementCount = 10000;
			var list = SimpleList<int>.SimpleListEmpty;

			for (int i = 0; i < elementCount; i++)
			{
				list = list.Push(i);
			}

			var watch = Stopwatch.StartNew();
			var str = list.ToUnbracketedString();

			watch.Stop();

			Console.WriteLine(watch.ElapsedMilliseconds);
			Console.WriteLine(str);
		}

		private static void BasicTest()
		{
			var list1 = SimpleList<int>.SimpleListEmpty.Push(1, 2, 3);
			var list2 = SimpleList<int>.SimpleListEmpty.Push(4, 5, 6);
			var list3 = list2.Concat(list1);

			var list4 = SimpleList<int>.SimpleListEmpty.Append(1, 2, 3);

			var list5 = SimpleList<int>.HughesListEmpty.Push(1, 2, 3);
			var list6 = SimpleList<int>.HughesListEmpty.Push(4, 5, 6);
			var list7 = list6.Concat(list5);
			var list8 = list7.Reverse();

			Console.WriteLine(list1);
			Console.WriteLine(list2);
			Console.WriteLine(list3);
			Console.WriteLine(list4);
			Console.WriteLine(list5);
			Console.WriteLine(list6);
			Console.WriteLine(list7);
			Console.WriteLine(list8);
		}

		private static void CombinatorialTest()
		{
			var radixes1 = new int[] {2, 3};
			var radixes2 = new int[] {0};

			var tuples1 = Combinatorial.MultiRadixTuples(radixes1);
			var tuples2 = Combinatorial.MultiRadixTuples(radixes2);

			var tuples = new[]
			{
				tuples1, tuples2
			};

			Console.WriteLine(tuples.ToPrettyString());
		}

		private static void BinarySearchTreeTest()
		{
			var tree = new BinarySearchTree<string>()
			{
				[3] = "three",
				[5] = "five",
				[1] = "one",
				[2] = "two",
				[7] = "seven"
			};

			Console.WriteLine(tree.ToPrettyString());
			Console.WriteLine(tree.ToRepresentation());
			Console.WriteLine(tree.Count);
			Console.WriteLine(tree.Depth);
			Console.WriteLine(tree.EmptyCount);

			foreach ((int key, string value) in tree)
			{
				Console.WriteLine($"{key}, {value}");
			}
		}
	}
}
