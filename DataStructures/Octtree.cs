using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DataStructures;

//Z is depth
public interface IOctree<T>
{
	/// <summary>
	/// The width of the grid this octree represents. 
	/// </summary>
	public int Width { get; }
	
	/// <summary>
	/// The height of the grid this octree represents.
	/// </summary>
	public int Height { get; }
	
	/// <summary>
	/// The depth of the grid this octree represents.
	/// </summary>
	public int Depth { get; }
	
	[Private(ExposedFor.PerformanceMonitoring)]
	public int NodeCount { get; }

	public IEnumerable<(int x, int y, int z)> Indexes { get; }

	/// <summary>
	/// Gets and sets the value of the octree at the given indexes.
	/// </summary>
	/// <param name="x">The horizontal index.</param>
	/// <param name="y">The vertical index.</param>
	/// /// <param name="z">The depth index.</param>
	/// <exception cref="ArgumentOutOfRangeException">when either index is negative or greater than or equals to <see cref="Width"/> and <see cref="Height"/>
	/// respectively.</exception>
	public T this[int x, int y, int z] { get; set; }
	
	/// <summary>
	/// Gets and sets the value of the octree at the given indexes (combined in a tuple).
	/// </summary>
	/// <param name="index">A tuple that contains the x and y index to get or set the value at.</param>
	/// <exception cref="ArgumentOutOfRangeException">when either index is negative or greater than or equals to <see cref="Width"/> and <see cref="Height"/>
	/// respectively.</exception>
	public T this[(int x, int y, int z) index] { get; set; }
	
	/// <summary>
	/// Sets all the elements in this octree to the same value. 
	/// </summary>
	/// <param name="initialElement">The value to set the octree to. If not given then <see langword="default"/> is used.</param>
	public void Clear(T initialElement = default);
}

public static class Octree
{
	/// <summary>
	/// Returns whether the given number is a power of 2. 
	/// </summary>
	/// <remarks>Zero (0) is not a power of two, one (1) is.</remarks>
	public static bool IsPot(int n) => n > 0 && (n == 1 || IsPot(n >> 1));
	
	/// <summary>
	/// Checks whether the given number is equal to or greater than the given minimum, and smaller than the maximum. 
	/// </summary>
	public static bool IndexInRange(int index, int min, int max) => index >= min && index < max;
	
	/// <summary>
	/// Returns the lowest power of two equal to or greater than the given number.
	/// </summary>
	/// <remarks>Returns 1 if <paramref name="n"/> is smaller than 1.</remarks>
	public static int NextPot(int n) => 
		n < 1 ? 1 :
		IsPot(n) ? n : 1 << (Log2_Unchecked(n) + 1);

	public static int Log2_Unchecked(int n)
	{
		int log = 0;

		while (n != 1)
		{
			n >>= 1;
			log++;
		}

		return log;
	}
}

/// <summary>
/// A 3D data structure that supports random access through indices suitable for representing data where large blocks of cells could gave the same values.
/// </summary>
/// <typeparam name="T">The type of data this tree holds.</typeparam>
/// <remarks>This base supports a variety of octrees used for implementation; the one to use is <see cref="Octree"/>.</remarks>
public abstract class OctreeBase<T> : IOctree<T>
{
	/// <inheritdoc />
	public abstract int Width { get; }
	
	/// <inheritdoc />
	public abstract int Height { get; }
	
	/// <inheritdoc />
	public abstract int Depth { get; }

	/// <summary>
	/// How many elements this octree hold.
	/// </summary>
	public int Count => Width * Height;

	public abstract int NodeCount { get; }

	public IEnumerable<(int x, int y, int z)> Indexes
	{
		get
		{
			for(int k = 0; k < Depth; k++)
			{
				for(int j = 0; j < Height; j++)
				{
					for (int i = 0; i < Width; i++)
					{
						yield return (i, j, k);
					}
				}
			}
		}
	}

	/// <inheritdoc />
	public abstract T this[int x, int y, int z] { get; set; }
	
	/// <inheritdoc />
	public abstract T this[(int x, int y, int z) index] { get; set; }

	/// <inheritdoc />
	public abstract void Clear(T initialElement = default);

	/// <summary>
	/// Checks whether the given indices are within the range of this octree. 
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">the indexes are not in range.</exception>
	protected void ValidateIndexesInRange(int x, int y, int z)
	{
		if (!Octree.IndexInRange(x, 0, Width)) throw new ArgumentOutOfRangeException(nameof(x));
		if (!Octree.IndexInRange(y, 0, Height)) throw new ArgumentOutOfRangeException(nameof(y));
		if (!Octree.IndexInRange(z, 0, Depth)) throw new ArgumentOutOfRangeException(nameof(z));
	}
}

/// <remarks>
///	<para>The tree is a square, and the size (width and height) must be a power of two. For a octree that supports all sizes see <see cref="Octree"/>.</para>
/// <para>The tree restructures itself automatically, and is kept as lean as possible, that is four leave nodes with the same parent cannot have the same value.</para>
/// <para><see cref="object.Equals(object?)"/> is used to make comparisons.</para>
/// </remarks>
public class CubePotOctree<T> : OctreeBase<T>
{
	private sealed class Oct 
	{
		public BaseNode Node;
		
		public T this[int x, int y, int z]
		{
			get
			{
				AssertIndexesInRange(x, y, z);
				
				return Node switch
				{
					Leaf leaf => leaf.Value,
					InternalNode internalNode => internalNode[x, y, z],
					_ => throw Exceptions.TypeCaseNotImplemented(Node, nameof(Node))
				};
			}

			set
			{
				AssertIndexesInRange(x, y, z);
				
				switch (Node)
				{
					case Leaf leaf when leaf.Value.Equals(value):
						break;
					
					case Leaf { Size: 1 } leaf:
						leaf.Value = value;
						break;
					
					case Leaf leaf:
						Node = new InternalNode(leaf.Size, leaf.Value)
						{
							[x, y, z] = value
						};
						break;
					
					case InternalNode internalNode when internalNode.ShouldBeLeaf(x, y, z, value):
						Node = new Leaf(internalNode.Size, value);
						break;
						
					case InternalNode internalNode:
						internalNode[x, y, z] = value;
						break;
					
					default:
						throw Exceptions.TypeCaseNotImplemented(Node, nameof(Node));
				}
			}
		}
		
		public void Clear(T initialElement = default)
		{
			switch (Node)
			{
				case Leaf leaf:
					leaf.Value = initialElement;
					break;
				
				case InternalNode:
					Node = new Leaf(Node.Size, initialElement);
					break;
					
				default:
					throw Exceptions.TypeCaseNotImplemented(Node, nameof(Node));
			}
		}

		public int NodeCount => Node.NodeCount;

		public Oct(int size, T value)
		{
			Node = new Leaf(size, value);
		}
		
		[Conditional("DEBUG")]
		private void AssertIndexesInRange(int x, int y, int z)
		{
			bool IndexInRange(int index) => Octree.IndexInRange(index, 0, Node.Size);
			
			GLDebug.Assert(IndexInRange(x));
			GLDebug.Assert(IndexInRange(y));
			GLDebug.Assert(IndexInRange(z));
		}
	}
	
	private abstract class BaseNode 
	{
		public readonly int Size;
		protected readonly int HalfSize;

		public abstract int NodeCount { get; }

		protected BaseNode(int size)
		{
			GLDebug.Assert(Octree.IsPot(2));
			
			Size = size;
			HalfSize = size / 2;
		}

		public abstract string ToStructureString();
	}

	private sealed class Leaf : BaseNode
	{
		public T Value;

		public override int NodeCount => 1;

		public Leaf(int size, T value)
			: base(size)
		{
			Value = value;
		}

		public override string ToStructureString() => ".";
	}

	private sealed class InternalNode:BaseNode
	{
		private readonly Oct[] children = new Oct [OctCount];

		public override int NodeCount => 1 + children.Sum(n => n.NodeCount);

		public InternalNode(int size, T value)
			: base(size)
		{
			for (int i = 0; i < OctCount; i++)
			{
				children[i] = new Oct(size / 2, value);
			}
		}
		
		public T this[int x, int y, int z]
		{
			get
			{
				(var child, int newX, int newY, int newZ) = GetChild(x, y, z);

				return child[newX, newY, newZ];
			}

			set
			{
				(var child, int newX, int newY, int newZ) = GetChild(x, y, z);

				child[newX, newY, newZ] = value;
			}
		}

		private (Oct child, int newX, int newY, int newZ) GetChild(int x, int y, int z)
		{
			int bigX = x < HalfSize ? 0 : 1;
			int bigY = y < HalfSize ? 0 : 1;
			int bigZ = z < HalfSize ? 0 : 1;
			
			int index = bigZ * 4 + bigY * 2 + bigX;
			var child = children[index];

			int newX = x - HalfSize * bigX;
			int newY = y - HalfSize * bigY;
			int newZ = z - HalfSize * bigZ;
			
			return (child, newX, newY, newZ);
		}

		public bool ShouldBeLeaf(int x, int y, int z, T value)
		{
			bool SiblingsAreLeavesEqualToValue(Oct oct)
			{
				for (int i = 0; i < OctCount; i++)
				{
					var sibling = children[i];

					if (sibling == oct) continue; //Given oct is not a sibling

					var node = sibling.Node;
					
					switch (node)
					{
						case Leaf leaf1 when leaf1.Value.Equals(value):
							break;

						case Leaf:
						case InternalNode:
							return false;

						default:
							throw Exceptions.TypeCaseNotImplemented(sibling.Node, nameof(node));
					}
				}

				return true;
			}

			(var affectedChild, int newX, int newY, int newZ) = GetChild(x, y, z);
			
			if (!SiblingsAreLeavesEqualToValue(affectedChild)) return false;
			
			var node = affectedChild.Node;

			if (node is Leaf { Size: 1 } leaf)
			{
				GLDebug.Assert(!leaf.Value.Equals(value));
			}
			
			return node switch
			{
				Leaf leaf2 => leaf2.Size == 1,
				InternalNode internalNode2 => internalNode2.ShouldBeLeaf(newX, newY, newZ, value),
				_ => throw Exceptions.TypeCaseNotImplemented(node, nameof(node))
			};
		}

		public override string ToStructureString()
		{
			string innerString = string.Join(string.Empty, children.Select(child => child.Node.ToStructureString()));

			return $"[{innerString}]";
		}
	}
	
	private const int OctCount = 8;
	private readonly Oct root;
	
	/// <summary>
	/// The width and height of this octree.
	/// </summary>
	public int Size => root.Node.Size;

	/// <inheritdoc />
	public override int Height => Size;
	
	/// <inheritdoc />
	public override int Width => Size;
	
	/// <inheritdoc />
	public override int Depth => Size;

	public override int NodeCount => root.NodeCount;

	public CubePotOctree(int size, T initialElement = default)
	{
		if (!Octree.IsPot(size)) throw new ArgumentException("Only powers of two are supported.", nameof(size));
		
		root = new Oct(size, initialElement);
	}
	
	public override T this[int x, int y, int z]
	{
		get
		{
			ValidateIndexesInRange(x, y, z);
			return root[x, y, z];
		}
		
		set
		{
			ValidateIndexesInRange(x, y, z);
			root[x, y, z] = value;
		}
	}
	
	public override T this[(int x, int y, int z) index]
	{
		get => this[index.x, index.y, index.z];
		set => this[index.x, index.y, index.z] = value;
	}

	public override void Clear(T initialElement = default) => root.Clear(initialElement);

	public string ToStructureString() => root.Node.ToStructureString();
}

public sealed class Octree<T> : OctreeBase<T>
{
	public enum GridStrategy
	{
		MaxCellSize, //single cell
		MinCellSize
	}
	
	private sealed class OctreeGrid : OctreeBase<T>
	{
		private readonly IOctree<T>[,,] grid;
		private readonly int internalSize;
		
		private readonly int gridDepth;
		private readonly int gridHeight;
		private readonly int gridWidth;

		public override int NodeCount
		{
			get
			{
				int sum = 0;
				
				for(int k = 0; k < gridDepth; k++)
				{
					for(int j = 0; j < gridHeight; j++)
					{
						for (int i = 0; i < gridWidth; i++)
						{
							sum += grid[i, j, k].NodeCount;
						}
					}
				}
				
				return sum;
			}
		}
		
		public OctreeGrid(Func<int, T, IOctree<T>> octreeFactory, int gridWidth, int gridHeight, int gridDepth, int internalSize, T initialElement = default)
		{
			this.gridWidth = gridWidth;
			this.gridHeight = gridHeight;
			this.gridDepth = gridDepth;
			this.internalSize = internalSize;
			
			Width = gridWidth * internalSize;
			Height = gridHeight * internalSize;
			Depth = gridDepth * internalSize;

			grid = new IOctree<T>[gridWidth, gridHeight, gridDepth];
			
			for(int k = 0; k < gridDepth; k++)
			{
				for(int j = 0; j < gridHeight; j++)
				{
					for (int i = 0; i < gridWidth; i++)
					{
						grid[i, j, k] = octreeFactory(internalSize, initialElement);
					}
				}
			}
		}

		public override int Width { get; }
		public override int Height { get; }
		public override int Depth { get; }
		
		public override T this[int x, int y, int z]
		{
			get
			{
				ValidateIndexesInRange(x, y, z);
				var (gridIndex, treeIndex) = GetIndexes(x, y, z);

				return GetTree(treeIndex)[gridIndex];
			}
			set
			{
				ValidateIndexesInRange(x, y, z);
				var (gridIndex, treeIndex) = GetIndexes(x, y, z);

				GetTree(gridIndex)[treeIndex] = value;
			}
		}

		private IOctree<T> GetTree((int x, int y, int z) gridIndex) => grid[gridIndex.x, gridIndex.y, gridIndex.z];

		public override T this[(int x, int y, int z) index]
		{
			get => this[index.x, index.y, index.z];
			set => this[index.x, index.y, index.z] = value;
		}

		public override void Clear(T initialElement = default)
		{
			for(int k = 0; k < gridDepth; k++)
			{
				for(int j = 0; j < gridHeight; j++)
				{
					for (int i = 0; i < gridWidth; i++)
					{
						grid[i, j, k] = new CubePotOctree<T>(internalSize, initialElement);
					}
				}
			}
		}

		private ((int x, int y, int z) gridIndex, (int x, int y, int z) treeIndex) GetIndexes(int x, int y, int z)
		{
			int gridX = x / internalSize;
			int gridY = y / internalSize;
			int gridZ = z / internalSize;
			
			int treeX = x % internalSize;
			int treeY = y % internalSize;
			int treeZ = z % internalSize;

			return ((gridX, gridY, gridZ), (treeX, treeY, treeZ));
		}
	}

	private readonly OctreeGrid grid;

	/// <inheritdoc/>
	public override int Height => grid.Height;

	/// <inheritdoc/>
	public override int Width => grid.Width;

	/// <inheritdoc/>
	public override int Depth => grid.Depth;

	public override int NodeCount => grid.NodeCount;

	/// <summary>
	/// Constructs a new octree of arbitrary width and height using <see cref="CubePotOctree{T}"/> us the underlying octree.
	/// </summary>
	public Octree(int width, int height, int depth, T initialElement = default, GridStrategy strategy = GridStrategy.MaxCellSize) :
		this((size, element) => new CubePotOctree<T>(size, element), width, height, depth, initialElement, strategy)
	{ }

	/// <summary>
	/// Constructs a new octree of arbitrary width and height.
	/// </summary>
	/// <param name="octreeFactory">A factory method that produces square power-of-two octrees.</param>
	/// <param name="width">The width of the octree.</param>
	/// <param name="height">The height of the octree.</param>
	/// /// <param name="depth">The depth of the octree.</param>
	/// <param name="initialElement">The initial element of the octree (the value of all cells). If not given <see langword="default"/> is used.</param>
	/// <param name="strategy">What strategy to use for choosing internal grid</param>
	//Different strategies can be used to choose appropriate dimensions for the tree grid.
	//It looks like GridStrategy.MinCellSize leads to fewer nodes on average when the tree is very filled. 
	public Octree(Func<int, T, IOctree<T>> octreeFactory,
		int width, int height, int depth, 
		T initialElement = default,
		GridStrategy strategy = GridStrategy.MinCellSize)
	{
		Func<int, int, int,  (int, int, int, int)> getGridDimensions = 
			strategy switch
			{
				GridStrategy.MaxCellSize => GetMaxGridDimensions,
				_ => GetMinGridDimensions
			};

		(int gridWidth, int gridHeight, int gridDepth, int internalSize) = getGridDimensions(width, height, depth);

		
		
		grid = new OctreeGrid(octreeFactory, gridWidth, gridHeight, gridDepth, internalSize, initialElement);
	}

	private static (int, int, int, int) GetMinGridDimensions(int width, int height, int depth)
	{
		int minDimension = Math.Min(Math.Min(width, height), depth);
		int internalSize = Quadtree.NextPot(minDimension);

		int GetGridSize(int size) => (int)Math.Ceiling(size / (float)internalSize);

		int gridWidth = GetGridSize(width);
		int gridHeight = GetGridSize(height);
		int gridDepth = GetGridSize(depth);

		return (gridWidth, gridHeight, gridDepth, internalSize);
	}
	
	private static (int, int, int, int) GetMaxGridDimensions(int width, int height, int depth)
	{
		int maxDimension = Math.Max(Math.Max(width, height), depth);
		int internalSize = Quadtree.NextPot(maxDimension);

		int GetGridSize(int size) => (int)Math.Ceiling(size / (float)internalSize);

		int gridWidth = GetGridSize(width);
		int gridHeight = GetGridSize(height);
		int gridDepth = GetGridSize(depth);

		return (gridWidth, gridHeight, gridDepth, internalSize);
	}

	/// <inheritdoc/>
	public override T this[int x, int y, int z]
	{
		get 
		{
			ValidateIndexesInRange(x, y, z);
			return grid[x, y, z];
		}

		set
		{
			ValidateIndexesInRange(x, y, z);
			grid[x, y, z] = value;
		} 
	}

	/// <inheritdoc/>
	public override T this[(int x, int y, int z) index]
	{
		get => this[index.x, index.y, index.z];
		set => this[index.x, index.y, index.z] = value;
	}

	/// <inheritdoc/>
	public override void Clear(T initialElement = default) => grid.Clear(initialElement);
}
