using System.Linq;
using DataStructures;
using DataStructures.Tiling;
using NUnit.Framework;

namespace Tests.Tiling;

[TestFixture]
public class TestUlongTile
{
	[Test]
	public void TestSingleCellPerRow()
	{
		var cells = new Int2[]
		{
			new (0, 0),
			new (1, 1),
			new (2, 2),
		};

		var tile = new UlongTile(cells);
		
		Assert.That(tile.Rows.Length, Is.EqualTo(3));
		
		Assert.That(tile.Rows[0], Is.EqualTo(1));
		Assert.That(tile.Rows[1], Is.EqualTo(2));
		Assert.That(tile.Rows[2], Is.EqualTo(4));
	}
	
	[Test]
	public void TestMultipleCellsPerRow()
	{
		var cells = new Int2[]
		{
			new (0, 0),
			new (0, 1),
			new (1, 1),
			new (0, 2),
			new (1, 2),
			new (2, 2),
		};

		var tile = new UlongTile(cells);
		
		Assert.That(tile.Rows.Length, Is.EqualTo(3));
		
		Assert.That(tile.Rows[0], Is.EqualTo(1));
		Assert.That(tile.Rows[1], Is.EqualTo(3));
		Assert.That(tile.Rows[2], Is.EqualTo(7));
	}

	[Test]
	public void TestPositiveOffsets()
	{
		Int2 offset = new(3, 5);
		var cells = new Int2[]
		{
			new (0, 0),
			new (0, 1),
			new (1, 1),
			new (0, 2),
			new (1, 2),
			new (2, 2),
		}.Select(p => p + offset);

		var tile = new UlongTile(cells);
		
		Assert.That(tile.Rows.Length, Is.EqualTo(3));
		Assert.That(tile.XOffset, Is.EqualTo(offset.X));
		Assert.That(tile.YOffset, Is.EqualTo(offset.Y));
		
		Assert.That(tile.Rows[0], Is.EqualTo(1));
		Assert.That(tile.Rows[1], Is.EqualTo(3));
		Assert.That(tile.Rows[2], Is.EqualTo(7));
	}
	
	[Test]
	public void TestNegativeOffsets()
	{
		Int2 offset = new(-3, -5);
		var cells = new Int2[]
		{
			new (0, 0),
			new (0, 1),
			new (1, 1),
			new (0, 2),
			new (1, 2),
			new (2, 2),
		}.Select(p => p + offset);

		var tile = new UlongTile(cells);
		
		Assert.That(tile.Rows.Length, Is.EqualTo(3));
		Assert.That(tile.XOffset, Is.EqualTo(offset.X));
		Assert.That(tile.YOffset, Is.EqualTo(offset.Y));
		
		Assert.That(tile.Rows[0], Is.EqualTo(1));
		Assert.That(tile.Rows[1], Is.EqualTo(3));
		Assert.That(tile.Rows[2], Is.EqualTo(7));
	}
}

class TestUlongTileImpl : TestUlongTile
{
}
