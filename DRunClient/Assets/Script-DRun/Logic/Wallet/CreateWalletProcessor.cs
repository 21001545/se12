using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Void = Festa.Client.Module.Void;

namespace DRun.Client.Logic.Wallet
{
	public class CreateWalletProcessor : BaseLogicStepProcessor
	{
		private string _pinHash;

		public static CreateWalletProcessor create(string pinHash)
		{
			CreateWalletProcessor processor = new CreateWalletProcessor();
			processor.init(pinHash);
			return processor;
		}

		private void init(string pinHash)
		{
			base.init();
			_pinHash = pinHash;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqToServer);
			_stepList.Add(getBalance);
		}

		private void reqToServer(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Wallet.CreateWalletReq);
			req.put("password", _pinHash);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					ViewModel.updateFromPacket(ack);

					// 생성한 세션에서는 비밀번호 물어 보지 말자
					ViewModel.Wallet.WalletPinHashChecked = true;
					handler(Future.succeededFuture());
				}
			});
		}

		private void getBalance(Handler<AsyncResult<Void>> handler)
		{
			GetBalanceProcessor step = GetBalanceProcessor.create();
			step.run(handler);
		}
	}
}
