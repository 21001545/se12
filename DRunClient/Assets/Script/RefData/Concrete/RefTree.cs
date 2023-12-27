using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.RefData
{
	public class RefTree : RefData
	{
		public int id;
		public int production_stepcount;
		public int production_coin;
		public int available_stepcount_min;
		public int available_stepcount_max;
		public int available_duration;
		public string resource;
		public string shop_thumbnail;
		public string shop_detail_thumbnail;
	}
}
