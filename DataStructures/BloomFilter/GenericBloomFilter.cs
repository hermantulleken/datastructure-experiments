using System;

namespace DataStructures.BloomFilter;

public partial class BloomFilter
{
	//Use the factory methods in BloomFilter to get instances of this class. 
	private sealed class GenericBloomFilter<T> : IBloomFilter<T>
	{
		private readonly Func<T, byte[]> toByteArray;
		private readonly ByteArrayBloomFilter bloomFilter;

		public GenericBloomFilter(int storageCount, int hashFunctionCount, Func<T, byte[]> toByteArray)
		{
			this.toByteArray = toByteArray;
			bloomFilter = new ByteArrayBloomFilter(storageCount, hashFunctionCount);
		}

		public double ApproximateCount => bloomFilter.ApproximateCount;
		public double ProbabilityOfFalsePositives => bloomFilter.ProbabilityOfFalsePositives;

		public void Add(T item)
			=> bloomFilter.Add(toByteArray(item));

		public BloomResult Contains(T item)
			=> bloomFilter.Contains(toByteArray(item));

		public void Clear()
			=> bloomFilter.Clear();
	}
}
