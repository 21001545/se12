using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.ProMode
{
	public class CheckNFTLevelUpLimitProcessor : BaseLogicStepProcessor
	{
		private int _tokenID;
		private long _dayID;

		private bool _check_result;

		public bool getCheckResult()
		{
			return _check_result;
		}

		public static CheckNFTLevelUpLimitProcessor create(int tokenID)
		{
			CheckNFTLevelUpLimitProcessor step = new CheckNFTLevelUpLimitProcessor();
			step.init(tokenID);
			return step;
		}

		private void init(int tokenID)
		{
			base.init();

			_tokenID = tokenID;
			_dayID = TimeUtil.todayDayCount();
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqToServer);
		}

		private void reqToServer(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.ProMode.CheckLevelUpLimitReq);
			req.put("id", _tokenID);
			req.put("day_id", _dayID);

			Network.call(req, ack => {
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_check_result = (bool)ack.get("check_result");
					handler(Future.succeededFuture());
				}
			});
		}
	}
}
