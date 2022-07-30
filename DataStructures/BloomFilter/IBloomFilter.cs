namespace DataStructures.BloomFilter;

/// <summary>
/// A space-efficient set-like data structure.
/// </summary>
/// <remarks>See "Handbook of Data Structures and Applications" (Mehta and Sahni, 2018) Chapter 10.</remarks>
public interface IBloomFilter<in T>
{
	/// <summary>
	/// Calculates approximately how many (unique) elements have bene added to this filter.
	/// </summary>
	/// <remarks>This function is provided for doing performance tests.</remarks>
	public double ApproximateCount { get; }
	
	/// <summary>
	/// Gives the probability of false positives, that is, the probability that 1
	/// <see cref="BloomResult.Maybe"/> is returned for an element that was never added. 
	/// </summary>
	/// <remarks><p>Among other things, depends on the number of (unique) elements added so far,
	/// which is estimated using<see cref="ApproximateCount"/>.</p>
	/// <p>This function is provided for doing performance tests.</p>
	/// </remarks>
	public double ProbabilityOfFalsePositives { get; }
	
	/// <summary>
	/// Adds a new element to this <see cref="BloomFilter"/>.
	/// </summary>
	/// <param name="item"></param>
	public void Add(T item);
	
	/// <summary>
	/// Checks whether the given item is in the set represented by this Bloom filter. 
	/// </summary>
	/// <param name="item"></param>
	/// <returns><see cref="BloomResult.No"/> if the element is definitely not present, or
	/// <see cref="BloomResult.Maybe"/> if it may be present. See also
	/// <see cref="ProbabilityOfFalsePositives"/></returns>
	public BloomResult Contains(T item);
	
	public void Clear();




}
