using System;
using System.Collections.Generic;
using System.Linq;
using DataStructures;
using DataStructures.Tiling;
using NUnit.Framework;

namespace Tests.Tiling;

public class TestPagedTiler
{
	[Test]
	public void TestMathWhiteBox()
	{
		
		int pageExponent = 2;
		int pageCount = 1 << pageExponent;
		int width = 70;
		int usedPageCount = (width + 63) / 64; //divides by 64 and gives the ceil
		int length = 5;
		int offset = 0;

		int dataLength = length + offset;

		ulong[] data = new ulong[dataLength * pageCount];
		
		var tileAsInt2 = TileUtils.NameToPoly("******/*/*/**");

		var tiles = new List<IEnumerable<Int2>>()
			{
				tileAsInt2
			}
			.Select(t => new ULongTile(t))
			.ToArray();

		var tile = tiles[0];
		
		Place(new Int2(62, 0), ref tile, pageExponent, data, width, length, pageCount, usedPageCount);
		
		
	}

	private void Place(Int2 position, ref ULongTile tile, int pageExponent, ulong[] data, int width,
		int length, int pageCount, int usedPageCount)
	{
		for (int rowIndex = 0; rowIndex < tile.Rows.Length; rowIndex++)
		{
			ulong row = tile.Rows[rowIndex];
				
			int y = position.Y + rowIndex + tile.YOffset;
			int x = tile.XOffset + position.X;
					
			Assert.That(x, Is.GreaterThanOrEqualTo(0));
					
			int xx = x & 63; //x % 64
			ulong movedRow = row << xx;

			if (movedRow != 0)
			{
				int yy = (y << pageExponent) + (x >> 6); // x >> 6 is x / 64
				data[yy] |= movedRow;
			}
					
			//if row == 0b10000, and xx == 62, then movedRow above will be 0
			//to get the points, we move -2 = 64 - 62, or (64 - xx) to the other side
			
			if (xx != 0) //Otherwies, the expression below becomes row >> 64, which does nothing
			{
				movedRow = row >> (64 - xx);

				if (movedRow != 0)
				{
					int yy = (y << pageExponent) + (x >> 6) + 1; // x >> 6 is x / 64
					data[yy] |= movedRow;
				}
			}

			var str = ToString(data, length, usedPageCount, pageCount, width);
			Console.WriteLine(str);
		}
	}

	public static string ToString(ulong[] data, int length, int usedPageCount, int pageCount, int width)
	{
		string RowToString(ulong row)
		{
			return new string(Convert.ToString((long) row, 2).Reverse().ToArray());
		}
			
		string s = "[";

		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < usedPageCount; j++)
			{
				int rowIndex = i * pageCount + j;
				
				s += RowToString(data[rowIndex]) + ".";
			}

			s += "/";

		}

		return s;
	}

	[Test]
	public void TestPlace()
	{
		var tile = TileUtils.NameToPoly("**/*");

		var tiles = new List<IEnumerable<Int2>>()
			{
				tile
			}
			.Select(t => new ULongTile(t))
			.ToArray();

		Tiler.Width = 5;
		var end = new PagedTiler.PagedUlongStripEnd();
		bool isEmpty = end.CanPlace(Int2.Zero, tiles[0]);
		
		Assert.That(isEmpty, Is.EqualTo(true));

		end = (PagedTiler.PagedUlongStripEnd)end.Place(Int2.Zero, tiles[0]);
		
		isEmpty = end.CanPlace(Int2.Zero, tiles[0]);
		Assert.That(isEmpty, Is.EqualTo(false));
		
		end = (PagedTiler.PagedUlongStripEnd)end.Place(new (2, 0), tiles[0]);
		
		isEmpty = end.CanPlace(new (2, 0), tiles[0]);
		Assert.That(isEmpty, Is.EqualTo(false));

		var empty = end.FindEmpty();
		
		Assert.That(empty, Is.EqualTo(new Int2(4, 0)));
	}
}
