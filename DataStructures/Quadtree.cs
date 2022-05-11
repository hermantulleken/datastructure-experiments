using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DataStructures;

public interface IQuadtree<T>
{
	/// <summary>
	/// The width of the grid this quadtree represents. 
	/// </summary>
	public int Width { get; }
	
	/// <summary>
	/// The height of the grid this quadtree represents.
	/// </summary>
	public int Height { get; }
	
	public IEnumerable<(int x, int y)> Indexes { get; }
	
	/// <summary>
	/// Gets and sets the value of the quadtree at the given indexes.
	/// </summary>
	/// <param name="x">The horizontal index.</param>
	/// <param name="y">The vertical index.</param>
	/// <exception cref="ArgumentOutOfRangeException">when either index is negative or greater than or equals to <see cref="Width"/> and <see cref="Height"/>
	/// respectively.</exception>
	public T this[int x, int y] { get; set; }
	
	/// <summary>
	/// Gets and sets the value of the quadtree at the given indexes (combined in a tuple).
	/// </summary>
	/// <param name="index">A tuple that contains the x and y index to get or set the value at.</param>
	/// <exception cref="ArgumentOutOfRangeException">when either index is negative or greater than or equals to <see cref="Width"/> and <see cref="Height"/>
	/// respectively.</exception>
	public T this[(int x, int y) index] { get; set; }
	
	/// <summary>
	/// Sets all the elements in this quadtree to the same value. 
	/// </summary>
	/// <param name="initialElement">The value to set the quadtree to. If not given then <see langword="default"/> is used.</param>
	public void Clear(T initialElement = default);
}

public static class Quadtree
{
	/// <summary>
	/// Returns whether the given number is a power of 2. 
	/// </summary>
	/// <remarks>Zero (0) is not a power of two, one (1) is.</remarks>
	public static bool IsPot(int n) => n > 0 && (n == 1 || (n % 2 == 0 && IsPot(n >> 1)));
	
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

	public static int NextMultiple(int n, int factor)
		=> (n % factor == 0) ? factor :
			n > 0 ? (n / factor + 1) * factor :
			(n / factor) * factor; // ((-5 / 3) * 3) -> -1 * 3 -> -3  

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
/// A 2D data structure that supports random access through indices suitable for representing data where large blocks of cells could gave the same values.
/// </summary>
/// <typeparam name="T">The type of data this tree holds.</typeparam>
/// <remarks>This base supports a variety of quadtrees used for implementation; the one to use is <see cref="Quadtree"/>.</remarks>
public abstract class QuadtreeBase<T> : IQuadtree<T>
{
	/// <inheritdoc />
	public abstract int Width { get; }
	
	/// <inheritdoc />
	public abstract int Height { get; }

	/// <summary>
	/// How many elements this quadtree hold.
	/// </summary>
	public int Count => Width * Height;
	
	public IEnumerable<(int x, int y)> Indexes
	{
		get
		{
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					yield return (x, y);
				}
			}
		}
	}
	
	/// <inheritdoc />
	public abstract T this[int x, int y] { get; set; }
	
	/// <inheritdoc />
	public abstract T this[(int x, int y) index] { get; set; }

	/// <inheritdoc />
	public abstract void Clear(T initialElement = default);
	
	/// <summary>
	/// Checks whether the given indices are within the range of this quadtree. 
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">the indexes are not in range.</exception>
	protected void ValidateIndexesInRange(int x, int y)
	{
		if (!Quadtree.IndexInRange(x, 0, Width)) throw new ArgumentOutOfRangeException(nameof(x));
		if (!Quadtree.IndexInRange(y, 0, Height)) throw new ArgumentOutOfRangeException(nameof(y));
	}
}

/// <remarks>
///	<para>The tree is a square, and the size (width and height) must be a power of two. For a quadtree that supports all sizes see <see cref="Quadtree"/>.</para>
/// <para>The tree restructures itself automatically, and is kept as lean as possible, that is four leave nodes with the same parent cannot have the same value.</para>
/// <para><see cref="object.Equals(object?)"/> is used to make comparisons.</para>
/// </remarks>
public class SquarePotQuadtree<T> : QuadtreeBase<T>
{
	private sealed class Quad 
	{
		public BaseNode Node;
		
		public Quad(int size, T value)
		{
			Node = new Leaf(size, value);
		}
		
		public T this[int x, int y]
		{
			get
			{
				AssertIndexesInRange(x, y);
				
				return Node switch
				{
					Leaf leaf => leaf.Value,
					InternalNode internalNode => internalNode[x, y],
					_ => throw Exceptions.TypeCaseNotImplemented(Node, nameof(Node))
				};
			}

			set
			{
				AssertIndexesInRange(x, y);
				
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
							[x, y] = value
						};
						break;
					
					case InternalNode internalNode when internalNode.ShouldBeLeaf(x, y, value):
						Node = new Leaf(internalNode.Size, value);
						break;
						
					case InternalNode internalNode:
						internalNode[x, y] = value;
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
		
		[Conditional("DEBUG")]
		private void AssertIndexesInRange(int x, int y)
		{
			bool IndexInRange(int index) => Quadtree.IndexInRange(index, 0, Node.Size);
			
			Debug.Assert(IndexInRange(x));
			Debug.Assert(IndexInRange(y));
		}
	}
	
	private abstract class BaseNode 
	{
		public readonly int Size;
		protected readonly int HalfSize;
		
		protected BaseNode(int size)
		{
			Debug.Assert(Quadtree.IsPot(2));
			
			Size = size;
			HalfSize = size / 2;
		}

		public abstract string ToStructureString();
	}

	private sealed class Leaf : BaseNode
	{
		public T Value;

		public Leaf(int size, T value)
			: base(size)
		{
			Value = value;
		}

		public override string ToStructureString() => ".";
	}

	private sealed class InternalNode:BaseNode
	{
		private readonly Quad[] children = new Quad [QuadCount];

		public InternalNode(int size, T value)
			: base(size)
		{
			for (int i = 0; i < QuadCount; i++)
			{
				children[i] = new Quad(size / 2, value);
			}
		}
		
		public T this[int x, int y]
		{
			get
			{
				(var child, int newX, int newY) = GetChild(x, y);

				return child[newX, newY];
			}

			set
			{
				(var child, int newX, int newY) = GetChild(x, y);

				child[newX, newY] = value;
			}
		}

		private (Quad child, int newX, int newY) GetChild(int x, int y)
		{
			int bigX = x < HalfSize ? 0 : 1;
			int bigY = y < HalfSize ? 0 : 1;
			
			int index = bigY * 2 + bigX;
			var child = children[index];

			int newX = x - HalfSize * bigX;
			int newY = y - HalfSize * bigY;
			
			return (child, newX, newY);
		}

		public bool ShouldBeLeaf(int x, int y, T value)
		{
			bool SiblingsAreLeavesEqualToValue(Quad quad)
			{
				for (int i = 0; i < QuadCount; i++)
				{
					var sibling = children[i];

					if (sibling == quad) continue; //Original quad is not a sibling

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

			(var affectedChild, int newX, int newY) = GetChild(x, y);
			
			if (!SiblingsAreLeavesEqualToValue(affectedChild)) return false;
			
			var node = affectedChild.Node;

			if (node is Leaf { Size: 1 } leaf)
			{
				Debug.Assert(!leaf.Value.Equals(value));
			}
			
			return node switch
			{
				Leaf leaf2 => leaf2.Size == 1,
				InternalNode internalNode2 => internalNode2.ShouldBeLeaf(newX, newY, value),
				_ => throw Exceptions.TypeCaseNotImplemented(node, nameof(node))
			};
		}

		public override string ToStructureString()
		{
			string innerString = string.Join(string.Empty, children.Select(child => child.Node.ToStructureString()));

			return $"[{innerString}]";
		}
	}
	
	private const int QuadCount = 4;
	private readonly Quad quad;
	
	/// <summary>
	/// The width and height of this quadtree.
	/// </summary>
	public int Size => quad.Node.Size;

	/// <inheritdoc />
	public override int Height => Size;
	
	/// <inheritdoc />
	public override int Width => Size;
	
	public SquarePotQuadtree(int size, T initialElement = default)
	{
		if (!Quadtree.IsPot(size)) throw new ArgumentException("Only powers of two are supported.", nameof(size));
		
		quad = new Quad(size, initialElement);
	}
	
	public override T this[int x, int y]
	{
		get
		{
			ValidateIndexesInRange(x, y);
			return quad[x, y];
		}
		
		set
		{
			ValidateIndexesInRange(x, y);
			quad[x, y] = value;
		}
	}
	
	public override T this[(int x, int y) index]
	{
		get => this[index.x, index.y];
		set => this[index.x, index.y] = value;
	}

	public override void Clear(T initialElement = default) => quad.Clear(initialElement);

	

	public string ToStructureString() => quad.Node.ToStructureString();
}

public sealed class Quadtree<T> : QuadtreeBase<T>
{
	private abstract class TreeList : QuadtreeBase<T>
	{
		protected readonly int TreeSize;
		
		private readonly IQuadtree<T>[] trees;

		protected TreeList(Func<int, T, IQuadtree<T>> quadtreeFactory, int treeCount, int treeSize, T initialElement = default)
		{
			Debug.Assert(treeCount >= 1);
			Debug.Assert(Quadtree.IsPot(treeSize));
			
			TreeSize = treeSize;
			trees = new IQuadtree<T>[treeCount];

			for (int i = 0; i < treeCount; i++)
			{
				trees[i] = quadtreeFactory(treeSize, initialElement);
			}
		}
		
		public override T this[int x, int y]
		{
			get
			{
				ValidateIndexesInRange(x, y);
				(int treeIndex, int newX, int newY) = GetIndexes(x, y);
				return trees[treeIndex][newX, newY];
			}

			set
			{
				ValidateIndexesInRange(x, y);
				(int treeIndex, int newX, int newY) = GetIndexes(x, y);
				trees[treeIndex][newX, newY] = value;
			}
		}
		
		public override T this[(int x, int y) index]
		{
			get => this[index.x, index.y];
			set => this[index.x, index.y] = value;
		}

		public override void Clear(T initialElement = default)
		{
			foreach (var tree in trees)
			{
				tree.Clear(initialElement);
			}
		}

		protected abstract (int treeIndex, int x, int y) GetIndexes(int x, int y);
	}

	private sealed class HorizontalQuadtree : TreeList
	{
		public override int Width { get; }
		public override int Height { get; }

		public HorizontalQuadtree(Func<int, T, IQuadtree<T>> quadtreeFactory, int width, int height, T initialElement = default)
			: base(quadtreeFactory, width / height, height, initialElement)
		{
			Debug.Assert(Quadtree.IsPot(width));
			Debug.Assert(Quadtree.IsPot(height));
			Debug.Assert(width >= height);
			
			//If all the above is true, width / height is a positive integer

			Width = width;
			Height = height;
		}

		protected override (int treeIndex, int x, int y) GetIndexes(int x, int y)
		{
			int newX = x % TreeSize;
			int treeIndex = x / TreeSize;

			return (treeIndex, newX, y);
		}
	}

	private sealed class VerticalQuadtree : TreeList
	{
		public override int Width { get; }
		public override int Height { get; }
		
		public VerticalQuadtree(Func<int, T, IQuadtree<T>> quadtreeFactory, int width, int height, T initialElement = default)
			: base(quadtreeFactory, height / width, height, initialElement)
		{
			Debug.Assert(Quadtree.IsPot(width));
			Debug.Assert(Quadtree.IsPot(height));
			Debug.Assert(height >= width);
			
			//If all the above is true, height / width is a positive integer
			
			Width = width;
			Height = height;
		}

		protected override (int treeIndex, int x, int y) GetIndexes(int x, int y)
		{
			int newY = y % TreeSize;
			int treeIndex = y / TreeSize;

			return (treeIndex, x, newY);
		}
	}

	private readonly TreeList trees;

	/// <inheritdoc/>
	public override int Height { get; }
	
	/// <inheritdoc/>
	public override int Width { get; }

	/// <summary>
	/// Constructs a new quadtree of arbitrary width and height using <see cref="SquarePotQuadtree{T}"/> us the underlying quadtree.
	/// </summary>
	public Quadtree(int width, int height, T initialElement = default) :
		this((size, element) => new SquarePotQuadtree<T>(size, element), width, height, initialElement)
	{ }

	/// <summary>
	/// Constructs a new quadtree of arbitrary width and height.
	/// </summary>
	/// <param name="quadtreeFactory">A factory method that produces square power-of-two quadtrees.</param>
	/// <param name="width">The width of the quadtree.</param>
	/// <param name="height">The height of the quadtree.</param>
	/// <param name="initialElement">The initial element of the quadtree (the value of all cells). If not given <see langword="default"/> is used.</param>
	public Quadtree(Func<int, T, IQuadtree<T>> quadtreeFactory, int width, int height, T initialElement = default)
	{
		Width = width;
		Height = height;
		
		int newWidth = Quadtree.NextPot(width);
		int newHeight = Quadtree.NextPot(height);

		if (newWidth >= newHeight)
		{
			trees = new HorizontalQuadtree(quadtreeFactory, width, height, initialElement);
		}
		else
		{
			trees = new VerticalQuadtree(quadtreeFactory, width, height, initialElement);
		}
	}

	/// <inheritdoc/>
	public override T this[int x, int y]
	{
		get
		{
			ValidateIndexesInRange(x, y);
			return trees[x, y];
		}

		set
		{
			ValidateIndexesInRange(x, y);
			trees[x, y] = value;
		}
	}

	/// <inheritdoc/>
	public override T this[(int x, int y) index]
	{
		get => this[index.x, index.y];
		set => this[index.x, index.y] = value;
	}

	/// <inheritdoc/>
	public override void Clear(T initialElement = default) => trees.Clear(initialElement);
}
