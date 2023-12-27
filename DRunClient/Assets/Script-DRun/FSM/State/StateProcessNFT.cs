using DRun.Client.Logic.ProMode;
using DRun.Client.Logic.Wallet;
using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.FSM;
using System.Collections.Generic;

namespace DRun.Client
{
	public class StateProcessNFT : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.process_nft;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("process pfp...", 96);

			if (_viewModel.Wallet.Wallet == null)
			{
				changeToNextState();
			}
			else
			{
				List<BaseStepProcessor.StepProcessor> stepList = new List<BaseStepProcessor.StepProcessor>();
				stepList.Add(queryNFTList);
				stepList.Add(autoEquipNFT);
				stepList.Add(updateNFTBonus);

				BaseStepProcessor.runSteps(0, stepList, false, result => {
					changeToNextState();
				});
			}
		}

		private void queryNFTList(Handler<AsyncResult<Void>> handler)
		{
			QueryNFTProcessor step = QueryNFTProcessor.create();
			step.run(handler);
		}

		private void autoEquipNFT(Handler<AsyncResult<Void>> handler)
		{
			List<ClientNFTItem> list = _viewModel.Wallet.getNFTItemList();
			if (_viewModel.ProMode.EquipedNFTItem != null || list.Count == 0 )
			{
				handler(Future.succeededFuture());
				return;
			}

			ClientNFTItem top_item = list[0];

			EquipNFTProcessor step = EquipNFTProcessor.create(top_item.token_id);
			step.run(handler);
		}

		private void updateNFTBonus(Handler<AsyncResult<Void>> handler)
		{
			UpdateNFTBonusProcessor step = UpdateNFTBonusProcessor.create();
			step.run(handler);
		}

	}
}
