using System;
using System.Collections.Generic;

namespace Mesmer
{

	public static class MemoiseDefaults
	{
		static MemoiseDefaults()
		{
			Value = 1000;
		}

		public static int Value{get; set;}
	}

	public static class MemoiseFuncExtensions
	{
		public static Func<TArg1, TResult> Memoise<TArg1, TResult> (this Func<TArg1, TResult> func)
		{
			return Memoise (func, new AtRecordCountFlushMemoizationPolicy (MemoiseDefaults.Value));
		}

		public static Func<TArg1, TResult> Memoise<TArg1, TResult> (this Func<TArg1, TResult> func, IMemoizationPolicy policy)
		{
			var lookup = new Dictionary<TArg1, TResult> ();
		
			return arg1 =>
			{
				EnforcePolicy (policy, lookup);

				if (!lookup.ContainsKey (arg1))
					EvaluateAndRecord (func, arg1, lookup);

				return lookup [arg1];
			};
		}

		private static void EvaluateAndRecord<TArg1, TResult> (Func<TArg1, TResult> func, TArg1 arg1, Dictionary<TArg1, TResult> lookup)
		{
			var result = func (arg1);
			lookup [arg1] = result;
		}

		private static void EnforcePolicy<TArg1, TResult> (IMemoizationPolicy policy, Dictionary<TArg1, TResult> lookup)
		{
			if (policy.ShouldFlushCache (lookup.Count))
				policy.Purge (lookup);
		}

		public static Func<TArg1, TArg2, TResult> Memoise<TArg1, TArg2, TResult> (this Func<TArg1, TArg2, TResult> func)
		{
			return Memoise (func, new AtRecordCountFlushMemoizationPolicy (MemoiseDefaults.Value));
		}

		public static Func<TArg1, TArg2, TResult> Memoise<TArg1, TArg2, TResult> (this Func<TArg1, TArg2, TResult> func, IMemoizationPolicy policy)
		{
			var lookup = new Dictionary<Tuple<TArg1, TArg2>, TResult> ();
			return (arg1, arg2) =>
			{
				EnforcePolicy (policy, lookup);

				if (!lookup.ContainsKey (Tuple.Create (arg1, arg2)))
					EvaluateAndRecord (func, arg1, arg2, lookup);

				return lookup [Tuple.Create (arg1, arg2)];
			};
		}

		private static void EvaluateAndRecord<TArg1, TArg2, TResult> (Func<TArg1, TArg2, TResult> func, TArg1 arg1, TArg2 arg2, Dictionary<Tuple<TArg1, TArg2>,TResult> lookup)
		{
			var result = func (arg1, arg2);
			lookup [Tuple.Create (arg1, arg2)] = result;
		}

		private static void EnforcePolicy<TArg1,TArg2, TResult> (IMemoizationPolicy policy, Dictionary<Tuple<TArg1,TArg2>, TResult> lookup)
		{
			if (policy.ShouldFlushCache (lookup.Count))
				policy.Purge (lookup);
		}

		public static Func<TArg1, TArg2, TArg3, TResult> Memoise<TArg1, TArg2,TArg3, TResult> (this Func<TArg1, TArg2,TArg3, TResult> func)
		{
			return Memoise (func, new AtRecordCountFlushMemoizationPolicy (MemoiseDefaults.Value));
		}

		public static Func<TArg1, TArg2, TArg3, TResult> Memoise<TArg1, TArg2,TArg3, TResult> (this Func<TArg1, TArg2,TArg3, TResult> func, IMemoizationPolicy policy)
		{
			var lookup = new Dictionary<Tuple<TArg1, TArg2, TArg3>, TResult> ();
			return (arg1, arg2, arg3) =>
			{
				EnforcePolicy (policy, lookup);

				if (!lookup.ContainsKey (Tuple.Create (arg1, arg2, arg3)))
					EvaluateAndRecord (func, arg1, arg2, arg3, lookup);

				return lookup [Tuple.Create (arg1, arg2, arg3)];
			};
		}

		private static void EvaluateAndRecord<TArg1, TArg2, TArg3, TResult> (Func<TArg1, TArg2, TArg3, TResult> func, TArg1 arg1, TArg2 arg2, TArg3 arg3, Dictionary<Tuple<TArg1, TArg2, TArg3>, TResult> lookup)
		{
			var result = func (arg1, arg2, arg3);
			lookup [Tuple.Create (arg1, arg2, arg3)] = result;
		}

		private static void EnforcePolicy<TArg1, TArg2, TArg3, TResult> (IMemoizationPolicy policy, Dictionary<Tuple<TArg1, TArg2, TArg3>, TResult> lookup)
		{
			if (policy.ShouldFlushCache (lookup.Count))
				policy.Purge (lookup);
		}
	}
}

