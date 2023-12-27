using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System.Collections.Generic;

namespace DRun.Client.Logic.Running
{
	public class QueryRunningLogProcessor : BaseLogicStepProcessor
	{
		private int _target_account_id;
		private int _begin;
		private int _count;

		private List<ClientRunningLog> _logList;

		public List<ClientRunningLog> getLogList()
		{
			return _logList;
		}

		public static QueryRunningLogProcessor create(int target_account_id,int begin,int count)
		{
			QueryRunningLogProcessor processor = new QueryRunningLogProcessor();
			processor.init(target_account_id, begin, count);
			return processor;
		}

		private void init(int target_account_id,int begin,int count)
		{
			base.init();

			_target_account_id = target_account_id;
			_begin = begin;
			_count = count;
		}

		protected override void buildSteps()
		{
			_stepList.Add(query);
		}
	
		private void query(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.ProMode.QueryRunningLogReq);
			req.put("id", _target_account_id);
			req.put("begin", _begin);
			req.put("count", _count);

			Network.call(req, ack => {
				if (ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_logList = ack.getList<ClientRunningLog>("data");
					handler(Future.succeededFuture());
				}
			});
		}




	}
}
