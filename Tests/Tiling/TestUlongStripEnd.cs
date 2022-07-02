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
		var tile = TileUtils.NameToPoints("**/*");

		var tiles = new List<IEnumerable<Int2>>()
		{
			tile
		}
			.Select(t => new UlongTile(t))
			.ToArray();

		var context = new Tiler.Context(5, (_, _) => false);
		
		var end = new Tiler.UlongStripEnd();
		bool isEmpty = end.CanPlace(context, Int2.Zero, tiles[0]);
		
		Assert.That(isEmpty, Is.EqualTo(true));

		end = (Tiler.UlongStripEnd)end.Place(context, Int2.Zero, tiles[0]);
		
		isEmpty = end.CanPlace(context, Int2.Zero, tiles[0]);
		Assert.That(isEmpty, Is.EqualTo(false));
		
		end = (Tiler.UlongStripEnd)end.Place(context, new Int2(2, 0), tiles[0]);
		
		isEmpty = end.CanPlace(context, new Int2(2, 0), tiles[0]);
		Assert.That(isEmpty, Is.EqualTo(false));

		var empty = end.FindEmpty(context);
		
		Assert.That(empty, Is.EqualTo(new Int2(4, 0)));

	}
}
