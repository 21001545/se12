using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System.Collections.Generic;

namespace DRun.Client.Logic.Running
{
	public class QueryRunningLogCumulationProcessor : BaseLogicStepProcessor
	{
		private int _query_type;
		private int _target_account_id;
		private int _running_type;
		private int _running_sub_type;
		private int _type;
		private long _id_begin;
		private long _id_end;
		private int _count;

		private Dictionary<int, ClientRunningLogCumulation> _dataMap;

		public Dictionary<int, ClientRunningLogCumulation> getDataMap()
		{
			return _dataMap;
		}

		public static QueryRunningLogCumulationProcessor create(int query_type,int target_account_id,int running_type,int running_sub_type,int type, long id_begin, long id_end)
		{
			QueryRunningLogCumulationProcessor processor = new QueryRunningLogCumulationProcessor();
			processor.init(query_type,target_account_id, running_type, running_sub_type, type, id_begin, id_end);
			return processor;
		}

		private void init(int query_type,int target_account_id, int running_type, int running_sub_type, int type, long id_begin, long id_end)
		{
			base.init();

			_query_type = query_type;
			_target_account_id = target_account_id;
			_running_type = running_type;
			_running_sub_type = running_sub_type;
			_type = type;
			_id_begin = id_begin;
			_id_end = id_end;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryFromServer);
		}

		private void queryFromServer(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.ProMode.QueryRunningLogCumulationReq);
			req.put("id", _target_account_id);
			req.put("query_type", _query_type);
			req.put("running_type", _running_type);
			req.put("running_sub_type", _running_sub_type);
			req.put("type", _type);

			if( _query_type == 0)
			{
				req.put("begin", _id_begin);
				req.put("end", _id_end);
			}
			else
			{
				req.put("count", _count);
			}

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_dataMap = ack.getDictionary<int, ClientRunningLogCumulation>("data");
					handler(Future.succeededFuture());
				}
			});
		}
	}
}
