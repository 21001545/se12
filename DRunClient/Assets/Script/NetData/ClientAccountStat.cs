using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientAccountStat
	{
		public int type;
		public int level;
		public int exp;
	
		public class StatType
		{
			public const int STR = 3;
			public const int ENT = 4;
			public const int REL = 5;
			public const int EXC = 6;
			public const int INT = 7;
			public const int EMO = 8;
		}
	}
}
