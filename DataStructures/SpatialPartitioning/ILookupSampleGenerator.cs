using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Gamelogic.Extensions.Algorithms;

namespace DataStructures.SpatialPartitioning;

public interface ILookupSampleGenerator<TItem>
{
	public IEnumerable<TItem> DataSamples(int n);
	public IEnumerable<TItem> LookupSamples(int n);
}

public static class LookupSampleGenerator
{
	private sealed class Uniform : ILookupSampleGenerator<Vector3>
	{
		/*
			We use two generators, so that we can still get consistent 
			results regardless of the order in which data and lookup 
			samples are requested.
		*/ 
		private readonly IGenerator<Vector3> dataGenerator;
		private readonly IGenerator<Vector3> lookupSampleGenerator;
		private readonly IGenerator<float> uniformRandomFloat;

		public Uniform(int seed, float range, float hitRate)
		{
			dataGenerator = Generator
				.UniformRandomFloat(seed)
				.Select(x => x * 2 * range - range)
				.Group(3)
				.Select(list => new Vector3(list[0], list[1], list[2]));
			
			var nonPresentSampleGenerator = Generator
				.UniformRandomFloat(seed + 1000)
				.Select(x => x * 2 * range - range)
				.Group(3)
				.Select(list => new Vector3(list[0], list[1], list[2]));

			var selector = Generator
				.RandomBoolGenerator(hitRate)
				.Select(hit => hit ? 1: 0);

			lookupSampleGenerator = Generator.Choose(new[] { nonPresentSampleGenerator, dataGenerator }, selector);

			uniformRandomFloat = Generator.UniformRandomFloat(seed + 2000);
		}

		public IEnumerable<Vector3> DataSamples(int n) => dataGenerator.Next(n);

		public IEnumerable<Vector3> LookupSamples(int n)
		{
			var list = lookupSampleGenerator
				.Next(n)
				.ToList();
				
				list.Shuffle(uniformRandomFloat);

				return list;
		}
	}

	public static ILookupSampleGenerator<Vector3> Uniform3D(int seed, float range, float hitRate) => new Uniform(seed, range, hitRate);
	
	/// <summary>
	/// Shuffles a list.
	/// </summary>
	/// <typeparam name="T">The type of items in the list.</typeparam>
	/// <param name="list">The list to shuffle.</param>
	/// <param name="uniformRng">The random generator to use.</param>
	public static void Shuffle<T>(this IList<T> list, IGenerator<float> uniformRng)
	{
		var n = list.Count;
		var gen = uniformRng.CloneAndRestart();

		while (n > 1)
		{
			n--;
			var k = GLMath.FloorToInt(gen.Next() * (n + 1));

			//Can happen when the generator produces exactly 1
			if (k > n)
			{
				k = n;
			}

			(list[k], list[n]) = (list[n], list[k]);
		}
	}
}
