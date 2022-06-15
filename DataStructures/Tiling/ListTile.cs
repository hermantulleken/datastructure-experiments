using System.Collections.Generic;

namespace DataStructures.Tiling;

public class ListTile : ITile
{
	public IEnumerable<Int2> Cells
	{
		get; private set;
	}

	public static ITile New(IEnumerable<Int2> cells) => new ListTile(cells);

	public ListTile(IEnumerable<Int2> cells)
	{
		Cells = cells;
	}
}