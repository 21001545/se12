using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.Wallet
{
	public class QueryTransactionProcessor : BaseLogicStepProcessor
	{
		private int _begin;
		private int _count;

		public static QueryTransactionProcessor create(int begin,int count)
		{
			QueryTransactionProcessor processor= new QueryTransactionProcessor();
			processor.init(begin, count);
			return processor;
		}

		private void init(int begin, int count)
		{
			base.init();

			_begin = begin;
			_count = count;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqToServer);
		}

		private void reqToServer(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Wallet.QueryTransactionReq);
			req.put("begin", _begin);
			req.put("count", _count);
			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					ViewModel.updateFromPacket(ack);
					handler(Future.succeededFuture());
				}
			});
		}
	}
}
