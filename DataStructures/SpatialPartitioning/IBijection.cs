namespace DataStructures.SpatialPartitioning;

public interface IBijection<TIn, TOut>
{
	/// <summary>
	/// Maps the given element to the result of applying the bijection. 
	/// </summary>
	public TOut this[TIn element] { get; }
	public IBijection<TOut, TIn> Inverse();
	public IBijection<TIn, T> Compose<T>(IBijection<TOut, T> other);
}
