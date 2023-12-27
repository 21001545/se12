using DRun.Client.Logic;
using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System.Collections.Generic;

namespace Assets.Script_DRun.Logic.Account
{
	public class ProcessIncopmleteInvitationRewardProcessor : BaseLogicStepProcessor
	{
		List<ClientDRNTransaction> _incompleteList;
		long _rewardAmount;

		public long getRewardAmount()
		{
			return _rewardAmount;
		}

		public static ProcessIncopmleteInvitationRewardProcessor create(List<ClientDRNTransaction> incompleteList)
		{
			ProcessIncopmleteInvitationRewardProcessor p = new ProcessIncopmleteInvitationRewardProcessor();
			p.init(incompleteList);
			return p;
		}

		private void init(List<ClientDRNTransaction> incompleteList)
		{
			base.init();
			_incompleteList = incompleteList;
			_rewardAmount = 0;
		}

		protected override void buildSteps()
		{
			_stepList.Add(processIncopmlete);
		}

		private void processIncopmlete(Handler<AsyncResult<Void>> handler)
		{
			processIncompleteIter(_incompleteList.GetEnumerator(), handler);
		}

		private void processIncompleteIter(IEnumerator<ClientDRNTransaction> it,Handler<AsyncResult<Void>> handler)
		{
			if( it.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ClientDRNTransaction transaction = it.Current;

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
					recordEvent(ack);
					processIncompleteIter(it, handler);
				}
			});
		}

		private void recordEvent(MapPacket ack)
		{
			List<ClientDRNTransaction> list = ack.getList<ClientDRNTransaction>(MapPacketKey.ClientAck.drn_transaction);
			foreach(ClientDRNTransaction transaction in list)
			{
				if( transaction.type == ClientDRNTransaction.Type.invitation_send)
				{
					_rewardAmount += transaction.delta;
				}
			}
		}

	}
}
