using System;
using System.Collections.Generic;
using System.Linq;
using DataStructures;
using DataStructures.Tiling;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Tests.Tiling;

[Parallelizable]
public class TestPagedTiler
{
	[DatapointSource, UsedImplicitly] 
	private static readonly (int width, ulong[] data, Int2 empty)[] TestEmptyData = 
	{
		(5, new ulong[] { 0b101, 0 }, new Int2(1, 0)),
		(5, new ulong[] { 0b11111, 0b101 }, new Int2(1, 1)),
		(69, new ulong[]{ ulong.MaxValue, 0b101, 0, 0 }, new Int2(65, 0)),
		(69, new ulong[]{ ulong.MaxValue, 0b11111, ulong.MaxValue,  0b101}, new Int2(65, 1))
	};

	[DatapointSource, UsedImplicitly] 
	private static readonly (int width, ulong[] left, ulong[] right, bool areEqual)[] TestComparerData = 
	{
		(5, new ulong[] { 0b101, 0b01 }, new ulong[] { 0b101, 0b01 }, true),
		(65, new ulong[] {ulong.MaxValue, 0b101}, new ulong[] {ulong.MaxValue, 0b101}, true),
		(65, new ulong[] {ulong.MaxValue, 0b101, ulong.MaxValue, 0b101}, new ulong[] {ulong.MaxValue, 0b101, ulong.MaxValue, 0b101}, true),
		
		(5, new ulong[] { 0b101, 0b01 }, new ulong[] { 0b101, 0b11 }, false),
		(65, new ulong[] {ulong.MaxValue, 0b101, ulong.MaxValue, 0b101}, new ulong[] {ulong.MaxValue, 0b101, ulong.MaxValue, 0b111}, false),
		
		//Extra bits are not ignored
		(5, new ulong[] { 0b101, 0b01 }, new ulong[] { 0b101, 0b01000111 }, false),
		(65, new ulong[] {ulong.MaxValue, 0b101, ulong.MaxValue, 0b101}, new ulong[] {ulong.MaxValue, 0b10100111, ulong.MaxValue, 0b101}, false),
	};

	private const ulong OneAt0And62 = (1ul << 62);

	[DatapointSource, UsedImplicitly] 
	private static readonly (int width, ulong[] end, string tileName, Int2 position, bool canPlace)[] TestCanPlaceData =
	{
		(5, new ulong[]{0b11, 0b1}, ".*/**", new Int2(2, 0), true),
		(5, new ulong[]{0b11, 0b1}, ".*/**", new Int2(1, 1), true),
		(5, new ulong[]{0b11, 0b1}, ".*/**", new Int2(1, 2), true),
		(5, new ulong[]{0b11, 0b1}, ".*/**", new Int2(2, 0), true),
		
		(5, new ulong[]{0b11, 0b1}, ".*/**", new Int2(0, 2), false),
		(5, new ulong[]{0b11, 0b1}, ".*/**", new Int2(0, 0), false),
		(5, new ulong[]{0b11, 0b1}, ".*/**", new Int2(0, 1), false),
		(5, new ulong[]{0b11, 0b1}, ".*/**", new Int2(1, 0), false),
		
		(5, new ulong[]{0b11, 0b1}, ".*/**", new Int2(-5, 0), false),
		(5, new ulong[]{0b11, 0b1}, ".*/**", new Int2(5, 0), false),
		(5, new ulong[]{0b11, 0b1}, "....*/....*/*****", new Int2(2, 0), false),
		(5, new ulong[]{0b11, 0b1}, "....*/....*/*****", new Int2(4, 0), true),
		
		//Note 0b100 represents a one at the end of the tile...
		(69, new ulong[] {OneAt0And62 , 0b100}, "***", new Int2(62, 0), false),
		(69, new ulong[] {OneAt0And62, 0b100}, "***", new Int2(63, 0), true),
		(69, new ulong[] {OneAt0And62, 0b100}, "***", new Int2(64, 0), false),
		
		(69, new ulong[] {OneAt0And62, 0}, "******", new Int2(63, 0), true),
		(69, new ulong[] {OneAt0And62, 0}, "*******", new Int2(63, 0), false)
	};

	[DatapointSource, UsedImplicitly] 
	private readonly (string tileName, int width, bool canTile)[] CanTileTestData =
	{
		(".*/***", 4, true),
		(".*/***", 7, false),
		(".*/****", 10, true),
		(".*/****", 9, true),
		//(".*/****", 16, false),
		//(".*/****", 65, true),
	};

	[Test]
	public void TestReflection1()
	{
		ulong[] data = new ulong[] { 0b1011};
		int width = 4;

		var context = new PagedTiler.Context(width);

		var reverseData = PagedTiler.StripEnd.GetReflectedData(data, 0, 1, context);
		
		string str = Convert.ToString((long) reverseData[0], 2);
		
		Assert.That(reverseData[0], Is.EqualTo(0b1101));
	}
	
	[Test]
	public void TestReflection2()
	{
		ulong[] data = new ulong[] {0, 0b1011};
		int width = 4;

		var context = new PagedTiler.Context(width);

		var reverseData = PagedTiler.StripEnd.GetReflectedData(data, 0, 2, context);
		
		string str = Convert.ToString((long) reverseData[1], 2);
		
		Assert.That(reverseData[1], Is.EqualTo(0b1101));
	}
	
	[Test]
	public void TestReflection3()
	{
		ulong[] data = new ulong[] { 0b1011};
		int width = 6;
		var context = new PagedTiler.Context(width);
		var reverseData = PagedTiler.StripEnd.GetReflectedData(data, 0, 1, context);
		string str = Convert.ToString((long) reverseData[0], 2);
		Assert.That(reverseData[0], Is.EqualTo(0b110100));
	}
	
	[Test]
	public void TestReflection4()
	{
		ulong[] data = new ulong[] { 0b1011, 0 };
		int width = 70;

		var context = new PagedTiler.Context(width);

		var reverseData = PagedTiler.StripEnd.GetReflectedData(data, 0, 1, context);

		string str = Convert.ToString((long) reverseData[0], 2);
		
		Assert.That(reverseData[1], Is.EqualTo(0b110100));
	}
	
	[Test]
	public void TestReflection5()
	{
		ulong[] data = new ulong[] { 0, 0, 0b1011, 0 };
		int width = 70;

		var context = new PagedTiler.Context(width);

		var reverseData = PagedTiler.StripEnd.GetReflectedData(data, 0, 2, context);

		string str = Convert.ToString((long) reverseData[0], 2);
		
		Assert.That(reverseData[3], Is.EqualTo(0b110100));
	}
	
	[Theory]
	public void TestFindEmpty((int width, ulong[] data, Int2 empty) testData)
	{
		var context = new PagedTiler.Context(testData.width);
		var end = new PagedTiler.StripEnd(context, testData.data);
		var empty = end.FindEmpty(context);
		
		Assert.That(empty, Is.EqualTo(testData.empty));
	}

	[Theory]
	public void TestComparer((int width, ulong[] left, ulong[] right, bool areEqual) testData)
	{
		var context = new PagedTiler.Context(testData.width);
		var leftEnd = new PagedTiler.StripEnd(context, testData.left);
		var rightEnd = new PagedTiler.StripEnd(context, testData.right);
		var comparer = (PagedTiler.StripEnd.Comparer) PagedTiler.StripEnd.GetComparer();
		bool areEqual = comparer.Equals(leftEnd, rightEnd);
		
		Assert.That(areEqual, Is.EqualTo(testData.areEqual));
	}

	[Theory]
	public void TestCanPlace((int width, ulong[] end, string tileName, Int2 position, bool canPlace) testData)
	{
		var context = new PagedTiler.Context(testData.width);
		var end = new PagedTiler.StripEnd(context, testData.end);
		var points = TileUtils.NameToPoints(testData.tileName);
		var normalizePoints = points.Normalize();
		var tile = new UlongTile(normalizePoints);
		bool canPlace = end.CanPlace(context, testData.position, tile);
		
		Assert.That(canPlace, Is.EqualTo(testData.canPlace));
	}
	
	[Theory]
	public void TestCanTile((string tileName, int width, bool canTile) testData)
	{
		var tiles = TileUtils
			.NameToPoints(testData.tileName)
			.GetAllSymmetriesNormalized()
			.Select(points => points.ToUlongTile());
		
		var res = PagedTiler.TileRect(tiles, testData.width, CannotRuleOutAnything);
		
		bool canTile = res != null;
	
		Assert.That(canTile, Is.EqualTo(testData.canTile));
	}

	private bool CannotRuleOutAnything(PagedTiler.Context context, PagedTiler.StripEnd potentialEnd) => false;
	
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
		
		var tileAsInt2 = TileUtils.NameToPoints("******/*/*/**");

		var tiles = new List<IEnumerable<Int2>>()
			{
				tileAsInt2
			}
			.Select(t => new UlongTile(t))
			.ToArray();

		var tile = tiles[0];
		
		Place(new Int2(62, 0), ref tile, pageExponent, data, width, length, pageCount, usedPageCount);
	}

	private void Place(Int2 position, ref UlongTile tile, int pageExponent, ulong[] data, int width,
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
		var tile = TileUtils.NameToPoints("**/*");

		var tiles = new List<IEnumerable<Int2>>()
			{
				tile
			}
			.Select(t => new UlongTile(t))
			.ToArray();

		var context = new PagedTiler.Context(5, (_, _) => false);
		var end = new PagedTiler.StripEnd();
		
		var empty1 = end.FindEmpty(context);
		Assert.That(empty1, Is.EqualTo(Int2.Zero));
		
		bool isEmpty = end.CanPlace(context, Int2.Zero, tiles[0]);
		
		Assert.That(isEmpty, Is.EqualTo(true));

		

		end = (PagedTiler.StripEnd)end.Place(context, Int2.Zero, tiles[0]);
		
		isEmpty = end.CanPlace(context, Int2.Zero, tiles[0]);
		Assert.That(isEmpty, Is.EqualTo(false));
		
		end = (PagedTiler.StripEnd)end.Place(context, new Int2(2, 0), tiles[0]);
		
		isEmpty = end.CanPlace(context, new Int2(2, 0), tiles[0]);
		Assert.That(isEmpty, Is.EqualTo(false));

		var empty = end.FindEmpty(context);
		
		Assert.That(empty, Is.EqualTo(new Int2(4, 0)));
	}
}
