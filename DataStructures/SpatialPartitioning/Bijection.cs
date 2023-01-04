using System;

namespace DataStructures.SpatialPartitioning;

public static class Bijection
{
	public sealed class CompositeBijection<TIn, TMid, TOut> : IBijection<TIn, TOut>
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

	public static IBijection<TIn, TOut> Create<TIn, TOut>(Func<TIn, TOut> func, Func<TOut, TIn> inverse) 
		=> new FuncBijection<TIn, TOut>(func, inverse);

	public static IBijection<TIn, TOut> Compose<TIn, TMid, TOut>(IBijection<TIn, TMid> bijection1, IBijection<TMid, TOut> bijection2) =>
		new CompositeBijection<TIn, TMid, TOut>(bijection1, bijection2);
}
