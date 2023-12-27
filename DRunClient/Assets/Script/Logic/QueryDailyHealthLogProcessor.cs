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
	public class QueryDailyHealthLogProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();

		private int _dataType;
		private long _beginDay;
		private long _endDay;

		private List<ClientHealthLogDaily> _list;
		private Dictionary<long, ClientHealthLogDaily> _map;

		public List<ClientHealthLogDaily> getList()
		{
			return _list;
		}

		public Dictionary<long, ClientHealthLogDaily> getMap()
		{
			return _map;
		}

		public int calcAverage(long begin,long end)
		{
			long count = 0;
			long sum = 0;
			for(long i = begin; i <= end; i++)
			{
				ClientHealthLogDaily log;
				if( _map.TryGetValue(i, out log))
				{
					count++;
					sum += log.value;
				}
			}

			if( count == 0)
			{
				return 0;
			}
			else
			{
				return (int)(sum / count);
			}
		}

		public ClientHealthLogDaily getLog(long day)
		{
			ClientHealthLogDaily log;
			if( _map.TryGetValue(day, out log))
			{
				return log;
			}
			return null;
		}

		public static QueryDailyHealthLogProcessor create(int data_type,long beginDay,long endDay)
		{
			QueryDailyHealthLogProcessor processor = new QueryDailyHealthLogProcessor();
			processor.init(data_type, beginDay, endDay);
			return processor;
		}

		private void init(int data_type,long beginDay,long endDay)
		{
			base.init();

			_dataType = data_type;
			_beginDay = beginDay;
			_endDay = endDay;
		}

		protected override void buildSteps()
		{
			_stepList.Add(query);
			_stepList.Add(buildMap);
		}

		private void query(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.HealthData.QueryDailyLogReq);
			req.put("type", _dataType);
			req.put("begin", _beginDay);
			req.put("end", _endDay);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_list = ack.getList<ClientHealthLogDaily>("data");

					handler(Future.succeededFuture());
				}
			});
		}

		private void buildMap(Handler<AsyncResult<Module.Void>> handler)
		{
			_map = new Dictionary<long, ClientHealthLogDaily>();
			foreach(ClientHealthLogDaily item in _list)
			{
				_map.Add(item.day, item);
			}

			handler(Future.succeededFuture());
		}

		
	}
}
