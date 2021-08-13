using System;
using System.Diagnostics;

namespace DataStructures
{
	public static class Program
	{
		public static void Main(string[] _)
		{
			BasicTest();
			//ProfileStringBuilding();
		}

		private static void ProfileStringBuilding()
		{
			const int elementCount = 10000;
			var list = SimpleList<int>.SimpleListEmpty;

			for (int i = 0; i < elementCount; i++)
			{
				list = list.Push(i);
			}
			
			var watch = Stopwatch.StartNew();
			var str = list.ToUnbracketedString();
			
			watch.Stop();
			
			Console.WriteLine(watch.ElapsedMilliseconds);
			//Console.WriteLine(str);
		}
		
		private static void BasicTest()
		{
			var list1 = SimpleList<int>.SimpleListEmpty.Push(1, 2, 3);
			var list2 = SimpleList<int>.SimpleListEmpty.Push(4, 5, 6);
			var list3 = list2.Concat(list1);
			
			var list4 = SimpleList<int>.SimpleListEmpty.Append(1, 2, 3);
			
			var list5 = SimpleList<int>.HughesListEmpty.Push(1, 2, 3);
			var list6 = SimpleList<int>.HughesListEmpty.Push(4, 5, 6);
			var list7 = list6.Concat(list5);
			var list8 = list7.Reverse(); 

			Console.WriteLine(list1);
			Console.WriteLine(list2);
			Console.WriteLine(list3);
			Console.WriteLine(list4);
			Console.WriteLine(list5);
			Console.WriteLine(list6);
			Console.WriteLine(list7);
			Console.WriteLine(list8);
		}
	}
}