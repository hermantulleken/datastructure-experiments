namespace DataStructures.Tiling;

public interface IStripEnd
{
	bool IsStraight { get; }
	Int2 FindEmpty();
}

public interface IStripEnd<in TTile> : IStripEnd
{
	bool CanPlace(Int2 position, TTile tile);
	IStripEnd Place(Int2 position, TTile longTile);
	
#pragma warning disable CA2252
	static abstract IStripEnd<TTile> New(int width);
	static abstract object GetComparer();
#pragma warning restore CA2252
}
