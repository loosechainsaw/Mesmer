using System;

namespace Mesmer
{

	public class NeverMemoizationPolicy : IMemoizationPolicy
	{
		public bool ShouldFlushCache(int records)
		{
			return false;
		}

		public void Purge(dynamic dictionary)
		{

		}
	}
	
}
