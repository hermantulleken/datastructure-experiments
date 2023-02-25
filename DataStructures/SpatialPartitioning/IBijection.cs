namespace DataStructures.SpatialPartitioning;

/// <summary>
/// Represents a function with an inverse. 
/// </summary>
/// <typeparam name="TIn">The function's input type.</typeparam>
/// <typeparam name="TOut">The function's output type.</typeparam>
public interface IBijection<TIn, TOut>
{
	/// <summary>
	/// Maps the given element to the result of applying the bijection. 
	/// </summary>
	public TOut this[TIn element] { get; }
	
	/// <summary>
	/// Returns the inverse bijection of this bijection. 
	/// </summary>
	/// <remarks>
	/// <para>The implementation may compute the inverse when this method is called,
	/// or return a pre-computed value. User's should cache a copy if they are concerned
	/// about performance instead of calling this method multiple times.
	/// </para>
	/// <para>
	/// Generally the following identity should hold (if floating point operations are
	/// involved, it will only be approximately):
	/// <![CDATA[
	/// x == bijection.Inverse()[x]
	/// ]]>
	/// </para>
	/// </remarks>
	/*
		Why not a property? FuncBijection implements the inverse as a simple 
		field, so exposing that as a property would make sense. However, 
		the calculation of the inverse may be quite slow (as it is for 
		ArrayPermutations, for example), and we don't necessarily 
		want to compute them when we construct the object. Nor do we want to suggest 
		accessing the inverse is a quick operation; therefore, we expose the
		inverse through a method instead. 
		
		Why note called ComputeInverse? Although sometimes computed, it is not always, 
		and may be confusing in those cases. 
	*/
	public IBijection<TOut, TIn> Inverse();
	
	public IBijection<TIn, T> Compose<T>(IBijection<TOut, T> other);
}
