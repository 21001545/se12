using DRun.Client.NetData;
using DRun.Client.Logic.BasicMode;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.FSM;
using DRun.Client.Running;
using DRun.Client.Logic.ProMode;
using Assets.Script_DRun.Logic.ProMode;
using Festa.Client.Module.Net;
using Festa.Client.RefData;
using Festa.Client.Module.UI;

namespace DRun.Client
{
	public class StateRun : ClientStateBehaviour
	{
		private ClientHealthManager _health;
		//private ClientLocationManager _location;
		private AbstractGPSTracker _gpsTracker;
		private RunningRecorder _runningRecorder;

		// 주간 보상 자동 받기
		private IntervalTimer _weeklyRewardTimer;
		// 프로모드 자동 충전
		private IntervalTimer _proModeStatTimer;
		// 프로모드 보너스 자동 충전
		private IntervalTimer _proModeBonusStatTimer;
		// 세션 체크
		private IntervalTimer _checkSessionTimer;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override int getType()
		{
			return ClientStateType.run;
		}

		protected override void init()
		{
			base.init();
			_weeklyRewardTimer = IntervalTimer.create(1.0f, false, true);
			_proModeStatTimer = IntervalTimer.create(3.0f, false, false);
			_proModeBonusStatTimer = IntervalTimer.create(3.0f, false, false);
			_checkSessionTimer = IntervalTimer.create(10, true, false);
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			_health = ClientMain.instance.getHealth();
			_gpsTracker = ClientMain.instance.getGPSTracker();
			_runningRecorder = ClientMain.instance.getRunningRecorder();
			//_location = ClientMain.instance.getLocation();
		}

		public override void update()
		{
			_health.update();
			_gpsTracker.update();
			_runningRecorder.update();
			
			//_location.update();

			checkWeeklyReward();
			checkProModeStat();
			checkProModeBonusStat();
			checkSession();
		}

		private void checkWeeklyReward()
		{
			if( _weeklyRewardTimer.update() == false)
			{
				return;
			}

			isProModeRunning();

			ClientBasicWeeklyReward reward = ViewModel.BasicMode.getClaimableWeeklyReward();
			if( reward == null)
			{
				_weeklyRewardTimer.setNext();
				return;
			}

			ClaimWeeklyRewardProcessor step = ClaimWeeklyRewardProcessor.create(reward.week_id);
			step.run(result => {
				_weeklyRewardTimer.setNext();
			});
		}

		private void checkProModeStat()
		{
			if( _proModeStatTimer.update() == false)
			{
				return;
			}

			// 일단 장착중인것만 리필해주자
			ClientNFTItem nftItem = ViewModel.ProMode.EquipedNFTItem;
			if(nftItem == null)
			{
				_proModeStatTimer.setNext();
				return;
			}

			if( System.DateTime.UtcNow < nftItem.next_refill_time)
			{
				_proModeStatTimer.setNext();
				return;
			}

			// 런닝 중에는 않됨
			if(isProModeRunning())
			{
				_proModeBonusStatTimer.setNext();
				return;
			}

			RefillStatProcessor step = RefillStatProcessor.create();
			step.run(result => {
				_proModeStatTimer.setNext();
			});
		}

		private void checkProModeBonusStat()
		{
			if( _proModeBonusStatTimer.update() == false)
			{
				return;
			}

			ClientNFTBonus bonus = ViewModel.ProMode.NFTBonus;
			if( bonus.hasBonus() == false)
			{
				_proModeBonusStatTimer.setNext();
				return;
			}

			if( System.DateTime.UtcNow < bonus.next_refill_time)
			{
				_proModeBonusStatTimer.setNext();
				return;
			}

			// 런닝 중에는 않됨
			if (isProModeRunning())
			{
				_proModeBonusStatTimer.setNext();
				return;
			}

			RefillBonusStateProcessor step = RefillBonusStateProcessor.create();
			step.run(result => {
				_proModeBonusStatTimer.setNext();
			});
			
		}

		private void checkSession()
		{
			if( _checkSessionTimer.update() == false)
			{
				return;
			}

			MapPacket req = _network.createReq(CSMessageID.Account.CheckSessionReq);
			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					onCheckSessionFail(ack);
				}
			});
		}

		private void onCheckSessionFail(MapPacket ack)
		{
			if( ack.getResult() != ResultCode.error_invalid_token)
			{
				return;
			}

			string message = StringCollection.get("ErrorPopup.network", 0);
			UIPopup.spawnOK(message, () => {
				UIManager.getInstance().closeAllActivePanels();
				UILoading.getInstance().open();
				_owner.changeState(ClientStateType.select_server);
			});
		}

		private bool isProModeRunning()
		{
			int status = ViewModel.Running.Status;
			return status != StateType.none && ViewModel.Running.RunningType == ClientRunningLogCumulation.RunningType.promode;
		}
	}
}
