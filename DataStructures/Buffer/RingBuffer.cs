using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace DataStructures.Buffer;

internal static class Buffer
{
	private const string BufferCantBeEmpty = "Cannot be called on a empty buffer.";
	
	internal static Exception EmptyBufferInvalid() => new InvalidOperationException(BufferCantBeEmpty);
}

public interface IReadonlyBuffer<out T> : IEnumerable<T>
{
	public int Count { get; }
	public int Capacity { get; }
	public T First { get; }
	public T Last { get; }
}

public interface IBuffer<T> : IReadonlyBuffer<T>
{
	public void Insert(T item);
	public void Clear();
}

public sealed class FullCapacity2Buffer<T> : IBuffer<T>
{
	private T item1;
	private T item2;
	private bool firstIsItem1;

	public int Count { get; private set; }
	public int Capacity => 2;
	public T First => (Count == 0) ? throw Buffer.EmptyBufferInvalid() : FirstUnsafe;
	public T Last => (Count == 0) ? throw Buffer.EmptyBufferInvalid() : LastUnsafe;
	
	private T FirstUnsafe => firstIsItem1 ? item1 : item2;
	private T LastUnsafe => firstIsItem1 ? item2 : item1;
	
	public FullCapacity2Buffer() => Clear();

	public IEnumerator<T> GetEnumerator()
	{
		if(Count > 0) yield return FirstUnsafe;
		if(Count > 1) yield return LastUnsafe;
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public void Insert(T item)
	{
		if (firstIsItem1)
		{
			item1 = item;
		}
		else
		{
			item2 = item;
		}

		if (Count < 2)
		{
			Count++;
		}

		firstIsItem1 = !firstIsItem1;
	}

	public void Clear()
	{
		Count = 0;
		item1 = default;
		item2 = default;
		firstIsItem1 = false; //true after first insertion
	}
}

public sealed class ResizeableBuffer<T> : IBuffer<T>
{
	[NotNull] private IBuffer<T> buffer;

	public int Count => buffer.Count;
	public int Capacity => buffer.Capacity;

	public T First => buffer.First;

	public T Last => buffer.Last;

	public ResizeableBuffer(int capacity) => buffer = new RingBuffer<T>(capacity);
	public void Insert(T item) => buffer.Insert(item);

	public void Clear() => buffer.Clear();

	public IEnumerator<T> GetEnumerator() => buffer.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	
	public void Resize(int newCapacity)
	{
		if (newCapacity < 0) throw new ArgumentOutOfRangeException(nameof(newCapacity), "Argument must be positive");

		if (newCapacity == 0)
		{
			buffer = new ZeroCapacityBuffer<T>();
			return;
		}

		var newBuffer = new RingBuffer<T>(newCapacity);
		
		foreach (var item in buffer.Take(newCapacity))
		{
			newBuffer.Insert(item);
		}

		buffer = newBuffer;
	}
}

public sealed class ZeroCapacityBuffer<T> : IBuffer<T>
{
	public int Count => 0;
	public int Capacity => 0;
	public T First => throw Buffer.EmptyBufferInvalid();
	public T Last  => throw Buffer.EmptyBufferInvalid();
	
	/// <summary>
	/// This method has no effect, since the capacity is 0.
	/// </summary>
	/*
		Just like other buffers, it is not illegal to insert items 
		when we are at capacity. So we don't throw an exception; we simply
		do nothing.
	*/
	public void Insert(T item) { } 

	//Nothing to do since there are no elements.
	public void Clear() { }
	
	public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed class FixedBufferWithArrayVanilla<T> : IBuffer<T>
{
	private const string ArgumentMustBePositive = "Argument must be positive.";
	
	
	private int front;
	private int back;

	private readonly T[] items;

	public int Count { get; private set; }

	public int Capacity { get; }
	public T First => throw new NotImplementedException();

	public T Last => throw new NotImplementedException();

	public FixedBufferWithArrayVanilla(int capacity)
	{
		if (capacity <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(capacity), ArgumentMustBePositive);
		}
		
		Capacity = capacity;
		items = new T[capacity];
		ResetPointers();
	}

	public void Insert(T item)
	{
		items[back] = item;

		back++;

		if (back == Capacity)
		{
			back = 0;
		}

		if (Count < Capacity)
		{
			Count++;
		}
		else
		{
			front++;
			if (front == Capacity)
			{
				front = 0;
			}
		}

		AssertCountInvariants();
	}

	public void Clear()
	{
		ReInitialize();
		ResetPointers();
	}

	public IEnumerator<T> GetEnumerator()
	{
		if (front < back)
		{
			for (int i = front; i < back; i++)
			{
				yield return items[i];
			}
		}
		else
		{
			for (int i = front; i < Capacity; i++)
			{
				yield return items[i];
			}
			
			for (int i = 0; i < back; i++)
			{
				yield return items[i];
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/*
		Not strictly necessary for value types. For reference types, this prevents 
		having ghost references to objects and so leak memory. 
	*/
	private void ReInitialize()
	{
		if (front < back)
		{
			for (int i = front; i < back; i++)
			{
				items[i] = default;
			}
		}
		else
		{
			for (int i = front; i < Capacity; i++)
			{
				items[i] = default;
			}
			
			for (int i = 0; i < back; i++)
			{
				items[i] = default;
			}
		}
	}

	public void ResetPointers()
	{
		front = 0;
		back = 0;
		Count = 0;
		
		AssertCountInvariants();
	}

	[AssertionMethod, Conditional(GLDebug.UseAsserts)]
	private void AssertCountInvariants()
	{
		GLDebug.Assert(Count <= Capacity);
		
		if (front < back)
		{
			GLDebug.Assert(Count == back - front);
		}
		else
		{
			GLDebug.Assert(Count == Capacity - front + back);
		}
	}
}

/// <summary>
/// A data structure that can always be added to, but only retains the last n
/// items, where n is the capacity of the <see cref="RingBuffer{T}"/>.
/// </summary>
/// <typeparam name="T">The type of items contain in the <see cref="RingBuffer{T}"/>.</typeparam>
public sealed class RingBuffer<T> : IBuffer<T>
{
	private const string ArgumentMustBePositive = "Argument must be positive.";

	private int front;
	private int back;

	private readonly T[] items;

	public int Count { get; private set; }

	public int Capacity { get; }

	public T First 
		=> Count > 0 
			? this[0] 
			: throw Buffer.EmptyBufferInvalid();
	public T Last 
		=> Count > 0
			? this[Count - 1]
			: throw Buffer.EmptyBufferInvalid();
	
	public T this[int index]
	{
		get
		{
			ValidateIndex(index);
			
			return items[GetRealIndex(index)];
		}

		set
		{
			ValidateIndex(index);

			items[GetRealIndex(index)] = value;
		}
	}

	public RingBuffer(int capacity)
	{
		if (capacity <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(capacity), ArgumentMustBePositive);
		}
		
		Capacity = capacity;
		items = new T[capacity];
		ResetCounters();
	}

	public void Insert(T item)
	{
		items[back] = item;

		back++;

		if (back == Capacity)
		{
			back = 0;
		}

		if (Count < Capacity)
		{
			Count++;
		}
		else
		{
			front++;
			if (front == Capacity)
			{
				front = 0;
			}
		}

		AssertCountInvariants();
	}

	public void Clear()
	{
		ReInitialize();
		ResetCounters();
	}

	public IEnumerator<T> GetEnumerator()
	{
		if (front < back)
		{
			for (int i = front; i < back; i++)
			{
				yield return items[i];
			}
		}
		else
		{
			for (int i = front; i < Capacity; i++)
			{
				yield return items[i];
			}
			
			for (int i = 0; i < back; i++)
			{
				yield return items[i];
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	
	private bool IndexInRange(int index) => 0 <= index && index < Count;

	private int GetRealIndex(int index)
	{
		Debug.Assert(IndexInRange(index));
		
		return (index + front) % Capacity;
	}

	/*
		Not strictly necessary for value types. For reference types, this prevents 
		having ghost references to objects and so leak memory. 
	*/
	private void ReInitialize()
	{
		if (front < back)
		{
			for (int i = front; i < back; i++)
			{
				items[i] = default;
			}
		}
		else
		{
			for (int i = front; i < Capacity; i++)
			{
				items[i] = default;
			}
			
			for (int i = 0; i < back; i++)
			{
				items[i] = default;
			}
		}
	}

	public void ResetCounters()
	{
		front = 0;
		back = 0;
		Count = 0;
	}
	
	[AssertionMethod]
	private void ValidateIndex(int index)
	{
		if (!IndexInRange(index)) throw new ArgumentOutOfRangeException(nameof(index));
	}
	
	[AssertionMethod, Conditional(GLDebug.UseAsserts)]
	private void AssertCountInvariants()
	{
		GLDebug.Assert(Count <= Capacity);
		
		if (front < back)
		{
			GLDebug.Assert(Count == back - front);
		}
		else
		{
			GLDebug.Assert(Count == Capacity - front + back);
		}
	}
}

public sealed class BufferWithQueue<T> : IBuffer<T>
{
	private readonly System.Collections.Generic.Queue<T> queue;

	public int Count => queue.Count;
	public int Capacity { get; }
	public T First => queue.First();
	public T Last => queue.Peek();

	public BufferWithQueue(int capacity)
	{
		Capacity = capacity;
		queue = new System.Collections.Generic.Queue<T>(capacity);
	}

	public void Insert(T item)
	{
		if (queue.Count == Capacity)
		{
			queue.Dequeue();
		}
		
		queue.Enqueue(item);
	}

	public void Clear() => queue.Clear();
	public IEnumerator<T> GetEnumerator() => queue.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed class ChangeDetector<T>
{
	private readonly IBuffer<T> buffer;
	private readonly IEqualityComparer<T> comparer;

	public event Action<T, T> OnValueChanged;
	
	public T Value
	{
		get => buffer.Last;

		set
		{
			buffer.Insert(value);
			
			if (buffer.Count == 2 && !comparer.Equals(Value, PreviousValue))
			{
				OnValueChanged?.Invoke(Value, PreviousValue);
			}
		}
	}
	
	public T PreviousValue => buffer.First;

	public ChangeDetector(IEqualityComparer<T> comparer = null)
	{
		this.comparer = comparer ?? EqualityComparer<T>.Default;
		buffer = new RingBuffer<T>(2);
	}

	public void Clear() => buffer.Clear();
}

public sealed class Differentiator
{
	private readonly IBuffer<float> buffer;

	public float Value
	{
		get => buffer.Last;
		set => buffer.Insert(value);
	}
	
	public float PreviousValue => buffer.First;

	/*
		Technically to be a derivative we need to divide by the time.
		If we assume a constant sample rate, this is a constant, that 
		can be absorbed by the PID filter. 
	*/
	public float Difference =>
		buffer.Count == 2
			? Value - PreviousValue
			: throw new InvalidOperationException("Not enough values set to calculate a derivative");

	public Differentiator() => buffer = new RingBuffer<float>(2);
}

public sealed class Integrator
{
	private readonly IBuffer<float> buffer;

	public float Value
	{
		get => buffer.Last;
		set => buffer.Insert(value);
	}
	
	public float PreviousValue => buffer.First;

	/*
		Technically, we need to scale each interval by the time between samples.
		We assume the sample rate is constant, and that it can be absorbed by the 
		factor in the PID controller.  
	 */
	public float Sum => buffer.Sum();

	public Integrator(int sumWindow) => buffer = new RingBuffer<float>(sumWindow);
}

public sealed class PidController
{
	private readonly Differentiator differentiator;
	private readonly Integrator integrator;

	private readonly float differentiatorFactor;
	private readonly float integrationFactor;
	private readonly float proportionalFactor;

	public float Value
	{
		get => differentiator.Value; //Could also use integrator
		
		set
		{
			differentiator.Value = value;
			integrator.Value = value;
		}
	}

	public float FilteredValue => 
		proportionalFactor * Value 
		+ differentiatorFactor * differentiator.Difference 
		+ integrationFactor * integrator.Sum;

	public PidController(int integrationWindow, float proportionalFactor, float differentiatorFactor, float integrationFactor)
	{
		differentiator = new Differentiator();
		integrator = new Integrator(integrationWindow);

		this.proportionalFactor = proportionalFactor;
		this.differentiatorFactor = differentiatorFactor;
		this.integrationFactor = integrationFactor;
	}
}

public sealed class EnumerableAsBuffer<T> : IReadonlyBuffer<T>
{
	private readonly IEnumerable<T> list;

	private IEnumerable<T> Buffer => list.Take(Capacity);
	
	public EnumerableAsBuffer(IEnumerable<T> list, int capacity)
	{
		this.list = list;
		Capacity = capacity;
	}

	public IEnumerator<T> GetEnumerator() => Buffer.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	public int Count => Buffer.Count();
	public int Capacity { get; }
	public T First => Buffer.First();
	public T Last => Buffer.Last();
}

public sealed class ListAsBuffer<T> : IReadonlyBuffer<T>
{
	private readonly IList<T> list;

	public int Count => Buffer.Count();
	public int Capacity { get; }
	public T First => Buffer.First();
	public T Last => Buffer.Last();
	
	public T this[int index]
	{
		get
		{
			ValidateIndex(index);
			
			return list[GetRealIndex(index)];
		}

		set
		{
			ValidateIndex(index);

			list[GetRealIndex(index)] = value;
		}
	}
	
	private IEnumerable<T> Buffer => list.Take(Capacity);
	private int Front => Math.Max(0, list.Count - Capacity);


	public ListAsBuffer(IList<T> list, int capacity)
	{
		this.list = list;
		Capacity = capacity;
	}

	public IEnumerator<T> GetEnumerator() => Buffer.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	
	private bool IndexInRange(int index) => 0 <= index && index < Count;

	private int GetRealIndex(int index)
	{
		Debug.Assert(IndexInRange(index));
		
		return (index + Front) % Capacity;
	}

	[AssertionMethod]
	private void ValidateIndex(int index)
	{
		if (!IndexInRange(index)) throw new ArgumentOutOfRangeException(nameof(index));
	}
}

public sealed class StatsExample
{
	public static IEnumerable<float> MedianFilter(IEnumerable<float> list)
	{
		float Median(float a, float b, float c) =>
			(a > b) ^ (a > c) 
				? a 
				: (b < a) ^ (b < c) 
					? b 
					: c;


		int windowSize = 3;
		var buffer = new RingBuffer<float>(windowSize);

		foreach (float item in list)
		{
			buffer.Insert(item);

			if (buffer.Count >= windowSize)
			{
				yield return Median(buffer[0], buffer[1], buffer[2]);
			}
		}
	}
}

public sealed class ChangeDetectUsing
{
	private readonly ChangeDetector<int> guess = new();

	public ChangeDetectUsing()
		=> guess.OnValueChanged += (value, previousValue) => Console.WriteLine($"You changed your guess from {previousValue} to {value}");

	public void Run() => guess.Value = AskGuess();

	public int AskGuess() => 0; /* Real implementation omitted */
}

class Example
{
	public static int GetFibonacci(int n)
	{
		if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "Can't be nbegative.");
		
		if (n == 0 || n == 1) return 1;
		
		var buffer = new RingBuffer<int>(2);
		
		buffer.Insert(1);
		buffer.Insert(1);

		for (int i = 0; i < n - 2; i++)
		{
			buffer.Insert(buffer[0] + buffer[1]);
		}

		return buffer.Last;
	}
}
