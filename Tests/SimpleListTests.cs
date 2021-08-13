using System;
using System.Collections.Generic;
using System.Linq;
using DataStructures;
using NUnit.Framework;

using ListConverter = System.Func<int[], DataStructures.ISimpleList<int>>;
using ListAction = System.Action<DataStructures.ISimpleList<int>>;

namespace Tests
{
	[Parallelizable(ParallelScope.All), TestOf(nameof(SimpleList<int>))]
	public class SimpleListTests
	{
		[Datapoint] private static readonly int[] Empty = Array.Empty<int>();
		[Datapoint] private static readonly int[] TestValues1 = {3, 7, 11};
		[Datapoint] private static readonly int[] TestValues2 = {4, 2, 90, 34, 56};
		
		[DatapointSource] 
		private static readonly ListConverter[] GetListFunctions =
		{
			values => values.ToSimpleList(),
			values => values.ToHughesList(),
			values => values.ToCountableList()
		};

		[DatapointSource] 
		private static readonly ListAction[] InvalidOperations =
		{
			list => list.Pop(),
			list => {var _ = list.Peek; },
		};

		[Theory]
		public void PushParams(ListConverter toList)
		{
			const int a = 1;
			const int b = 2;
			const int c = 3;

			var list1 = toList(Empty).Push(a, b, c);

			var list2 = toList(Empty)
				.Push(a)
				.Push(b)
				.Push(c);

			Assert.That(list1, Is.EqualTo(list2));
		}

		[Theory]
		public void IsEmpty(ListConverter toList, int[] values)
		{
			var list = toList(values); 
			
			Assert.That(list.IsEmpty, Is.EqualTo(list.Count == 0));
		}
		
		[Theory]
		public void Count(ListConverter toList, int[] values)
		{
			var list = toList(values);
			
			Assert.That(list.Count, Is.EqualTo(values.Length));
		}

		[Theory]
		public void PopPeekInvariant(ListConverter toList, int[] values)
		{
			var list = values.ToSimpleList();

			while (!list.IsEmpty)
			{
				var peeked = list.Peek;
				list = list.Pop(out var popped);
				Assert.That(peeked, Is.EqualTo(popped));
			}
		}

		[Theory]
		public void PushPeekInvariant(ListConverter toList, int[] values)
		{
			var list = toList(Empty);

			foreach (var testValue in values)
			{
				list = list.Push(testValue);
				
				Assert.That(list.Peek, Is.EqualTo(testValue));
			}
		}

		[Theory]
		public void Concat(ListConverter toList, int[] values1, int[] values2)
		{
			var list1 = values1.ToSimpleList();
			var list2 = values2.ToSimpleList();
			var combinedList = list2.Concat(list1);
			
			Assert.That(combinedList, Is.EqualTo(values1.Concat(values2).Reverse()));
		}

		[Theory]
		public void Reverse(ListConverter toList, int[] values)
		{
			var list = toList(values);
			
			Assert.That(list.Reverse(), Is.EqualTo(values));
		}

		[Theory]
		public void AppendPeekInvariant(ListConverter toList, int[] values)
		{
			var list = SimpleList<int>.SimpleListEmpty;

			foreach (var testValue in values)
			{
				list = list.Append(testValue);
				
				Assert.That(list.Peek, Is.EqualTo(values[0]));
			}
		}

		[Theory]
		public void AppendReverseInvariant(ListConverter toList, int[] values)
		{
			var list1 = SimpleList<int>.SimpleListEmpty.Push(values).Reverse();
			var list2 = SimpleList<int>.SimpleListEmpty.Append(values);

			Assert.That(list1, Is.EqualTo(list2));
		}

		[Theory]
		public void UnbracketedString(ListConverter toList, int[] values)
		{
			var stringPresentation = ToString(values.Reverse());
			var list = values.ToSimpleList();
			
			Assert.That(list.ToUnbracketedString(),  Is.EqualTo(stringPresentation));
		}

		[Theory]
		public void ToString(ListConverter toList, int[] values)
		{
			var stringPresentation = $"[{ToString(values.Reverse())}]";
			var list = values.ToSimpleList();
			
			Assert.That(list.ToString(), Is.EqualTo(stringPresentation));
		}

		[Theory]
		public void InvalidOperationsOnEmpty(ListConverter toList, ListAction operation)
		{
			Assert.That( () => operation(toList(Empty)), Throws.TypeOf<InvalidOperationException>() );
		}

		[Theory]
		public void TestEquals(int[] values)
		{
			var list1 = SimpleList<int>.HughesListEmpty.Push(values);
			var list2 = SimpleList<int>.HughesListEmpty.Push(values);

			Assert.That(list1.Equals(list2), Is.True);
		}
		
		[Test]
		public void TestNotEquals()
		{
			var list1 = SimpleList<int>.HughesListEmpty.Push(TestValues1);
			var list2 = SimpleList<int>.HughesListEmpty.Push(TestValues2);

			Assert.That(list1.Equals(list2), Is.False);
		}

		private static string ToString<T>(IEnumerable<T> list) => 
			string.Join(", ", list.Select(val => val.ToString()));
	}
}