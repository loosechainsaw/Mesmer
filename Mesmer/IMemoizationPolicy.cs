using System;

namespace Mesmer
{
	public interface IMemoizationPolicy
	{
		bool ShouldFlushCache(int records);
		void Purge(dynamic dictionary);
	}
	
}
