namespace DataStructures.Tiling;

public interface IStripEnd
{
	bool IsStraight { get; }
	
}

public interface IStripEnd<in TTile, in TContext> : IStripEnd
{
	Int2 FindEmpty(TContext context);
	
	bool CanPlace(TContext context, Int2 position, TTile tile);
	IStripEnd Place(TContext context, Int2 position, TTile longTile);
	
#pragma warning disable CA2252
	static abstract IStripEnd<TTile, TContext> New(int width);
	static abstract object GetComparer();
#pragma warning restore CA2252
}
