using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace DataStructures.Trie;

public class Tree<T>
{
	/// <summary>
	/// Used to construct trees, where the children are fields or fields of components attached to game objects. 
	/// </summary>
	private sealed class Node
	{
		//Public, but can be accessed inside Tree only. 
		//Access is necessary to build the tree. Nothing gained by using a property. 
		public List<Node> Children; //null for leaf.
		
		private readonly Node parent; //null for root.

		public T Value { get; }

		public bool IsLeaf => Children == null;
		public bool IsRoot => parent == null;
		
		public Node(T value, Node parent = null, List<Node> children = null)
		{
			Value = value;
			this.parent = parent;
			Children = children;
		}
		
		/// <summary>
		/// Returns this <see cref="Node"/> and all its children, their children, and so on.
		/// </summary>
		public IEnumerable<Node> SelfAndDescendents()
		{
			var list = new List<Node>();
			AddSelfAndDescendents(list);

			return list;
		}
		
		private void AddSelfAndDescendents(IList<Node> list)
		{
			list.Add(this);
			
			if(Children != null)
			{
				foreach (var node in Children)
				{
					node.AddSelfAndDescendents(list);
				}
			}
		} 

		/// <summary>
		/// Returns this <see cref="Node"/> and its parent, their parent, and so on.
		/// </summary>
		public IEnumerable<T> SelfAndAncestors()
		{
			var current = this;
			var stack = new Stack<Node>();
			
			stack.Push(current);

			while (current.parent != null)
			{
				current = current.parent;
				stack.Push(current);

				if (stack.Count() > 1000)
				{
					break;
				}
			}

			while (stack.Any())
			{
				yield return stack.Pop().Value;
			}
		}

		public override string ToString() => Value.ToString();
	}

	//Null root represents an empty tree.
	private readonly Node root;

	public static readonly Tree<T> Empty = new(null);

	public bool IsEmpty => root == null;

	private Tree(Node root) => this.root = root;

	/// <summary>
	/// Builds a new tree given a start node and a way to get children.
	/// </summary>
	/// <param name="maxDepth">If no value is given or the value is negative, the max depth is infinite.</param>
	/// <remarks>
	/// This method does not check for cycles, and will not terminate if there is a cycle.
	/// </remarks>
	public static Tree<T> New(T start, Func<T, IEnumerable<T>> getChildren, int maxDepth = -1)
	{
		const Node depthSentinel = null;
		var openList = new Queue<Node>();
		var root = new Node(start);
			
		openList.Enqueue(root);
		openList.Enqueue(depthSentinel);

		int depth = 0;
			
		while (openList.Any())
		{
			var nextNode = openList.Dequeue();
				
			if (nextNode == depthSentinel)
			{
				depth++;
					
				if (maxDepth >= 0 && depth > maxDepth)
				{
					return new Tree<T>(root);
				}
					
				openList.Enqueue(depthSentinel);
			}
			else
			{
				var value = nextNode.Value;
				nextNode.Children = getChildren(value).Select(child => new Node(child, nextNode)).ToList();
			}
		}

		return new Tree<T>(root);
	}
	
	public IEnumerable<T> TraverseBreadthFirst()
	{
		var queue = new System.Collections.Generic.Queue<Node>();
		
		return Traverse(
			queue,
			(list, x) => list.Enqueue(x),
			list => list.Dequeue(),
			list => list.Any());
	}
	
	public IEnumerable<T> TraverseDepthFirst()
	{
		var stack = new System.Collections.Generic.Stack<Node>();
		
		return Traverse(
			stack,
			(list, x) => list.Push(x),
			list => list.Pop(),
			list => list.Any());
	}

	private IEnumerable<T> Traverse<TList>(
		TList list, 
		Action<TList, Node> push, 
		Func<TList, Node> pop,
		Func<TList, bool> any)
	{
		if (IsEmpty) yield break;
		
		push(list, root);

		while (any(list))
		{
			var node = pop(list);
			yield return node.Value;
			
			if(node.IsLeaf) continue;

			foreach (var child in node.Children)
			{
				push(list, child);
			}
		}
	}
}
