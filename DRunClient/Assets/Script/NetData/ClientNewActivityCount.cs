using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{ 
	public class ClientNewActivityCount
	{
		public Dictionary<int, int> countMap;

		public int getCount(int type)
		{
			int count;
			if( countMap.TryGetValue( type, out count))
			{
				return count;
			}
			return 0;
		}

		public static ClientNewActivityCount createEmpty()
		{
			ClientNewActivityCount c = new ClientNewActivityCount();
			c.countMap = new Dictionary<int, int>();
			return c;
		}

		public static class CountType
		{
			public static int moment_like = 1;
			public static int moment_comment = 2;
			public static int reward_moment_like_unclaimed = 100;
			public static int reward_moment_like = 101;
		}
	}
}
