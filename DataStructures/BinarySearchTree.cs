using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gamelogic.Extensions;
using JetBrains.Annotations;


namespace DataStructures
{
	// This tree does not support elements with the same key, and since it
	// it requires integer keys, it is not general purpose. 
	public sealed class BinarySearchTree<T> : IDictionary<int, T>
	{
		private enum Direction
		{
			Left,
			Right
		}
		
		private sealed class Choice
		{
			private bool toggle;

			public Direction Value
			{
				get
				{
					toggle = !toggle;
					return toggle ? Direction.Left: Direction.Right;
				}
			}
		}

		private interface INode
		{
			bool IsEmpty { get; }
			INode Find(int key);
			int Count();
			int Depth { get; }
			string ToRepresentation();
			int EmptyCount { get; }
		}
		
		private sealed class EmptyNode : INode
		{
			public bool IsEmpty => true;
			public INode Find(int key) => null;
			public int Count() => 0;
			public int Depth => 0;
			public string ToRepresentation() => "[]";
			public int EmptyCount => 1;
			public override string ToString() => ToRepresentation();
		}

		private abstract class NonEmptyNode : INode
		{
			public int Key { get; protected set; }
			public T Value { get; private set; }
			public bool IsEmpty => false;
			public abstract INode Find(int key);
			public abstract int Count();
			public abstract int Depth { get; }
			public abstract string ToRepresentation();
			public abstract int EmptyCount { get; }
			public override string ToString() => ToRepresentation();

			protected NonEmptyNode(int key, T value)
			{
				Key = key;
				Value = value;
			}
		}

		//A node no children
		private sealed class Singleton : NonEmptyNode
		{
			public override INode Find(int key) => this.Key == key ? this : null;
			public Singleton(int key, T value) : base(key, value) { }
			public override int Count() => 1;
			public override int Depth => 1;
			public override string ToRepresentation() => $"[{Key}]";
			public override int EmptyCount => 0;
		}
		
		//A node with at least one child
		private sealed class Parent : NonEmptyNode
		{
			[NotNull] public INode Left;
			[NotNull] public INode Right;
			[NotNull] private readonly Choice choice;
			
			public override string ToRepresentation() => $"[{Left.ToRepresentation()} / {Key} \\ {Right.ToRepresentation()}]";
			public override int Depth => 1 + Math.Max(Left.Depth, Right.Depth);
			public override int EmptyCount => Left.EmptyCount + Right.EmptyCount;

			public Parent(int key, T value, Choice choice) : base(key, value)
			{
				Left = Empty;
				Right = Empty;
				this.choice = choice ?? throw new ArgumentNullException(nameof(choice));
			}

			public override INode Find(int keyToFind)
			{
				if (Key == keyToFind)
				{
					return this;
				}

				if(keyToFind < Key)
				{
					return Left.Find(keyToFind);
				}

				GLDebug.Assert(keyToFind > Key);
				
				return Right.Find(keyToFind);
			}

			public override int Count() => 1 + Left.Count() + Right.Count();

			private void Insert(INode node)
			{
				if (node is NonEmptyNode nonEmptyNode)
				{
					Insert(nonEmptyNode);
				}
			}
			
			public void Insert(NonEmptyNode subtree)
			{
				if (subtree.Key < Key)
				{
					InsertAt(ref Left, subtree);

					return;
				}
				
				GLDebug.Assert(subtree.Key >= Key);
				InsertAt(ref Right, subtree);
			}

			private void InsertAt(ref INode insertLocation, NonEmptyNode subtree)
			{
				switch (insertLocation)
				{
					case EmptyNode:
						insertLocation = subtree;
						break;
					
					case Singleton singleton:
						var newNode = new Parent(singleton.Key, singleton.Value, choice);
						newNode.Insert(subtree);
						insertLocation = newNode;
						break;
					
					case Parent parent:
						parent.Insert(subtree);
						break;
					
					default:
						throw new InvalidOperationException();
				}
			}

			public void Remove(int keyToFind)
			{
				void RemoveFrom(ref INode subtree)
				{
					switch (subtree)
					{
						case EmptyNode:
							throw new InvalidOperationException();

						case Singleton singleton:
							if (singleton.Key == keyToFind)
							{
								subtree = Empty;
							}
							else
							{
								throw new InvalidOperationException();
							}

							break;

						case Parent parent:
							parent.Remove(keyToFind);
							break;
					}
				}

				void RemoveThisNode()
				{
					//These assertions are true because non-empty nodes with no children should be singletons
					if (Left.IsEmpty)
					{
						GLDebug.Assert(!Right.IsEmpty);
						InsertSubtreeHere(Right);
					}
					else if (Right.IsEmpty)
					{
						GLDebug.Assert(!Left.IsEmpty);
						InsertSubtreeHere(Left);
					}
					else
					{
						GLDebug.Assert(!Right.IsEmpty);
						GLDebug.Assert(!Left.IsEmpty);

						InsertSubtreeHere(choice.Value == Direction.Left ? Left : Right);
					}
				}
				
				void InsertSubtreeHere(INode subtree)
				{
					var nonEmptyNode = (NonEmptyNode) subtree;

					Key = nonEmptyNode.Key;

					if (nonEmptyNode is Parent parent)
					{
						Insert(parent.Left);
						Insert(parent.Right);
					}
				}

				if (keyToFind == Key)
				{
					RemoveThisNode();
				}
				else
				{
					if (keyToFind < Key)
					{
						RemoveFrom(ref Left);
					}
					else
					{
						RemoveFrom(ref Right);
					}
				}
			}
		}
		
		private static readonly INode Empty = new EmptyNode(); 
		private INode root;
		private readonly Choice choice;

		public bool IsEmpty => root == null;
		public int Depth => root.Depth;
		public int EmptyCount => root.EmptyCount;
		
		public bool TryGetValue(int key, out T value)
		{
			var node = Find(key);

			if (node != null)
			{
				var nonEmptyNode = (NonEmptyNode) node;
				value = nonEmptyNode.Value;
				
				return true;
			}

			value = default;
			return false;
		}

		public T this[int key]
		{
			get => ((NonEmptyNode) Find(key)).Value;
			set => Insert(key, value);
		}

		public ICollection<int> Keys => this.Select(pair => pair.Key).ToList();
		public ICollection<T> Values => this.Select(pair => pair.Value).ToList();
		public void Clear() => root = Empty;
		public int Count => root.Count();
		public bool IsReadOnly => false;
		
		public BinarySearchTree()
		{
			root = Empty;
			choice = new Choice();
		}

		public void Add(int key, T value) => this[key] = value;
		public void Add(KeyValuePair<int, T> item) => this[item.Key] = item.Value; 
		public bool ContainsKey(int key) => Find(key) != null;

		public bool Remove(int key)
		{
			switch(root)
			{
				case EmptyNode : 
					throw new InvalidOperationException();
				
				case Singleton singleton :
					if (singleton.Key != key)
					{
						throw new InvalidOperationException();
					}
					
					root = Empty;

					break;
				case Parent parent:
					parent.Remove(key);
					break;
			}

			return true;
		}

		public bool Contains(KeyValuePair<int, T> item)
		{
			var node = Find(item.Key);

			return node != null && Equals(((NonEmptyNode) node).Value, item);
		}

		public void CopyTo(KeyValuePair<int, T>[] array, int arrayIndex)
		{
			array.ThrowIfNull(nameof(array));
			int currentIndex = arrayIndex;
			
			foreach (var pair in this)
			{
				array[currentIndex] = pair;
				currentIndex++;
			}
		}

		public bool Remove(KeyValuePair<int, T> item)
		{
			(int key, _) = item;
			var node = Find(key);

			if (node == null)
			{
				return false;
			}

			var nonEmptyNode = (NonEmptyNode) node;

			if (Equals(nonEmptyNode.Value))
			{
				Remove(key);
				return true;
			}

			return false;
		}

		public string ToRepresentation() => root.ToRepresentation();
		
		public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
		{
			using var enumerator = GetNodeEnumerator();
			
			while (enumerator.MoveNext())
			{
				var node = enumerator.Current;

				if (node is NonEmptyNode nonEmptyNode)
				{
					yield return new KeyValuePair<int, T>(nonEmptyNode.Key, nonEmptyNode.Value);
				}
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		
		private IEnumerator<INode> GetNodeEnumerator()
		{
			if (IsEmpty)
			{
				yield break;
			}

			var stack = new System.Collections.Generic.Stack<INode>();
			stack.Push(root);

			while (stack.Any())
			{
				var currentNode = stack.Pop();
				
				switch(currentNode)
				{
					case EmptyNode :
						break;
					
					case Singleton singleton : yield return singleton;
						break;
					
					case Parent parent:
						stack.Push(parent.Left);
						stack.Push(parent.Right);
						yield return parent;

						break;
				}
			}
		}
		
		private INode Find(int key) => root.Find(key);
		private void Insert(int key, T value) => Insert(new Singleton(key, value));
		
		private void Insert(NonEmptyNode subtree)
		{
			switch (root)
			{
				case EmptyNode:
					root = subtree;
					break;
				
				case Singleton singleton:
					var newRoot = new Parent(singleton.Key, singleton.Value, choice);
					newRoot.Insert(subtree);
					root = newRoot;
					break;
				
				case Parent parent:
					parent.Insert(subtree);
					break;
				
				default:
					throw new InvalidOperationException();
			}
		}
	}

	public sealed class RandomIterator<T>
	{
		private readonly IList<T> list;
		private readonly Random random;
		
		private bool isIterating;

		public RandomIterator(IList<T> list, Random random = null)
		{
			this.list = list;
			this.random = random ?? new Random();
			isIterating = false;
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			if (isIterating)
			{
				throw new InvalidOperationException("Cannot iterate more than once over this collection at the same time.");
			}

			isIterating = true;
			
			for(int i = 0; i < list.Count - 1; i++)
			{
				int j = random.Next(i, list.Count);
				Algorithms.SwapAt(list, i, j);
				yield return list[i];
			}
				
			yield return list[^1];
			isIterating = false;
		}
	}

	public sealed class RandomList : IEnumerable<int>
	{
		private static readonly Random Seeder = new();
		
		private readonly int seed;
		private readonly int range;
		
		public static RandomList New(int range) => new RandomList(range, Seeder.Next());

		private RandomList(int range, int seed)
		{
			this.range = range;
			this.seed = seed;
		}


		public IEnumerator<int> GetEnumerator()
		{
			var rand = new Random(seed);

			while (true)
			{
				yield return rand.Next(range);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	
	public static class SortDirect
	{
		public static void SelectionSort<T>(IList<T> list) where T:IComparable<T>
		{
			/* advance the position through the entire array */
			/*   (could do i < aLength-1 because single element is also min element) */
			for (int i = 0; i < list.Count-1; i++)
			{
				/* find the min element in the unsorted a[i .. aLength-1] */

				/* assume the min is the first element */
				int jMin = i;
			
				/* test against elements after i to find the smallest */
				for (int j = i+1; j < list.Count; j++)
				{
					/* if this element is less, then it is the new minimum */
					if (list[j].CompareTo(list[jMin]) < 0)
					{
						/* found new minimum; remember its index */
						jMin = j;
					}
				}

				if (jMin != i) 
				{
					SwapAt(list, i, jMin);
				}
			}
		}

		private static void SwapAt<T>(IList<T> list, int a, int b)
		{
			T tmp = list[a];
			list[a] = list[b];
			list[b] = tmp;
		}
	}

	public struct ListWindow<T>
	{
		public int Start;
		public int Length;
		public IList<T> List;
		
		public bool Empty => Length == 0;
		
		public T this[int logicalIndex]
		{
			get => List[ToPhysicalIndex(logicalIndex)];
			set => List[ToPhysicalIndex(logicalIndex)] = value; 
		}

		private int ToPhysicalIndex(int index) => (Start + index) % Length;
	}
	
	public struct CircularListWindow<T>
	{
		public int PhysicalStart;
		public int Length;
		public int LogicalStart;
		public IList<T> List;

		public bool Empty => Length == 0;
		
		public T this[int logicalIndex]
		{
			get => List[ToPhysicalIndex(logicalIndex)];
			set => List[ToPhysicalIndex(logicalIndex)] = value; 
		}

		private int ToPhysicalIndex(int index) => (LogicalStart + index - PhysicalStart) % Length + PhysicalStart;

		public void ResetRotation()
		{
			var tmp = List[PhysicalStart];
			for (int i = 0; i < Length; i++)
			{
				List[i] = this[i];
				this[i] = tmp;

				if (i + 1 < Length)
				{
					tmp = List[i + 1];
				}
				//otherwise we are in the last interation and don't need the next tmp value
			}
		}
	}

	public sealed class DoublyLinkedList<T> : IEnumerable<T>
	{
		private sealed class Node
		{
			public Node NextNode;
			public Node PreviousNode;
			public T Item;
		}

		private sealed class NodeIterator : IEnumerable<Node>
		{
			private readonly DoublyLinkedList<T> list;
			
			public NodeIterator(DoublyLinkedList<T> list)
			{
				this.list = list;
			}

			public IEnumerator<Node> GetEnumerator()
			{
				if (list.Empty)
				{
					yield break;
				}

				var node = list.firstNode;
				yield return node;
			
				while (node.NextNode != null)
				{
					node = node.NextNode;
					yield return node;
				}
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		private Node firstNode;
		private Node lastNode;

		public bool Empty => firstNode == null;
		public int Count { get; private set; }
		public T First => firstNode.Item is {} item ? item : throw new InvalidOperationException();
		public T Last => lastNode.Item is {} item ? item : throw new InvalidOperationException();

		private NodeIterator Nodes { get; set; }

		public DoublyLinkedList()
		{
			Clear();
			Nodes = new NodeIterator(this);
		}

		public void AddFirst(T item)
		{
			var node = new Node
			{
				NextNode = firstNode,
				PreviousNode = null,
				Item = item
			};

			if (Empty)
			{
				lastNode = node;
				firstNode = node;
			}
			else
			{
				firstNode = node;
				
				if (firstNode.NextNode != null)
				{
					firstNode.NextNode.PreviousNode = node;
				}
			}

			Count++;
		}

		public void AddLast(T item)
		{
			var node = new Node
			{
				NextNode = null,
				PreviousNode = lastNode,
				Item = item
			};

			GLDebug.Assert(lastNode.NextNode == null);
			lastNode.NextNode = node;
			lastNode = node;
			
			Count++;
		}

		public T RemoveFirst()
		{
			if (Empty) throw new InvalidOperationException("Cannot remove from empty list.");

			var item = firstNode.Item;
			firstNode = firstNode.NextNode; //also works if NextNode is null
			firstNode.PreviousNode = null;
			
			Count--;
			
			return item;
		}

		public T RemoveLast()
		{
			if (Empty) throw new InvalidOperationException("Cannot remove from empty list.");

			var item = lastNode.Item;

			if (Count > 1)
			{
				lastNode = lastNode.PreviousNode;
				lastNode.NextNode = null;
			}
			else
			{
				lastNode = firstNode = null;
			}

			Count--;

			return item;
		}
		
		public void Clear()
		{
			firstNode = null;
			lastNode = null;
			Count = 0;
		}

		public void Reverse()
		{
			if (Empty)
			{
				return;
			}

			static void SwapNodeDirection(Node node) => Algorithms.Swap(ref node.PreviousNode, ref node.NextNode);

			foreach (var node in Nodes)
			{
				SwapNodeDirection(node);
			}
			
			Algorithms.Swap(ref firstNode, ref lastNode);
		}

		private void Remove(Node node)
		{
			if (node == firstNode)
			{
				RemoveFirst();
			}
			else if (node == lastNode)
			{
				RemoveLast();
			}
			else
			{
				var previous = node.PreviousNode;
				var next = node.NextNode;
				previous.NextNode = next;
				next.PreviousNode = previous;
				Count--;
			}
		}

		private void InsertBefore(Node node, T item)
		{
			if (node == firstNode)
			{
				AddFirst(item);
			}
			else
			{
				var newNode = new Node
				{
					NextNode = node,
					PreviousNode = node.PreviousNode,
					Item = item
				};

				node.PreviousNode.NextNode = newNode;
				node.PreviousNode = newNode;
				Count++;
			}
		}

		private void InsertAfter(Node node, T item)
		{
			if (node == lastNode)
			{
				AddLast(item);
			}
			else
			{
				var newNode = new Node
				{
					NextNode = node.NextNode,
					PreviousNode = node,
					Item = item
				};
			
				node.NextNode.PreviousNode = newNode;
				node.NextNode = newNode;
				
				Count++;
			}
		}

		public IEnumerator<T> GetEnumerator() => Nodes.Select(node => node.Item).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public sealed class ResizeableArray<T> : IList<T>
	{
		private const int DefaultCapacity = 20;
		private const int NotFoundIndex = -1;
		
		private T[] items;
		
		public int Count { get; private set; }
		public bool IsReadOnly => false;
		public bool Empty => Count == 0;
		public int LastIndex => Count - 1;
		public T Last => items[LastIndex];
		
		public T this[int index]
		{
			get
			{
				ThrowIfOutOfRange(index);
				return items[index];
			}

			set
			{
				ThrowIfOutOfRange(index);
				items[index] = value;
			}
		}
		
		private int Capacity => items.Length;
		
		public ResizeableArray(int capacity = DefaultCapacity)
		{
			items = new T[capacity];
			Count = 0;
		}

		public void Add(T item)
		{
			if (Count == Capacity)
			{
				DoubleCapacity();
			}

			items[Count] = item;
			Count++;
		}
		
		public bool Remove(T item)
		{
			int index = IndexOf(item);

			if (index == NotFoundIndex)
			{
				return false;
			}
			
			RemoveAt(index);
			
			return true;
		}

		public void Clear()
		{
			if(!typeof(T).IsValueType)
			{
				for (int i = 0; i < Count; i++)
				{
					items[i] = default;
				}
			}

			Count = 0;
		}

		public bool Contains(T item) => IndexOf(item) != NotFoundIndex;

		public void CopyTo(T[] array, int arrayIndex)
		{
			array.ThrowIfNull(nameof(array));
			arrayIndex.ThrowIfNegative(nameof(arrayIndex));

			if (Count + arrayIndex > array.Length)
			{
				throw new ArgumentException(nameof(arrayIndex));
			}
			
			for (int i = 0; i < Count; i++)
			{
				array[i + arrayIndex] = this[i];
			}
		}
		
		public int IndexOf(T item)
		{
			for (int i = 0; i < Count; i++)
			{
				if (Equals(item, items[i]))
				{
					return i;
				}
			}

			return NotFoundIndex;
		}

		public void Insert(int index, T item)
		{
			void ShiftRightFromIndex()
			{
				for (int i = Count; i > index; i--)
				{
					items[i] = items[i - 1];
				}
			}

			if (Count == Capacity)
			{
				DoubleCapacity();
			}

			ShiftRightFromIndex();
			items[index] = item;
		}

		public void RemoveAt(int index)
		{
			void ShiftLeftFromIndex()
			{
				for (int i = index; i < Count - 1; i++)
				{
					items[i] = items[i + 1];
				}
			}

			ThrowIfOutOfRange(index);
			ShiftLeftFromIndex();
			items[Count - 1] = default;
			Count--;
		}

		public T DeleteAt(int index)
		{
			var item = items[index];
			RemoveAt(index);
			return item;
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
			{
				yield return items[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		
		private void DoubleCapacity()
		{
			var newItems = new T[2 * Capacity];

			for (int i = 0; i < Count; i++)
			{
				newItems[i] = items[i];
			}

			items = newItems;
		}
		
		private bool Equals(T item1, T item2) => Comparer<T>.Default.Compare(item1, item2) == 0;
		
		[AssertionMethod]
		private void ThrowIfOutOfRange(int index)
		{
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
		}
	}

	public class Bag<T> : IEnumerable<T>
	{
		private readonly DoublyLinkedList<T> list;

		public int Count => list.Count;
		public bool Empty => list.Empty;

		public Bag() => list = new DoublyLinkedList<T>();
		public void Add(T item) => list.AddLast(item);
		public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	
	public sealed class RandomBag<T> : IEnumerable<T>
	{
		private readonly ResizeableArray<T> list;
		private readonly RandomIterator<T> iterator;
		
		public int Count => list.Count;
		public bool Empty => list.Empty;

		public RandomBag()
		{
			list = new ResizeableArray<T>();
			iterator = new RandomIterator<T>(list);
		} 
		
		public void Add(T item) => list.Add(item);

		public IEnumerator<T> GetEnumerator() => iterator.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public sealed class Queue<T> : IEnumerable<T>
	{
		private readonly DoublyLinkedList<T> list;

		public int Count => list.Count;
		public bool Empty => list.Empty;
		public T Peek => list.Last;
		public Queue() => list = new DoublyLinkedList<T>();
		public void Enqueue(T item) => list.AddFirst(item);
		public T Dequeue() => list.RemoveLast();
		public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public sealed class GeneralizedQueue<T> : IEnumerable<T>
	{
		private readonly ResizeableArray<T> list;

		public GeneralizedQueue() => list = new ResizeableArray<T>();
		
		public bool Empty => list.Empty;
		public int Count => list.Count;
		public void Insert(T item) => list.Insert(0, item);
		public void Delete(int fromLast) => list.RemoveAt(Count - fromLast);
		public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	
	public sealed class RandomQueue<T> : IEnumerable<T>
	{
		private readonly ResizeableArray<T> list;
		private readonly Random random;
		private readonly RandomIterator<T> iterator;

		public int Count => list.Count;
		public bool Empty => list.Empty;
		public T Sample => list[RandomIndex];

		private int RandomIndex => random.Next(Count);

		public RandomQueue()
		{
			list = new ResizeableArray<T>();
			random = new Random();
			iterator = new RandomIterator<T>(list, random);
		}
		
		public void Enqueue(T item) => list.Insert(0, item);
		
		public T Dequeue()
		{
			Algorithms.SwapAt(list, RandomIndex, Count - 1);

			return list.DeleteAt(Count - 1);
		}
		
		public IEnumerator<T> GetEnumerator() => iterator.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public sealed class Stack<T> : IEnumerable<T>
	{
		private readonly DoublyLinkedList<T> list;

		public int Count => list.Count;
		public bool Empty => list.Empty;
		public T Peek => list.Last;

		public Stack() => list = new DoublyLinkedList<T>();
		public void Push(T item) => list.AddLast(item);
		public T Pop() => list.RemoveLast();
		public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public sealed class Steque<T> : IEnumerable<T>
	{
		private readonly DoublyLinkedList<T> list;

		public int Count => list.Count;
		public bool Empty => list.Empty;
		public T Peek => list.Last;

		public Steque() => list = new DoublyLinkedList<T>();
		public void Enqueue(T item) => list.AddFirst(item);
		public void Push(T item) => list.AddLast(item);
		public T Pop() => list.RemoveLast();
		public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public sealed class Deque<T> : IEnumerable<T>
	{
		private readonly DoublyLinkedList<T> list;

		public int Count => list.Count;
		public bool Empty => list.Empty;
		public T PeekLeft => list.First;
		public T PeekRight => list.Last;
		
		public Deque() => list = new DoublyLinkedList<T>();
		
		public void PushRight(T item) => list.AddLast(item);
		public T PopRight() => list.RemoveLast();
		public void PushLeft(T item) => list.AddFirst(item);
		public T PopLeft() => list.RemoveFirst();
		public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
