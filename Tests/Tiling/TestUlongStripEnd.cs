using System.Collections.Generic;
using System.Linq;
using DataStructures;
using DataStructures.Tiling;
using NUnit.Framework;

namespace Tests.Tiling;

public class TestUlongStripEnd
{
	[Test]
	public void TestPlace()
	{
		var tile = TileUtils.NameToPoly("*/**");

		var tiles = new List<IEnumerable<Int2>>()
		{
			tile
		}
			.Select(t => new ULongTile(t))
			.ToArray();

		Tiler.Width = 5;
		var end = new Tiler.UlongStripEnd();
		bool isEmpty = end.CanPlace(Int2.Zero, tiles[0]);
		
		Assert.That(isEmpty, Is.EqualTo(true));

		end = (Tiler.UlongStripEnd)end.Place(Int2.Zero, tiles[0]);
		
		isEmpty = end.CanPlace(Int2.Zero, tiles[0]);
		Assert.That(isEmpty, Is.EqualTo(false));
		
		end = (Tiler.UlongStripEnd)end.Place(new (2, 0), tiles[0]);
		
		isEmpty = end.CanPlace(new (2, 0), tiles[0]);
		Assert.That(isEmpty, Is.EqualTo(false));

		var empty = end.FindEmpty();
		
		Assert.That(empty, Is.EqualTo(new Int2(4, 0)));

	}
}
