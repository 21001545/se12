using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.RefData
{
	public class RefMomentLifeLog : RefData
	{
		public int id;
		public int type;
		public int order;
		public string icon;

		public const int ID_Walking = 101;
	}
}
