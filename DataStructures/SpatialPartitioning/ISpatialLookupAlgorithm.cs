using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DataStructures.SpatialPartitioning;

public interface ISpatialLookupAlgorithm<in TItem>
{
	public LookupResult Contains(TItem item, float epsilon);
	public void AddData(IEnumerable<TItem> data);
	public string Name { get; }
}

public static class Algorithm
{
	private class SequentialLookupAlgorithm : ISpatialLookupAlgorithm<Vector3>
	{
		private List<Vector3> data;

		public LookupResult Contains(Vector3 item, float epsilon)
		{
			int compareCount = 0;
			foreach (var x in data)
			{
				compareCount++;
				if ((x - item).Length() < epsilon)
				{
					return new LookupResult
					{
						Result = true,
						ComparisonCount = compareCount
					};
				}
			}

			return new LookupResult
			{
				Result = false,
				ComparisonCount = compareCount
			};
		}

		public void AddData(IEnumerable<Vector3> data)
		{
			this.data = data.ToList();
		}

		public string Name => nameof(SequentialLookup);
	}

	public static ISpatialLookupAlgorithm<Vector3> SequentialLookup => new SequentialLookupAlgorithm();
}
