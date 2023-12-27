using DRun.Client.NetData;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System.Collections.Generic;

namespace DRun.Client.Logic.Record
{
	public class ReadTodayMarathonRecordProcessor : BaseLogicStepProcessor
	{
		private int _target_account_id;
		private int _running_type;
		private int _running_sub_type;
		private RecordViewModel _cacheVM;

		public static ReadTodayMarathonRecordProcessor create()
		{
			ReadTodayMarathonRecordProcessor step = new ReadTodayMarathonRecordProcessor();
			step.init();
			return step;
		}

		protected override void init()
		{
			base.init();

			_cacheVM = ViewModel.Record;

			_target_account_id = Network.getAccountID();
			_running_type = ClientRunningLogCumulation.RunningType.marathon;
			_running_sub_type = ClientRunningLogCumulation.MarathonType._sum;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqToServer);
		}

		private void reqToServer(Handler<AsyncResult<Void>> handler)
		{
			int type = ClientRunningLogCumulation.TimeType.day;
			long id = TimeUtil.todayDayCount();

			readSingleLogCumulation(type, id, handler);
		}

		private void readSingleLogCumulation(int type, long id, Handler<AsyncResult<Void>> handler)
		{
			if (_cacheVM.containsLogCumulation(_running_type, type, id))
			{
				handler(Future.succeededFuture());
				return;
			}

			MapPacket req = Network.createReq(CSMessageID.ProMode.QueryRunningLogCumulationReq);
			req.put("id", _target_account_id);
			req.put("query_type", 0);
			req.put("running_type", _running_type);
			req.put("running_sub_type", _running_sub_type);
			req.put("type", type);
			req.put("begin", id);
			req.put("end", id);

			Network.call(req, ack => {
				if (ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					List<ClientRunningLogCumulation> logList = ack.getList<ClientRunningLogCumulation>("data");
					_cacheVM.putLogCumulation(logList);
					handler(Future.succeededFuture());
				}
			});
		}
	}
}
