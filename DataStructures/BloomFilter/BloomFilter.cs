using System;

namespace DataStructures.BloomFilter;
	
/// <summary>
/// Provides methods for creating Bloom filters. See <see cref="IBloomFilter{T}"/>.
/// </summary>
public static partial class BloomFilter
{
	public static IBloomFilter<byte[]> ForByteArray(int storageCount, int hashFunctionCount)
		=> new ByteArrayBloomFilter(storageCount, hashFunctionCount);
	
	public static IBloomFilter<T> ForT<T>(int storageCount, int hashFunctionCount, Func<T, byte[]> toByteArray)
		=> new GenericBloomFilter<T>(storageCount, hashFunctionCount, toByteArray);
	
	public static IBloomFilter<int> ForInt(int storageCount, int hashFunctionCount)
		=> new GenericBloomFilter<int>(storageCount, hashFunctionCount, BitConverter.GetBytes);
	
	public static IBloomFilter<long> ForLong(int storageCount, int hashFunctionCount)
		=> new GenericBloomFilter<long>(storageCount, hashFunctionCount, BitConverter.GetBytes);
}
