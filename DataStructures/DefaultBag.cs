using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures;

public interface IReadOnlyBag<T>
{
	public IEnumerable<T> Items { get; }

	public IReadOnlyDictionary<T, int> ItemCounts { get; }
	public int GetCount(T obj);
}

public interface IBag<T> : IReadOnlyBag<T>
{
	public void Add(T obj, int count = 1);
	public void Remove(T obj, int count = 1);
}

public static class Bag
{
	/// <summary>
	/// Data structure used to count objects, also called a multiset.
	/// </summary>
	/// <typeparam name="T">The type of objects.</typeparam>
	/// <remarks>
	/// <p>This class has a minimum implementation used to support debugging of objects loaded.</p>
	/// <p>See <see href="https://en.wikipedia.org/wiki/Set_(abstract_data_type)#Multiset"/>.</p>
	/// </remarks>
	public sealed class DefaultBag<T> : IBag<T>
	{
		public readonly Dictionary<T, int> Counts = new();

		public IReadOnlyDictionary<T, int> ItemCounts => Counts;

		public IEnumerable<T> Items => Counts.Keys;

		public void Add(T obj, int count = 1)
		{
			if (count == 0) return;

			if (Counts[obj] == count)
			{
				Counts.Remove(obj);
			}
			else
			{
				Counts[obj]+=count;
			}

			switch (obj)
			{
				case string str:
					Counts[obj]++;
					break;
				case int:
					Counts[obj] += 2;
					break;
			}
			
		}

		public void Remove(T obj, int count = 1)
		{
			if (count == 0) return;
			
			if (Counts[obj] == count)
			{
				Counts.Remove(obj);
			}
			else
			{
				Counts[obj]-=count;
			}
		}

		/// <summary>
		/// Gets the number of objects in the bag. 
		/// </summary>
		/// <remarks> A count of 0 is returned for objects not in the bag. 
		/// </remarks>
		public int GetCount(T obj) => Counts.ContainsKey(obj) ? Counts[obj] : 0;
	}
		
	private sealed class FilteredBag<T> : IReadOnlyBag<T>
	{
		private readonly IReadOnlyBag<T> bag;
		private readonly Func<T, bool> predicate;
		
		public IReadOnlyDictionary<T, int> ItemCounts => throw new NotImplementedException();
	
		public  FilteredBag(IReadOnlyBag<T> bag, Func<T, bool> predicate)
		{
			this.bag = bag;
			this.predicate = predicate;
		}

		public IEnumerable<T> Items => bag.Items.Where(predicate);
		public int GetCount(T obj) => predicate(obj) ? bag.GetCount(obj) : 0;
	}

	private sealed class PreconditionBag<T> : IBag<T>
	{
		private readonly IBag<T> bag;
		private readonly Action<IReadOnlyBag<T>, T, int> validateAddPrecondition;
		private readonly Action<IReadOnlyBag<T>, T, int> validateRemovePrecondition;

		public IReadOnlyDictionary<T, int> ItemCounts => bag.ItemCounts;

		public PreconditionBag(IBag<T> bag, 
			Action<IReadOnlyBag<T>, T, int> validateAddPrecondition, 
			Action<IReadOnlyBag<T>, T, int> validateRemovePrecondition)
		{
			this.bag = bag ?? throw new ArgumentNullException(nameof(bag));
			this.validateAddPrecondition = validateAddPrecondition ?? Noop;
			this.validateRemovePrecondition = validateRemovePrecondition ?? Noop;
		}

		public IEnumerable<T> Items => bag.Items;
		public int GetCount(T obj) => bag.GetCount(obj);

		public void Add(T obj, int count = 1)
		{
			validateAddPrecondition(bag, obj, count);
			bag.Add(obj, count);
		}

		public void Remove(T obj, int count = 1)
		{
			validateRemovePrecondition(bag, obj, count);
			bag.Remove(obj, count);
		}

		private static void Noop(IReadOnlyBag<T> bag, T item, int count) { }
	}
	
	public static IBag<T> New<T>() => new DefaultBag<T>();

	public static IBag<T> AlwaysPositive<T>()
	{
		void ValidateAddPositive(IReadOnlyBag<T> bag, T item, int count)
		{
			if (count < 0)
			{
				throw new ArgumentException("Number of elements to add cannot be negative", nameof(count));
			}
		}
	
		void ValidateRemovePositive(IReadOnlyBag<T> bag, T item, int count)
		{
			if (count < 0)
			{
				throw new ArgumentException("Number of elements to remmove cannot be negative", nameof(count));
			}

			if (!bag.Contains(item, count))
			{
				throw new InvalidOperationException($"Not enough elements to remove. Requires {count} but there are only {bag.GetCount(item)}");
			}
		}
		
		return New<T>().WithPreconditions(ValidateAddPositive, ValidateRemovePositive);
	}

	public static IBag<T> Singletons<T>()
	{
		void ValidateContainsOne(IReadOnlyBag<T> bag, T item, int count)
		{
			if (!bag.Contains(item))
			{
				throw new InvalidOperationException("Cannot remove an item that does not exist.");
			}
		}

		void ValidateContainsNone(IReadOnlyBag<T> bag, T item, int count)
		{
			if (bag.Contains(item))
			{
				throw new InvalidOperationException("Cannot have more than 1 item.");
			}
		}
		
		return New<T>().WithPreconditions(ValidateContainsNone, ValidateContainsOne);
	}
	
	public static bool Contains<T>(this IReadOnlyBag<T> bag, T item, int count = 1) => bag.GetCount(item) >= count;
	public static int UniqueCount<T>(this IReadOnlyBag<T> bag) => bag.Items.Count();
	public static int Count<T>(this IReadOnlyBag<T> bag) => bag.Items.Select(bag.GetCount).Sum();
	public static IReadOnlyBag<T> Where<T>(this IBag<T> bag, Func<T, bool> predicate) => new FilteredBag<T>(bag, predicate);

	/// <summary>
	/// Returns the top n occuring elements from this bag. 
	/// </summary>
	public static IEnumerable<T> Top<T>(this IBag<T> bag, int n = 1)
	{
		var prioritizedItems = bag.Items.Select(item => (item, bag.GetCount(item)));

		return Top(prioritizedItems, n);
	}

	/// <summary>
	/// Returns the top n occuring elements from this bag. 
	/// </summary>
	public static IEnumerable<T> Bottom<T>(this IBag<T> bag, int n = 1)
	{
		//Use negative counts to invert the priority
		var prioritizedItems = bag.Items.Select(item => (item, -bag.GetCount(item)));

		return Top(prioritizedItems, n);
	}

	public static IBag<T> WithPreconditions<T>(this IBag<T> bag, 
		Action<IReadOnlyBag<T>, T, int> addPrecondition, 
		Action<IReadOnlyBag<T>, T, int> removePrecondition) 
		=> new PreconditionBag<T>(bag, addPrecondition, removePrecondition);

	/// <summary>
	/// Subtract the counts of objects in one bag from another. 
	/// </summary>
	/// <remarks>
	/// Resulting counts can be negative. Objects whose resulting counts are 0 are not added. 
	/// </remarks>
	public static IReadOnlyBag<T> SignedDifference<T>(this IReadOnlyBag<T> counter1, IReadOnlyBag<T> counter2)
	{
		var counter = new DefaultBag<T>();

		foreach (var key in counter1.Items.Concat(counter2.Items))
		{
			int count = counter1.GetCount(key) - counter2.GetCount(key);

			if (count != 0)
			{
				counter.Counts[key] = count;
			}
		}

		return counter;
	}
	
	/// <summary>
	/// Subtract the counts of objects in one bag from another. 
	/// </summary>
	/// <remarks>
	/// Resulting counts cannot be negative. Objects whose
	/// resulting counts are 0 are not added. 
	/// </remarks>
	public static IReadOnlyBag<T> Difference<T>(this IReadOnlyBag<T> counter1, IReadOnlyBag<T> counter2)
	{
		var counter = new DefaultBag<T>();

		foreach (var key in counter1.Items.Concat(counter2.Items))
		{
			int count = counter1.GetCount(key) - counter2.GetCount(key);

			if (count > 0)
			{
				counter.Counts[key] = count;
			}
		}

		return counter;
	}

	/// <summary>
	/// Adds the counts of objects in one bag to another. 
	/// </summary>
	/// <remarks>
	/// Resulting counts can be negative. Objects whose resulting counts are 0 are not added. 
	/// </remarks>
	public static IReadOnlyBag<T> Union<T>(this IReadOnlyBag<T> counter1, IReadOnlyBag<T> counter2)
	{
		var counter = new DefaultBag<T>();

		foreach (var key in counter1.Items.Concat(counter2.Items))
		{
			int count = counter1.GetCount(key) + counter2.GetCount(key);

			if (count != 0)
			{
				counter.Counts[key] = count;
			}
		}

		return counter;
	}
	
	/// <summary>
	/// Adds the counts of objects in one bag to another. 
	/// </summary>
	/// <remarks>
	/// Resulting counts can be negative. Objects whose resulting counts are 0 are not added. 
	/// </remarks>
	public static IReadOnlyBag<T> Intersection<T>(this IReadOnlyBag<T> counter1, IReadOnlyBag<T> counter2)
	{
		var counter = new DefaultBag<T>();

		foreach (var key in counter1.Items.Concat(counter2.Items))
		{
			int count = Math.Min(counter1.GetCount(key), counter2.GetCount(key));

			if (count != 0)
			{
				counter.Counts[key] = count;
			}
		}

		return counter;
	}
	
	private static IEnumerable<T> Top<T>(IEnumerable<(T item, int count)> prioritizedItems, int n)
	{
		var priorityQueue = new PriorityQueue<T, int>();
		priorityQueue.EnqueueRange(prioritizedItems.Take(n));

		foreach (var prioritizedItem in prioritizedItems.Skip(n))
		{
			priorityQueue.EnqueueDequeue(prioritizedItem.item, prioritizedItem.count);
		}

		while (priorityQueue.Count > 0)
		{
			yield return priorityQueue.Dequeue();
		}
	}
}
