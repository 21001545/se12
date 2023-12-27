using DRun.Client.Logic;
using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System.Collections.Generic;
using Void = Festa.Client.Module.Void;

namespace Drun.Client.Logic.Wallet
{
	public class ProcessIncompleteTransactionProcessor : BaseLogicStepProcessor
	{
		private List<ClientDRNTransaction> _incompleteList;

		public List<ClientDRNTransaction> getIncompleteList()
		{
			return _incompleteList;
		}

		public static ProcessIncompleteTransactionProcessor create()
		{
			ProcessIncompleteTransactionProcessor processor = new ProcessIncompleteTransactionProcessor();
			processor.init();
			return processor;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryIncomplete);
			_stepList.Add(processIncomplete);
		}

		private void queryIncomplete(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Wallet.QueryIncompleteTransactionReq);
			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_incompleteList = ack.getList<ClientDRNTransaction>(MapPacketKey.ClientAck.drn_transaction);

					ViewModel.updateFromPacket(ack);
					handler(Future.succeededFuture());
				}
			});
		}

		private void processIncomplete(Handler<AsyncResult<Void>> handler)
		{
			processIncompleteIter(_incompleteList.GetEnumerator(), handler);
		}

		private void processIncompleteIter( IEnumerator<ClientDRNTransaction> it, Handler<AsyncResult<Void>> handler )
		{
			if( it.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ClientDRNTransaction transaction = it.Current;

			// 2023.02.06 초대 코드 보상은 연출을 위해 다른 곳에서 처리함
			if( transaction.type == ClientDRNTransaction.Type.wait_claim_invitation_send)
			{
				processIncompleteIter(it, handler);
				return;
			}

			MapPacket req = Network.createReq(CSMessageID.Wallet.ProcessIncompleteTransactionReq);
			req.put("id", transaction.transaction_id);
			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					ViewModel.updateFromPacket(ack);
					processIncompleteIter(it, handler);
				}
			});
		}
	}
}
