using DRun.Client.NetData;
using DRun.Client.Running;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System.Collections.Generic;

namespace DRun.Client.Logic.Running
{
	public class WriteRunningLogProcessor : BaseStepProcessor
	{
		private ClientRunningLog _log;
		private NFTStatParam _statData;

		public class NFTStatParam
		{
			public int heart = 0;
			public int distance = 0;
			public int stamina = 0;
			public int bonus_heart = 0;
			public int bonus_distance = 0;
		}

		private int _resultCode;

		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public int getResultCode()
		{
			return _resultCode;
		}

		public static WriteRunningLogProcessor create()
		{
			WriteRunningLogProcessor processor = new WriteRunningLogProcessor();
			processor.init();
			return processor;
		}

		public ClientRunningLog getLog()
		{
			return _log;
		}

		protected override void init()
		{
			base.init();

			RunningViewModel vm = ViewModel.Running;

			_log = vm.createRunningLog();
			
			_statData = new NFTStatParam();
			if( vm.isProMode())
			{
				_statData.heart = vm.NFTHeart;
				_statData.distance = vm.NFTDistance;
				_statData.stamina = vm.NFTFinalStamina;
				_statData.bonus_heart = vm.NFTBonusHeart;
				_statData.bonus_distance = vm.NFTBonusDistance;
			}
		}

		protected override void buildSteps()
		{
			_stepList.Add(req);
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.ProMode.WriteRunningLogReq);
			req.put("data", _log);
			req.put("heart", _statData.heart);
			req.put("distance", _statData.distance);
			req.put("stamina", _statData.stamina);
			req.put("bonus_heart", _statData.bonus_heart);
			req.put("bonus_distance", _statData.bonus_distance);
			req.put("id", _log.running_id);

			Network.call(req, ack => {
				_resultCode = ack.getResult();

				if ( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					ViewModel.updateFromPacket(ack);

					// 통계를 위해 VM에 기록한다
					List<ClientRunningLog> logList = new List<ClientRunningLog>();
					logList.Add(_log);
					ViewModel.Record.putLog(logList);

					handler(Future.succeededFuture());
				}
			});
		}
	}
}
