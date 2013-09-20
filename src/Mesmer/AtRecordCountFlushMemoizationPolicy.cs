using System;

namespace Mesmer
{

	public class AtRecordCountFlushMemoizationPolicy : IMemoizationPolicy
	{
		public AtRecordCountFlushMemoizationPolicy(int count)
		{
			_count = count;
		}

		public bool ShouldFlushCache(int records)
		{
			return records >= _count;
		}

		public void Purge(dynamic dictionary)
		{
			dictionary.Clear();
		}

		private readonly int _count;

	}
	
}
