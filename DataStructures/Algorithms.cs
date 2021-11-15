using System.Collections.Generic;
using System.Linq;

namespace DataStructures
{
	public static class Algorithms
	{
		public static void Swap<T>(ref T a, ref T b)
		{
			var tmp = a;
			a = b;
			b = tmp;
		}

		public static void SwapAt<T>(IList<T> list, int a, int b)
		{
			var tmp = list[a];
			list[a] = list[b];
			list[b] = tmp;
		}

		public static IList<T> Range<T>(IList<T> list, int start, int end)
		{
			var newList = new List<T>(list.Count);

			for (int i = start; i < end; i++)
			{
				newList.Add(list[i]);
			}

			return newList;
		}
	}
}