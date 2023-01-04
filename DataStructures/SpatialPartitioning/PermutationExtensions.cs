using System.Collections.Generic;
using System.Linq;

namespace DataStructures.SpatialPartitioning;

public static class PermutationExtensions
{
	// Can be used for fluid construction. 
	public static IPermutation MulCycle(this IPermutation permutation, params int[] cycle) 
		=> permutation.Compose(ArrayPermutation.FromCycle(permutation.SetCount, cycle));

	public static IPermutation Conjugate(this IPermutation permutation, IPermutation other)
		=> other.Compose(permutation.Compose(other.Inverse()));

	//Note: performs approximately as many multiplications as the returned order.
	public static int Order(this IPermutation permutation)
	{
		int order = 1;
		var permutationPower = permutation;

		while (!permutationPower.IsIdentity())
		{
			permutationPower = permutationPower.Compose(permutation);
			order++;
		}

		return order;
	}

	public static bool IsIdentity(this IPermutation permutation)
		=> permutation
			.Elements()
			.All(permutation.KeepsFixed);

	public static int CalcCycleLength(this IPermutation permutation, int element)
	{
		int cycleLength = 1;
		int nextElement = permutation[element];
			
		while (nextElement != element)
		{
			cycleLength++;
			nextElement = permutation[nextElement];
		}

		return cycleLength;
	}

	public static bool IsCycle(this IPermutation permutation)
	{
		if (permutation.IsIdentity())
		{
			return true;
		}

		int firstNonFixedPoint = permutation
			.Elements()
			.First(permutation.Moves); //Must exist because permutation is not the identity

		return permutation.CalcCycleLength(firstNonFixedPoint) == permutation.CountNonFixedPoints();
	}

	public static int CountFixedPoints(this IPermutation permutation)
		=> permutation
			.Elements()
			.Count(permutation.KeepsFixed);

	public static int CountNonFixedPoints(this IPermutation permutation)
		=> permutation.SetCount - permutation.CountFixedPoints();

	public static bool IsDerangement(this IPermutation permutation) 
		=> !permutation.HasFixedPoint(); //We could also use permutation.CountFixedPoints() == 0, but this is slightly more efficient. 

	public static IList<T> Permute<T>(this IList<T> list, IPermutation permutation)
		=> list.Select((_, index) => list[permutation[index]]).ToList();

	public static IEnumerable<int> Apply(this IEnumerable<int> list, IPermutation permute)
		=> list.Select(element => permute[element]);

	public static IEnumerable<T> Apply<T>(this IEnumerable<T> list, IPermutation permute, IBijection<int, T> bijection)
	{
		var inverse = bijection.Inverse();
		return list.Select(element => bijection[permute[inverse[element]]]);
	}

	public static IEnumerable<IList<T>> GeneratePermutations<T>(this IList<T> list) => 
		ArrayPermutation
			.GeneratePermutations(list.Count)
			.Select(list.Permute);

	//FromPermutation simply returns the original permutation casted if it is already of the right type
	public static int LexicographicalRank(this IPermutation permutation) =>
		ArrayPermutation
			.FromPermutation(permutation)
			.LexicographicalRank();

	/*
		This may be useful enough to make public outright.
		
		On the other hand, we may find though that using this method generates too much garbage,
		and that we are better off using old-style for loops in this class and outside it. 
	*/
	[Private(ExposedFor.Testing)]
	public static IEnumerable<int> Elements(this IPermutation permutation)
		=> Enumerable.Range(0, permutation.SetCount);
	
	private static bool KeepsFixed(this IPermutation permutation, int element)
		=> (permutation[element] == element);
	
	private static bool Moves(this IPermutation permutation, int element)
		=> !permutation.KeepsFixed(element);
	
	private static bool HasFixedPoint(this IPermutation permutation) 
		=> permutation
			.Elements()
			.Any(permutation.KeepsFixed);
}
