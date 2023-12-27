using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Logic
{
	public class QueryTripHistoryProcessor : BaseStepProcessor
	{
		private int _targetAccountID;
		private int _begin;
		private int _count;

		private List<ClientTripLog> _logList;

		public List<ClientTripLog> getLogList()
		{
			return _logList;
		}

		protected ClientNetwork Network => ClientMain.instance.getNetwork();

		public static QueryTripHistoryProcessor create(int targetAccountID,int begin,int count)
		{
			QueryTripHistoryProcessor p = new QueryTripHistoryProcessor();
			p.init(targetAccountID, begin, count);
			return p;
		}

		private void init(int targetAccountID,int begin,int count)
		{
			base.init();

			_targetAccountID = targetAccountID;
			_begin = begin;
			_count = count;
		}

		// cache도 구현해야 될것 같은데 나중에 하자

		protected override void buildSteps()
		{
			_stepList.Add(query);
		}

		private void query(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Trip.QueryTripListReq);
			req.put("target", _targetAccountID);
			req.put("begin", _begin);
			req.put("count", _count);

			Network.call(req, ack => {
				if (ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_logList = ack.getList<ClientTripLog>(MapPacketKey.ClientAck.trip_log);

					handler(Future.succeededFuture());
				}
			});
		}

		//// 테스트 코드
		//MapPacket reqHistory = Network.createReq(CSMessageID.Trip.QueryTripListReq);
		//reqHistory.put("target", 9);
		//reqHistory.put("begin", 3);
		//reqHistory.put("count", 10);

		//Network.call(reqHistory, ackHistory =>
		//{
		//	if (ackHistory.getResult() == ResultCode.ok)
		//	{
		//		ClientTripLog log = ackHistory.getList<ClientTripLog>(MapPacketKey.ClientAck.trip_log)[0];

		//		UITripEndResult.getInstance().setup(log);
		//		UITripEndResult.getInstance().open();
		//		close();
		//	}
		//});
	}
}
