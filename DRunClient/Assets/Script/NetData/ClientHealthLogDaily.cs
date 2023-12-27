using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientHealthLogDaily
	{
		public int data_type;
		public long day;
		public int goal;
		public long value;

		public float getAchivementRate()
		{
			if( goal == 0)
			{
				return 0;
			}
			else
			{
				return (float)value / (float)goal;
			}
		}
	}
}
