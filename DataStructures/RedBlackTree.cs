using System;
using System.Diagnostics;

namespace DataStructures;

public enum Child
{
	Left, 
	Right
}

public class BinaryTree<T> where T : IComparable
{
	public class Node
	{
		public T Data;
		
		private Node left;
		private Node right;
		public Node Parent;
		
		public Node this[Child direction]
		{
			get => direction == Child.Left ? left : right;
			
			set
			{
				if (direction == Child.Left)
				{
					left = value;
				}
				else
				{
					right = value;
				}
			}
		}

		public bool IsRoot => Parent == null;
		
		//Cannot be called on root
		public bool IsLeftChild => this == Parent[Child.Left];
		public bool IsRightChild => this == Parent[Child.Right];
		
		public void Insert(Node node)
		{
			void InsertNode(Child child)
			{
				if (this[child] == null)
				{
					this[child] = node;
				}
				else
				{
					this[child].Insert(node);
				}
			}

			InsertNode(Data.CompareTo(node.Data) < 0 ? Child.Left : Child.Right);
		}
		
		//To get a left rotate, call with Child.Left then Child.Right, 
		//To get a right rotate, call with Child.Right then Child.Left
		public void Rotate(BinaryTree<T> tree, Child left, Child right)
		{
			var other = this[right];

			/* Turn other's left sub-tree into x's right sub-tree */
			this[right] = other[left];
			if (other[left] != null)
			{
				other[left].Parent = this;
			}

			/* other's new Parent was this's Parent */
			other.Parent = Parent;

			/* Set the Parent to point to other instead of this */
			if (IsRoot)
			{
				tree.root = other;
			}
			else if (IsLeftChild)
			{
				Parent[left] = other;
			}
			else
			{
				GLDebug.Assert(IsRightChild);
				Parent[right] = other;
			}

			/* Finally, put this on other's left */
			other[left] = this;
			Parent = other;
		}
	}

	public Node root = null;

	public Node Insert(T item)
	{
		var newNode = new Node { Data = item };
		
		if (root == null)
		{
			root = newNode;
		}

		else
		{
			root.Insert(newNode);
		}

		return newNode;
	}
}

public class RedBlackTree<T>
{
	private enum Color
	{
		Red, Black
	}

	//private Node root;

	private readonly BinaryTree<(Color color, T data)> tree = new();
	
	private static void SetColor(BinaryTree<(Color color, T data)>.Node node, Color color) => node.Data = (color, node.Data.data);

	public void Insert(T data)
	{
		/* Insert in the tree in the usual way */
		var newNode = tree.Insert((Color.Red, data));
		
		void Rebalance(Child left, Child right)
		{
			/* If newNode's Parent is a left, y is newNode's right 'uncle' */
			var rightUncle = newNode.Parent.Parent[right];

			if (rightUncle.Data.color == Color.Red)
			{
				/* case 1 - change the colors */
				SetColor(newNode.Parent, Color.Black);
				SetColor(newNode.Parent.Parent, Color.Red);
				SetColor(rightUncle, Color.Black);
				/* Move newNode up the tree */
				newNode = newNode.Parent.Parent;
			}
			else
			{
				/* y is a black node */
				if (newNode == newNode.Parent[right])
				{
					/* and newNode is to the right */
					/* case 2 - move newNode up and rotate */
					newNode = newNode.Parent;
					newNode.Rotate(tree, left, right);
				}

				/* case 3 */
				SetColor(newNode.Parent, Color.Black);
				SetColor(newNode.Parent.Parent, Color.Red);
				newNode.Parent.Parent.Rotate(tree, right, left);
			}
		}

		/* Now restore the red-black property */
		SetColor(newNode, Color.Red);
		
		while ( (newNode != tree.root) && (newNode.Parent.Data.color == Color.Red) ) 
		{
			if (newNode.Parent.IsLeftChild)
			{
				Rebalance(Child.Left, Child.Right);
			}
			else 
			{
				Rebalance(Child.Right, Child.Left);
			}
		}
		/* color the root black */
		SetColor(tree.root, Color.Black);
	}
}
