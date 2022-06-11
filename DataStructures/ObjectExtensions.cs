// Copyright Gamelogic (c) http://www.gamelogic.co.za

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Gamelogic.Extensions
{
	/// <summary>
	/// Provides extensions for objects.
	/// </summary>
	public static class ObjectExtensions
	{
		#region Static Methods

		/// <summary>
		/// Throws a <see cref="ArgumentNullException"/> if the object is null.
		/// </summary>
		/// <param name="obj">An object to check.</param>
		/// <param name="name">The name of the variable this
		/// methods is called on.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[AssertionMethod]
		public static void ThrowIfNull(this object obj, string name)
		{
			if(obj == null) throw new ArgumentNullException(name);
		}

		/// <summary>
		/// Throws a ArgumentOutOfRange exception if the integer is negative.
		/// </summary>
		/// <param name="n">The integer to check.</param>
		/// <param name="name">The name of the variable.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[AssertionMethod]
		public static void ThrowIfNegative(this int n, string name)
		{
			if(n < 0) throw new ArgumentOutOfRangeException(name, n, "argument cannot be negative");
		}

		/// <summary>
		/// Throws a ArgumentOutOfRange exception if the float is negative.
		/// </summary>
		/// <param name="x">The float to check.</param>
		/// <param name="name">The name of the variable.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[AssertionMethod]
		public static void ThrowIfNegative(float x, string name)
		{
			if (x < 0) throw new ArgumentOutOfRangeException(name, x, "argument cannot be negative");
		}

		#endregion
	}
	
	public static class DebugEnumerableExtensions
	{
		private const string CommaSpaceSeparator = ", ";
		private const string BracketsAround = "[{0}]";
		
		public static string ToCommaSeparatedString<T>(this IEnumerable<IEnumerable<T>> list) =>
			string.Join(CommaSpaceSeparator, list.Select(ToPrettyString));
		
		public static string ToCommaSeparatedString<T>(this IEnumerable<T> list) =>
			string.Join(CommaSpaceSeparator, list.Select(ItemToString));
		
		public static string ToPrettyString<T>(this IEnumerable<T> list) => 
			string.Format(BracketsAround, list.ToCommaSeparatedString());
		
		private static string ItemToString<T>(T item) => 
			item switch 
			{
				IEnumerable enumerable => ToPrettyString(enumerable.Cast<object>()),
				_ => item.ToString()
			};
	}
}
