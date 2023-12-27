using DRun.Client.Logic.BasicMode;
using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.FSM;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client
{
	public class StateBecomeActive : ClientStateBehaviour
	{
		private UIBecomeActive _uiBecomeActive;

		public override int getType()
		{
			return ClientStateType.become_active;
		}

		/*
			런닝 시작한 뒤로 앱을 장시간 내려놓은 다음
			앱을 다시 올릴때 checkSession등에서 실패하게 되면 런닝기록이 망실될 수 있다

				=> 재로그인하는 과정에서 디스크에 저장된 옛날 런닝 기록을 다시 로딩해서 생긴 문제

			따라서 세션 체크 하기 전에 일단 위치 정보등은 모두 잘 수집할 수 있도록 조치해주어야 한다
		*/


		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			_uiBecomeActive = UIBecomeActive.spawn();

			List<BaseStepProcessor.StepProcessor> stepList = new List<BaseStepProcessor.StepProcessor>();
			//stepList.Add(sleepTest);
			stepList.Add(updateRunning);
			stepList.Add(checkSession);             // 세션 체크
			stepList.Add(sendHealth);               // 걸음 수 보내기
			stepList.Add(expireDailyStepReward);    // 일일 보상 만료 처리
			stepList.Add(claimWeeklyReward);        // 주간 보상 수령

			BaseStepProcessor.runSteps(0, stepList, false, result => {
				_uiBecomeActive.close();

				if (result.succeeded())
				{
					_owner.changeState(ClientStateType.run);
				}
			});
		}

		//private void sleepTest(Handler<AsyncResult<Void>> handler)
		//{
		//	ClientMain.instance.StartCoroutine(_sleepTest(handler));
		//}

		//private IEnumerator _sleepTest(Handler<AsyncResult<Void>> handler)
		//{
		//	yield return new WaitForSeconds(2.0f);

		//	handler(Future.succeededFuture());
		//}

		private void updateRunning(Handler<AsyncResult<Void>> handler)
		{
			ClientMain.instance.getRunningRecorder().onBecomeActive(handler);
		}

		private void checkSession(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = _network.createReq(CSMessageID.Account.CheckSessionReq);
			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					onCheckSessionFail(ack);
					handler(Future.failedFuture(new System.Exception("check session fail")));
				}
				else
				{
					handler(Future.succeededFuture());
				}
			});			
		}

		private void onCheckSessionFail(MapPacket ack)
		{
			RefStringCollection sc = GlobalRefDataContainer.getStringCollection();

			string message;

			if (ack.getResult() == ResultCode.error_invalid_token)
			{
				message = sc.get("ErrorPopup.network", 0);
			}
			else
			{
				message = sc.get("ErrorPopup.network", 1);
			}

			UIPopup.spawnOK(message, () => {

				UIManager.getInstance().closeAllActivePanels();

				UILoading.getInstance().open();

				_owner.changeState(ClientStateType.select_server);
			});
		}

		private void sendHealth(Handler<AsyncResult<Void>> handler)
		{
			ClientMain.instance.getHealth().initialQuery(() => {
				handler(Future.succeededFuture());
			});
		}

		private void expireDailyStepReward(Handler<AsyncResult<Void>> handler)
		{
			ExpireDailyRewardProcessor step = ExpireDailyRewardProcessor.create();
			step.run(handler);
		}

		private void claimWeeklyReward(Handler<AsyncResult<Void>> handler)
		{
			ClientBasicWeeklyReward reward = _viewModel.BasicMode.getClaimableWeeklyReward();
			if( reward == null)
			{
				handler(Future.succeededFuture());
				return;
			}

			ClaimWeeklyRewardProcessor step = ClaimWeeklyRewardProcessor.create(reward.week_id);
			step.run(handler);
		}
	}
}
