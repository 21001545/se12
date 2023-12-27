using DRun.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRun.Client.ViewModel
{
	public struct RunningLogCumulationRangeKey
	{
		public int running_type;
		public int running_sub_type;
		public int type;
		public long id_begin;
		public long id_end;

		public override int GetHashCode()
		{
			return running_type * 10 + type;
		}

		public override bool Equals(object obj)
		{
			return base.Equals((RunningLogCumulationRangeKey)obj);
		}

		public bool Equals(RunningLogCumulationRangeKey key)
		{
			return running_type == key.running_type &&
				running_sub_type == key.running_sub_type &&
				type == key.type &&
				id_begin == key.id_begin &&
				id_end == key.id_end;
		}

		public static RunningLogCumulationRangeKey create(int running_type, int running_sub_type, int type, long id_begin,long id_end)
		{
			RunningLogCumulationRangeKey key = new RunningLogCumulationRangeKey();
			key.running_type = running_type;
			key.running_sub_type = running_sub_type;
			key.type = type;
			key.id_begin = id_begin;
			key.id_end = id_end;
			return key;
		}
	}
}
