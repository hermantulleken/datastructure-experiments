using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Gamelogic.Extensions;

namespace DataStructures.SpatialPartitioning;



public static class Program
{

	public static void Main()
	{
	}
	
#if TABLE_READY
	public static void Main()
	{
		
		/*
		float range = 100;
		
		var g1 = Generator
			.UniformRandomFloat(0)
			.Select(x => x * 2 * range - range)
			.Group(3)
			;
		var g2 = g1.CloneAndRestart();
		
		Console.WriteLine(g1.Next(3).ToPrettyString());
		Console.WriteLine(g2.Next(3).ToPrettyString());
		*/
		var sampleCount = new[] { 1, 2, 3 };

		var results = Measure(5, sampleCount, new[] { Algorithm.SequentialLookup });

		var t = results.Select(list => list.Select(x => x.comparisonCount));
		foreach (var (key1, key2) in t.Keys)
		{
			Console.WriteLine(t[key1, key2].ToPrettyString());
		}

		var timeResults = results.Select(result => result.Select(res => res.comparisonCount).Average());
		Console.WriteLine(timeResults.ToTabDelimitedLines());

		var comparisonResults = results.Select(result => result.Select(res => res.lookupTime).Sum());
		Console.WriteLine(comparisonResults.ToTabDelimitedLines());
	}

	public static Table<string, int, List<(double lookupTime, int comparisonCount)>> Measure(
		int lookupSampleCount,
		IEnumerable<int> dataCounts,
		IEnumerable<ISpatialLookupAlgorithm<Vector3>> algorithms)
	{
		var results = new Table<string, int, List<(double lookupTime, int comparisonCount)>>();

		const float epsilon = 0.001f;
		var stopwatch = new Stopwatch();

		foreach (int dataCount in dataCounts)
		{
			var dataGenerator = LookupSampleGenerator.Uniform3D(0, 100, 1f);
			var data = dataGenerator.DataSamples(dataCount);
			
			var samples = dataGenerator.LookupSamples(lookupSampleCount);

			Console.WriteLine(data.ToPrettyString());
			Console.WriteLine(samples.ToPrettyString());

			foreach (var algorithm in algorithms)
			{
				algorithm.AddData(data);
				results[algorithm.Name, dataCount] = new List<(double lookupTime, int comparisonCount)>();

				foreach (var sample in samples)
				{
					stopwatch.Restart();
					var result = algorithm.Contains(sample, epsilon);
					stopwatch.Stop();
					double elapsedTime = stopwatch.Elapsed.TotalSeconds;

					results[algorithm.Name, dataCount].Add((elapsedTime, result.ComparisonCount));
				}
			}
		}

		return results;
	}
#endif
}
