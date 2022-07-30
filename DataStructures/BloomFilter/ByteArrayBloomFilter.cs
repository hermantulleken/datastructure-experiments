using System;
using System.Linq;
using DataStructures.Hash;

namespace DataStructures.BloomFilter;

public partial class BloomFilter
{
	//Use the factory methods in BloomFilter to get instances of this class. 
	private sealed class ByteArrayBloomFilter : IBloomFilter<byte[]>
	{
		private readonly bool[] elements;
		private Murmur3HashFunction[] hashFunctions;

		//This is usually denoted by m in literature
		private int storageCount;
		
		//This is usually denoted by k in the literature
		private int hashFunctionCount;
		
		public double ProbabilityOfFalsePositives
			=> Math.Pow(1 - Math.Exp(-hashFunctions.Length * ApproximateCount / elements.Length), hashFunctions.Length);

		//Usually denoted ny n* (approximately n) in literature.
		public double ApproximateCount
			=> -elements.Length / (double)hashFunctions.Length * Math.Log(1 - BitCount / (double)elements.Length);

		private int BitCount => elements.Count(b => b);

		public ByteArrayBloomFilter(int storageCount, int hashFunctionCount)
		{
			this.storageCount = storageCount;
			this.hashFunctionCount = hashFunctionCount;
			
			elements = new bool[storageCount];
			InitializeHashFunctions(hashFunctionCount);
		}

		private void InitializeHashFunctions(int hashFunctionCount)
		{
			hashFunctions = new Murmur3HashFunction[hashFunctionCount];
			for (uint i = 0; i < hashFunctionCount; i++)
			{
				hashFunctions[i] = new Murmur3HashFunction(i);
			}
		}

		public BloomResult Contains(byte[] item)
		{
			for (int i = 0; i < hashFunctions.Length; i++)
			{
				int bitPosition = GetHash(item, i);

				if (elements[bitPosition] == false)
				{
					return BloomResult.No;
				}
			}

			return BloomResult.Maybe;
		}

		public void Add(byte[] item)
		{
			for (int i = 0; i < hashFunctions.Length; i++)
			{
				int bitPosition = GetHash(item, i);
				elements[bitPosition] = true;
			}
		}

		public void Clear()
		{
			for (int i = 0; i < elements.Length; i++)
			{
				elements[i] = false;
			}
		}
		
		private int GetHash(byte[] item, int k)
		{
			(ulong hash, _) = hashFunctions[k].ComputeHash(item);

			return (int)(hash % (uint)elements.Length);
		}
	}
}
