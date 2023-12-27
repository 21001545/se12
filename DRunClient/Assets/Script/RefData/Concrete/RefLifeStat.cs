using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.RefData
{
	public class RefLifeStat : RefData
	{
		public int id;
		public int max_level;

		// UI쪽에서 쓸일이 있을 수도
		public const int STR = 3;
		public const int ENT = 4;
		public const int REL = 5;
		public const int EXC = 6;
		public const int INT = 7;
		public const int EMO = 8;
	}
}
