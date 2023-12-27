using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientTripCheerable
	{
		public int account_id;
		public int trip_id;

		[SerializeOption(SerializeOption.NONE)]
		public bool _isFollow = false;      // 내친구인가?

		[SerializeOption(SerializeOption.NONE)]
		public bool _isAlreadyCheered = false;	// 이미 응원하기를 보냈는지

		public static ClientTripCheerable create(int account_id,int trip_id)
		{
			ClientTripCheerable c = new ClientTripCheerable();
			c.account_id = account_id;
			c.trip_id = trip_id;
			return c;
		}
	}
}
