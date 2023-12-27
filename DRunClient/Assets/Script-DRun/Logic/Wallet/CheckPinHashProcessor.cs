using DRun.Client.Logic.ProMode;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Void = Festa.Client.Module.Void;

namespace DRun.Client.Logic.Wallet
{
	public class CheckPinHashProcessor : BaseLogicStepProcessor
	{
		private string _pinHash;
		private bool _checkResult;

		public bool getCheckResult()
		{
			return _checkResult;
		}

		public static CheckPinHashProcessor create(string pinHash)
		{
			CheckPinHashProcessor processor = new CheckPinHashProcessor();
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
			_stepList.Add(req);
			_stepList.Add(getBalance);
			_stepList.Add(getFee);  // 하는 김에 이것도
			_stepList.Add(getNFT);  // ㅎㅎㅎ
			_stepList.Add(updateNFTBonus);
			_stepList.Add(applyViewModel);
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Wallet.CheckPinHashReq);
			req.put("password", _pinHash);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_checkResult = (bool)ack.get("check_result");
					handler(Future.succeededFuture());
				}
			});
		}

		private void getBalance(Handler<AsyncResult<Void>> handler)
		{
			GetBalanceProcessor step = GetBalanceProcessor.create();
			step.run(handler);
		}
		
		private void getFee(Handler<AsyncResult<Void>> handler)
		{
			GetFeeProcessor step = GetFeeProcessor.create();
			step.run( handler);
		}
		
		private void getNFT(Handler<AsyncResult<Void>> handler)
		{
			QueryNFTProcessor step = QueryNFTProcessor.create();
			step.run(handler);
		}

		private void updateNFTBonus(Handler<AsyncResult<Void>> handler)
		{
			UpdateNFTBonusProcessor step = UpdateNFTBonusProcessor.create();
			step.run(handler);
		}

		private void applyViewModel(Handler<AsyncResult<Void>> handler)
		{
			ViewModel.Wallet.WalletPinHashChecked = _checkResult;
			handler(Future.succeededFuture());
		}
	}
}
