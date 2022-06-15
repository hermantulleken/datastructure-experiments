using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions;

namespace DataStructures.Tiling;

/// <summary>
/// A set of these can represent a tiling (to be proper there should be no overlap).
/// </summary>
public struct PositionedTile<TTile> where TTile : ITile
{
	public TTile Tile;
	public Int2 Position;

	public IEnumerable<Int2> Points
	{
		get
		{
			var position = Position;
			return Tile.Cells.Select(point => point + position);
		}
	}

	public override string ToString() => Points.ToPrettyString();
}
