using System;

namespace DataStructures.SpatialPartitioning;

/// <summary>
/// Provides methods for creating instances of <see cref="IBijection{TIn,TOut}"/> and extension methods on <see cref="IBijection{TIn,TOut}"/>.
/// </summary>
public static class Bijection
{
	/// <summary>
	/// Represents a bijection composed from two other bijections. 
	/// </summary>
	/// <typeparam name="TMid">The input type of one bijection and output type of the other bijection.</typeparam>
	private sealed class CompositeBijection<TIn, TMid, TOut> : IBijection<TIn, TOut>
	{
		private readonly IBijection<TIn, TMid> bijection1;
		private readonly IBijection<TMid, TOut> bijection2;

		public CompositeBijection(IBijection<TIn, TMid> bijection1, IBijection<TMid, TOut> bijection2)
		{
			this.bijection1 = bijection1;
			this.bijection2 = bijection2;
		}
		
		public TOut this[TIn element] => bijection2[bijection1[element]];

		public IBijection<TOut, TIn> Inverse() => new CompositeBijection<TOut, TMid, TIn>(bijection2.Inverse(), bijection1.Inverse());
		public IBijection<TIn, T> Compose<T>(IBijection<TOut, T> other) => new CompositeBijection<TIn, TOut, T>(this, other);
	}
	
	/*
		Represents a bijection (and it's inverse) made from a Funcs.
	*/
	private sealed class FuncBijection<TIn, TOut> : IBijection<TIn, TOut>
	{
		private readonly Func<TIn, TOut> func;
		private readonly FuncBijection<TOut, TIn> inverse;
		
		public TOut this[TIn element] => func(element);
		
		public FuncBijection(Func<TIn, TOut> func, Func<TOut, TIn> inverse)
		{
			this.func = func;
			this.inverse = new FuncBijection<TOut, TIn>(inverse, this);
		}
		
		private FuncBijection(Func<TIn, TOut> func, FuncBijection<TOut, TIn> inverse)
		{
			this.func = func;
			this.inverse = inverse;
		}
		
		public IBijection<TOut, TIn> Inverse() => inverse;
		
		public IBijection<TIn, T> Compose<T>(IBijection<TOut, T> other) 
			=> new CompositeBijection<TIn, TOut, T>(this, other);
	}
	
	/// <summary>
	/// Creates a <see cref="IBijection{TIn,TOut}"/> from a <see cref="Func{TResult}"/> and its inverse, also a <see cref="Func{TResult}"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// It is up to the user to ensure that inverse is indeed the inverse of func, and that the bijection and it's inverse is used
	/// within the right range. For example, the following is a valid bijection only if we restrict the forward use to [-Math.Pi, Math.Pi]
	/// and the inverse use to [-1, 1]:
	/// <![CDATA[
	/// Bijection.Create( x => Math.Sin(x), x => Math.Asin(x)) 
	/// ]]>
	/// </para>
	/// <para>
	///The following is an identity that in general should hold:
	/// <![CDATA[
	/// x == bijection.Inverse()[bijection[x]];
	/// ]]>
	/// However, it may not hold if the bijection involves floating point numbers. (It would be approximately true, though.)
	/// </para>
	/// </remarks>
	public static IBijection<TIn, TOut> Create<TIn, TOut>(Func<TIn, TOut> func, Func<TOut, TIn> inverse) 
		=> new FuncBijection<TIn, TOut>(func, inverse);
	
	/// <summary>
	/// Compose a bijection with another to form a new bijection.
	/// </summary>
	/// <remarks>
	/// The following identities hold (although only approximately if the bijections involve floating point operations):
	/// <![CDATA[
	/// var newBijection = bijection1.Compose(bijection2);
	/// newBijection[x] == bijection2[bijection1[x]];
	/// newBijection.Inverse()[x] == bijection1.Inverse()[bijection2.Inverse()[x]];
	/// ]]>
	/// </remarks>
	public static IBijection<TIn, TOut> Compose<TIn, TMid, TOut>(this IBijection<TIn, TMid> bijection1, IBijection<TMid, TOut> bijection2) =>
		new CompositeBijection<TIn, TMid, TOut>(bijection1, bijection2);
}
