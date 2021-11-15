using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures
{
	/*
		The code is based on these blog posts:
			https://ericlippert.com/2019/01/22/an-interesting-list-structure/
			https://ericlippert.com/2019/01/24/an-interesting-list-structure-part-2/
			
		The Hughes list is original described in: 
			A novel representation of lists and its application to the function "reverse" 
		by RJM Hughes, available here: 
			https://www.cs.tufts.edu/~nr/cs257/archive/john-hughes/lists.pdf
	*/
	
	/// <summary>
	/// A data structure that represents an immutable FILO-list of items. 
	/// </summary>
	/// <typeparam name="T">The type of the items in the list.</typeparam>
	/// <remarks>
	/// Use <see cref="SimpleList{T}.SimpleListEmpty"/>, <see cref="SimpleList{T}.HughesListEmpty"/>, or
	/// <see cref="SimpleList{T}.CountableListEmpty"/>> to get instances of lists.  
	/// </remarks>
	public interface ISimpleList<T> : IEnumerable<T>
	{
		public bool IsEmpty { get; }
		public T Peek { get; }
		public int Count { get; }
		public ISimpleList<T> Pop(out T head);
		public ISimpleList<T> Concat(in ISimpleList<T> other);
		public string ToUnbracketedString();
		public ISimpleList<T> Push(T element);
		public ISimpleList<T> Append(T element);
		public ISimpleList<T> Reverse();
	}
	
	public static class SimpleListExtensions
	{
		public static int Count<T>(this ISimpleList<T> list)
		{
			return list.IsEmpty ? 0 : 1 + list.Pop().Count();
		}

		public static ISimpleList<T> Pop<T>(this ISimpleList<T> list)
		{
			return list.Pop(out _);
		}

		/// <summary>
		/// Push items from a <see cref="IEnumerable"/> one by one to a list. 
		/// </summary>
		// This method is mostly useful for testing
		public static ISimpleList<T> Push<T>(this ISimpleList<T> list, IEnumerable<T> itemsToPush) =>
			itemsToPush.Aggregate(list, (current, item) => current.Push(item));
		
		public static ISimpleList<T> Push<T>(this ISimpleList<T> list, params T[] itemsToPush) =>
			list.Push((IEnumerable<T>) itemsToPush);
		
		public static ISimpleList<T> Append<T>(this ISimpleList<T> list, IEnumerable<T> itemsToAppend) =>
			itemsToAppend.Aggregate(list, (current, item) => current.Append(item));
		
		public static ISimpleList<T> Append<T>(this ISimpleList<T> list, params T[] itemsToAppend) =>
			list.Append((IEnumerable<T>) itemsToAppend);

		public static ISimpleList<T> ToSimpleList<T>(this IEnumerable<T> list) => SimpleList<T>.SimpleListEmpty.Push(list);
		public static ISimpleList<T> ToHughesList<T>(this IEnumerable<T> list) => SimpleList<T>.HughesListEmpty.Push(list);

		public static ISimpleList<T> ToCountableList<T>(this IEnumerable<T> list) =>
			SimpleList<T>.CountableListEmpty.Push(list);

		public static bool Equals<T>(this ISimpleList<T> @this, ISimpleList<T> other)
		{
			var node1 = @this;
			var node2 = other;

			while (!node1.IsEmpty)
			{
				if (node2.IsEmpty)
				{
					return false; //other is shorter than @this 
				}

				node1 = node1.Pop(out var val1);
				node2 = node2.Pop(out var val2);

				if (!val1.Equals(val2))
				{
					return false;
				}
			}

			return node2.IsEmpty; 
		}
	}
	
	public static class SimpleList<T>
	{
		public static readonly ISimpleList<T> SimpleListEmpty = new EmptyList();
		public static ref readonly ISimpleList<T> HughesListEmpty => ref HughesList.Empty;
		public static ISimpleList<T> CountableListEmpty => SimpleListWithCheapCount.Empty;

		private abstract class AbstractSimpleList : ISimpleList<T>
		{
			public abstract bool IsEmpty { get; }
			public abstract T Peek { get; }
			public abstract int Count { get; }
			public abstract ISimpleList<T> Pop(out T head);
			public abstract ISimpleList<T> Concat(in ISimpleList<T> other);
			public abstract string ToUnbracketedString();
			public ISimpleList<T> Push(T element) => new NonEmptyList(this, element);
			public ISimpleList<T> Append(T element) => Concat(SimpleListEmpty.Push(element));
			public abstract IEnumerator<T> GetEnumerator();
			public override string ToString() => $"[{ToUnbracketedString()}]";
			public abstract ISimpleList<T> Reverse();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		private sealed class EmptyList : AbstractSimpleList
		{
			private const string CannotPerformOperationOnEmpty = "Cannot perform the operation on a empty list.";
			public override bool IsEmpty => true;
			public override T Peek => throw new InvalidOperationException(CannotPerformOperationOnEmpty);
			public override int Count => 0;
			public override ISimpleList<T> Pop(out T _) => throw new InvalidOperationException(CannotPerformOperationOnEmpty);
			public override ISimpleList<T> Concat(in ISimpleList<T> other) => other;
			public override string ToUnbracketedString() => "";

			public override IEnumerator<T> GetEnumerator()
			{
				yield break;
			}

			public override ISimpleList<T> Reverse() => this;
		}

		private sealed class NonEmptyList : AbstractSimpleList
		{
			private readonly T head;
			private readonly ISimpleList<T> tail;

			public NonEmptyList(ISimpleList<T> tail, T head)
			{
				this.tail = tail ?? throw new ArgumentNullException(nameof(tail));
				this.head = head;
			}

			public override bool IsEmpty => false;
			public override T Peek => head;

			public override int Count
			{
				get
				{
					int count = 1;
					var next = tail;

					while (!next.IsEmpty )
					{
						count++;
						next = next.Pop();
					}

					return count;
				}
			}

			public override ISimpleList<T> Pop(out T oldHead)
			{
				oldHead = head;
				return tail;
			}

			public override ISimpleList<T> Concat(in ISimpleList<T> other)
			{
				var nextNode = Reverse();
				var result = other;
				
				while (!nextNode.IsEmpty)
				{
					result = result.Push(nextNode.Peek);
					nextNode = nextNode.Pop();
				}

				return result;
			}

			public override ISimpleList<T> Reverse()
			{
				var res = SimpleListEmpty.Push(head);
				var next = tail;

				while (!next.IsEmpty)
				{
					res = res.Push(next.Peek);
					next = next.Pop();
				}

				return res;
			}

			public override string ToUnbracketedString()
			{
				var str = head.ToString();
				var nextNode = tail;

				while (!nextNode.IsEmpty)
				{
					str += ", " + nextNode.Peek;
					nextNode = nextNode.Pop();
				}

				return str;

			}
			
			public override IEnumerator<T> GetEnumerator()
			{
				yield return head;

				foreach (var val in tail)
				{
					yield return val;
				}
			}
			
			#region Inefficient reference methods
			//These recursive implementations overflows the stack with 10,000 elements, possibly less
			
			private string ToUnbracketedString_recursive() => tail.IsEmpty ? head.ToString() : $"{head}, {tail.ToUnbracketedString()}";
			private ISimpleList<T> Concat_recursive(ISimpleList<T> other) => other.IsEmpty ? this : this.Pop().Concat(other).Push(head);
			
			#endregion 
		}
		
		/// <summary>
		/// A data structure that represents an immutable list of items, and provides constant time push and concatenation,
		/// and linear time peek and pop. 
		/// </summary>
		private readonly struct HughesList : ISimpleList<T>, IEquatable<HughesList>
		{
			private static readonly Func<ISimpleList<T>, ISimpleList<T>> Identity = x => x;
		
			/// <summary>
			/// The empty list.
			/// </summary>
			public static readonly ISimpleList<T> Empty = Make(Identity);
			
			private readonly Func<ISimpleList<T>, ISimpleList<T>> concatToThis;
			
			private HughesList(Func<ISimpleList<T>, ISimpleList<T>> f) => concatToThis = f;
			
			public T Peek => ToSimple().Peek;
			public int Count => ToSimple().Count;
			public bool IsEmpty => concatToThis == Identity;
			public ISimpleList<T> Pop(out T oldHead) => FromSimple(ToSimple().Pop(out oldHead));
			public string ToUnbracketedString() => ToSimple().ToUnbracketedString();
			public ISimpleList<T> Push(T element) => Make(list => list.Push(element), concatToThis);
			public ISimpleList<T> Append(T element) => Make(concatToThis, list => list.Push(element));
			public ISimpleList<T> Reverse() => Make(ReverseImpl, concatToThis);
			public ISimpleList<T> Concat(in ISimpleList<T> list) => Make(concatToThis, ((HughesList)list).concatToThis);  
			public override string ToString() => ToSimple().ToString();
			
			public IEnumerator<T> GetEnumerator() => ToSimple().GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			
			private static HughesList FromSimple(ISimpleList<T> list) => list.IsEmpty ? (HughesList) Empty : Make(x => list.Concat(x));
			private ISimpleList<T> ToSimple() => concatToThis(SimpleListEmpty);
			private static ISimpleList<T> ReverseImpl(ISimpleList<T> list) => FromSimple(list.Reverse());
			private static HughesList Make(Func<ISimpleList<T>, ISimpleList<T>> f) => new(f);
			
			private static HughesList Make(
				Func<ISimpleList<T>, ISimpleList<T>> f1,
				Func<ISimpleList<T>, ISimpleList<T>> f2) =>
				new(a => f1(f2(a)));

			public bool Equals(HughesList other) => SimpleListExtensions.Equals(this, other);
			public override bool Equals(object obj) => obj is HughesList other && Equals(other);
			public override int GetHashCode() => throw new NotSupportedException();
			public static bool operator ==(HughesList left, HughesList right) => left.Equals(right);
			public static bool operator !=(HughesList left, HughesList right) => !left.Equals(right);
		}
		
		private class SimpleListWithCheapCount : ISimpleList<T>
		{
			private readonly ISimpleList<T> list;
			
			public int Count { get; }

			private SimpleListWithCheapCount(ISimpleList<T> list, int count)
			{
				this.list = list;
				Count = count;
			}

			public static readonly ISimpleList<T> Empty = new SimpleListWithCheapCount(SimpleList<T>.SimpleListEmpty, 0);
			public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			public bool IsEmpty => list.IsEmpty;
			public T Peek => list.Peek;
			public ISimpleList<T> Pop(out T head) => new SimpleListWithCheapCount(list.Pop(out head), Count - 1);

			public ISimpleList<T> Concat(in ISimpleList<T> other)
			{
				if (other is SimpleListWithCheapCount otherCheapList)
				{
					return new SimpleListWithCheapCount(list.Concat(otherCheapList), Count + otherCheapList.Count);
				}

				return new SimpleListWithCheapCount(list.Concat(other), Count + other.Count());
			}

			public string ToUnbracketedString() => list.ToUnbracketedString();
			public ISimpleList<T> Push(T element) => new SimpleListWithCheapCount(list.Push(element), Count + 1);
			public ISimpleList<T> Append(T element) => new SimpleListWithCheapCount(list.Append(element), Count + 1);
			public ISimpleList<T> Reverse() => new SimpleListWithCheapCount(list.Reverse(), Count);
		}
	}
}