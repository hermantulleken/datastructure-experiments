using System.Collections.Generic;

namespace DataStructures.Tiling;

public interface ITile
{
	IEnumerable<Int2> Cells { get; }
	
#pragma warning disable CA2252
	static abstract ITile New(IEnumerable<Int2> cells);
#pragma warning restore CA2252
}
