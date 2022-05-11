using System;
using System.Collections.Generic;

namespace DataStructures;

public interface IPriorityQueue<T>
{
	void Insert(T item);
	T Peek();
	T Pop();
}

//Useful for very small queues
public class PriorityQueueWithFixedUnsortedArray<T> : IPriorityQueue<T>
{
	private readonly IComparer<T> comparer;

	private readonly T[] array;
	private int emptyIndex = 0;

	public PriorityQueueWithFixedUnsortedArray(IComparer<T> comparer, int capacity)
	{
		this.comparer = comparer;
		array = new T[capacity];
	}

	public bool HasCapacity() => emptyIndex < array.Length;

	public bool Empty() => emptyIndex == 0;

	public void Insert(T item)
	{
		if (!HasCapacity()) throw new Exception("Data structure full."); 

		array[emptyIndex] = item;
		emptyIndex++;
	}

	public T Peek() => array[GetMinIndex()];

	public T Pop()
	{
		int minIndex = GetMinIndex();
		var item = array[minIndex];
		emptyIndex--;
		array[minIndex] = array[emptyIndex];

		return item;
	}

	private int GetMinIndex()
	{
		if (Empty()) throw new Exception("Cannot use operation on empty queue.");
		
		int minIndex = 0;
		var minItem = array[minIndex];
		
		for (int i = 0; i < emptyIndex; i++)
		{
			var item = array[i];

			if (comparer.Compare(item, minItem) < 0)
			{
				minItem = item;
				minIndex = i;
			}
		}

		return minIndex;
	}
}
