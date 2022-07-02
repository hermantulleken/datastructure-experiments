using DataStructures;
using DataStructures.Tiling;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Tests.Tiling;

[Parallelizable]
public class TestTileUtils
{
	[DatapointSource, UsedImplicitly]
	private static readonly (string name, Int2[] expectedPoints)[] TestNameToPolyData =
	{
		(".*/****", new []{new Int2(1, 0), new Int2(0, 1), new Int2(1, 1), new Int2(2, 1), new Int2(3, 1)})
	};

	[DatapointSource, UsedImplicitly]
	private static readonly (string patternName, int sripEndwidth, ulong[] stripEndData, bool expectedHasPattern)[] TestHasPatternData = 
	{
		("***/*.*", 3, 
			new ulong[]
			{
				0b111,
				0b101,
			}, 
			true),
		
		("***/*.*", 3, 
			new ulong[]
			{
				0b101,
				0
			}, 
			true),
		
		("*.*/*.*", 3, 
			new ulong[]
			{
				0b101,
				0
			}, 
			false),
		
		("***/*.*", 20, 
			new ulong[]
			{
				0b111 << 3,
				0b101 << 3,
			}, 
			true),
		
		("***/*.*", 80, 
			new ulong[]
			{
				0,
				0b111 << 3,
				0,
				0b101 << 3,
			}, 
			true),
		
		("***/*.*", 80, 
			new ulong[]
			{
				0,
				0,
				0,
				0b111 << 3,
				0,
				0b101 << 3,
			}, 
			true),
		
		("***/*.*", 80, 
			new ulong[]
			{
				0,
				0,
				0,
				0b111 << 4,
				0,
				0b101 << 3,
			}, 
			false)
	};
	
	[Theory]
	public void TestNameToPoly((string name, Int2[] expectedPoints) testData)
	{
		var actualPoints = TileUtils.NameToPoints(testData.name);
		
		Assert.That(actualPoints, Is.EquivalentTo(testData.expectedPoints));
	}

	[Theory]
	public void TestHasPattern((string patternName, int sripEndwidth, ulong[] stripEndData, bool expectedHasPattern) testData)
	{
		var points = TileUtils.NameToPoints(testData.patternName);
		var tile = new UlongTile(points);
		var pattern = new Pattern(tile);
		var context = new PagedTiler.Context(testData.sripEndwidth);

		Assert.That(testData.stripEndData.Length % context.PageCount, Is.Zero);

		bool hasPattern = TileUtils.HasPattern(context, new PagedTiler.StripEnd(context, testData.stripEndData), pattern);
		Assert.That(hasPattern, Is.EqualTo(testData.expectedHasPattern));
	}
}
