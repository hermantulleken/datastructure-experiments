using System;
using System.Collections.Generic;

namespace DataStructures.Tiling;

public class DoubleEndedList<T>
{
	
	
	private T[] data;
	private int offset; //where is item 0
	private int rightEmpty;
	private int Capacity => data.Length;
	
	public bool IsReadOnly => false;
	public int Count => rightEmpty - offset;
	private bool FullLeft => offset == 0;
	private bool FullRight => rightEmpty == Capacity - 1;

	public bool CanTakeRight(int count) => rightEmpty + count < Capacity;
	
	
	private bool Any() => offset < rightEmpty;
	
	

	public T this[int index]
	{
		get => data[ValidateInRange(index + offset)];
		set
		{
			if (InRange(index))
			{
				data[index + offset] = value;
				return;
			}

			switch (index)
			{
				case > 0 when offset + index >= Capacity:
					Expand(index + offset - Count);
					break;
				case <= 0 when offset + index < 0:
					Expand(Math.Abs(offset + index));
					break;
			}
			
			data[index + offset] = value;
		}
	}

	private int ValidateInRange(int index) 
		=> InRange(index) ? index : throw new ArgumentOutOfRangeException(nameof(index));
	
	public DoubleEndedList(int capacity)
	{
		data = new T[capacity];
		rightEmpty = offset = capacity/2;
	}

	//Right
	public void Add(T obj, int count)
	{
		if (!CanTakeRight(count))
		{
			Expand(count);
		}

		for (int i = 0; i < count; i++)
		{
			data[rightEmpty] = obj;
			rightEmpty++;
		}
	}
	
	public void AddFront(T obj)
	{
		if (FullLeft)
		{
			Expand(1);
		}

		offset--;
		data[offset] = obj;
	}

	public void Remove()
	{
		if (!Any()) throw new InvalidOperationException();

		rightEmpty--;
		data[rightEmpty] = default;
	}

	public void RemoveFront() => RemoveFront(1);
	
	public void RemoveFront(int count)
	{
		if(Count < count) throw new InvalidOperationException();

		for (int i = 0; i < count; i++)
		{
			data[offset + i] = default;
		}
		
		offset += count;
	}

	public void Clear()
	{
		for (int i = offset; i < rightEmpty; i++)
		{
			data[i] = default;
		}
		
		rightEmpty = offset = Capacity/2;
	}

	private void Expand(int count)
	{
		int newCapacity = 2 * count;

		while (newCapacity < Count + count)
		{
			newCapacity *= 2;
		}
		
		var newData = new T[newCapacity];
		int newOffset = (newCapacity - Capacity) / 2;

		for (int i = 0; i < Count; i++)
		{
			newData[i + newOffset] = data[i + offset];
		}

		data = newData;
		offset = newOffset;
		rightEmpty = offset + Count;
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = offset; i < rightEmpty; i++)
		{
			yield return data[i];
		}
	}

	public bool InRange(int index) => index >= 0 && index < Count;
}