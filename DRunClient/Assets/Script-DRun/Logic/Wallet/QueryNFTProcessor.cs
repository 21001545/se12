using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.Wallet
{
	public class QueryNFTProcessor : BaseLogicStepProcessor
	{
		public static QueryNFTProcessor create()
		{
			QueryNFTProcessor processor = new QueryNFTProcessor();
			processor.init();
			return processor;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqToServer);
		}

		private void reqToServer(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Wallet.QueryNFTReq);
			req.put("timezone_offset", TimeUtil.timezoneOffset());
			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					// 전체 갱신
					ViewModel.Wallet.NFTITemMap.Clear();
					ViewModel.Wallet.NFTWithdrawExternalMap.Clear();
					ViewModel.updateFromPacket(ack);
					handler(Future.succeededFuture());
				}
			});
		}
	}
}
