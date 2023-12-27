using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System.Collections.Generic;

namespace DRun.Client.Logic.Account
{
	public class AcceptInvitationCodeProcessor : BaseLogicStepProcessor
	{
		private string _code;

		private ClientDRNTransaction _transaction;
		private int _errorCode;

		public ClientDRNTransaction getTransaction()
		{
			return _transaction;
		}

		public int getErrorCode()
		{
			return _errorCode;
		}

		public static AcceptInvitationCodeProcessor create(string code)
		{
			AcceptInvitationCodeProcessor p = new AcceptInvitationCodeProcessor();
			p.init(code);
			return p;
		}

		private void init(string code)
		{
			base.init();
			_code = code.ToUpper();
		}

		protected override void buildSteps()
		{
			_stepList.Add(req);
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Account.AcceptInvitationCodeReq);
			req.put("code", _code);
			req.put("device_id", GlobalConfig.getDeviceID());

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					_errorCode = ack.getResult();
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					getTransaction(ack);

					ViewModel.updateFromPacket(ack);
					handler(Future.succeededFuture());
				}
			});
		}

		private void getTransaction(MapPacket ack)
		{
			List<ClientDRNTransaction> list  = ack.getList<ClientDRNTransaction>(MapPacketKey.ClientAck.drn_transaction);
			foreach(ClientDRNTransaction transaction in list)
			{
				if( transaction.type == ClientDRNTransaction.Type.invitation_receive)
				{
					_transaction = transaction;
				}
			}
		}

	}
}
