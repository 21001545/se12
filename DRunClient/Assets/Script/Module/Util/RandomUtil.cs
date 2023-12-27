using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.Module
{
	public static class RandomUtil
	{
		public static int intRandom(int begin,int end)
		{
			return Mathf.FloorToInt(Random.Range(begin, end + 0.9f));
		}

		public static typeT select<typeT>(IList<typeT> list)
		{
			if (list == null || list.Count == 0)
			{
				return default(typeT);
			}

			return list[intRandom(0, list.Count - 1)];
		}
	}
}
