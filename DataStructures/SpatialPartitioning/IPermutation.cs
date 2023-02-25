namespace DataStructures.SpatialPartitioning;

/// <summary>
/// Represents a bijection on Z_n (integers from 0 up to n - 1).
/// </summary>
/// <remarks>
/// <seealso href="https://en.wikipedia.org/wiki/Permutation"/>
/// </remarks>
public interface IPermutation : IBijection<int, int>
{
	/// <value>Always positive.</value>
	/*
		We exclude the possibility of permutations defined on the empty set. 
		Although we may come up with a consistent notion, it is very hard to
		reason about (for example, does this permutation have an inverse? if 
		it is itself, is it therefor the identity function?). Besides the 
		mathematical difficulties, the code is also simpler to write if we simply 
		exclude this possibility. 
	*/
	public int SetCount { get; }
	
	///<summary>Returns the inverse permutation of this permutation.</summary>
	///<remarks>Composing a permutation with its inverse will yield the identity, and is the only permutation to do so.</remarks>
	public new IPermutation Inverse();
	
	/// <summary>
	/// Returns the composition of this permutation with <paramref name="other"/>.
	/// </summary>
	/// <remarks>If <see langword="this"/> maps x to y and <paramref name="other"/> maps y to z, their composition maps x to z</remarks>
	public IPermutation Compose(IPermutation other);
	
	/// <summary>
	/// Returns true of two permutations are equivalent. 
	/// </summary>
	/// <remarks>Permutations are equivalent if they are defined on the same sets (i.e. their values of <see cref="SetCount"/> are equal),
	/// and they map each element of the set to the same value.</remarks>
	public bool Equivalent(IPermutation other);
}
