using DRun.Client.Logic;
using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System.Collections.Generic;

namespace DRun.Client
{
	public class CheckIncompleteInvitationRewardProcessor : BaseLogicStepProcessor
	{
		private List<ClientDRNTransaction> _incompleteList;

		public List<ClientDRNTransaction> getIncompleteList()
		{
			return _incompleteList;
		}

		public static CheckIncompleteInvitationRewardProcessor create()
		{
			CheckIncompleteInvitationRewardProcessor p = new CheckIncompleteInvitationRewardProcessor();
			p.init();
			return p;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqToServer);
		}
		
		private void reqToServer(Handler<AsyncResult<Void>> handler)
		{
			_incompleteList = new List<ClientDRNTransaction>();
			MapPacket req = Network.createReq(CSMessageID.Wallet.QueryIncompleteTransactionReq);
			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					List<ClientDRNTransaction> list = ack.getList<ClientDRNTransaction>(MapPacketKey.ClientAck.drn_transaction);

					foreach(ClientDRNTransaction transaction in list)
					{
						if( transaction.type == ClientDRNTransaction.Type.wait_claim_invitation_send)
						{
							_incompleteList.Add(transaction);
						}
					}

					ViewModel.updateFromPacket(ack);
					handler(Future.succeededFuture());
				}
			});
		}


	}
}
